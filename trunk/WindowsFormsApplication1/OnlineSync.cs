using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GameAnywhere
{
    class OnlineSync: Sync
    {
        //Sync direction
        public static readonly int Uninitialize = 0; //Default
        public static readonly int WebToCom = 4;
        public static readonly int ComToWeb = 5;
        public static readonly int ExternalAndWeb = 6;

        //Data member
        private int syncDirection = Uninitialize;
        private List<SyncAction> syncActionList;
        private User currentUser;
        private string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");

        //S3 Objects
        private Storage s3;

        public OnlineSync(int direction, List<Game> gameList, User user)
        {
            syncDirection = direction;
            installedGameList = gameList;
            currentUser = user;
            s3 = new Storage();
        }
        public override List<SyncAction> SynchronizeGames(List<SyncAction> list)
        {
            syncActionList = list;
            foreach (SyncAction sa in syncActionList)
            {
                Debug.Assert(sa.MyGame != null);
                Debug.Assert(sa.MyGame.ConfigPathList.Count != 0 || sa.MyGame.SavePathList.Count != 0);

                string syncFolderGamePath = Path.Combine(syncFolderPath, sa.MyGame.Name);

                if (syncDirection == WebToCom)
                {
                    int backupResult = Backup(sa);
                    if (backupResult == None) //Backup was not successful
                        //Add all game files to error list
                        AddToUnsuccessfulSyncFiles(sa, syncFolderGamePath, "Unable to backup original game files");
                    else
                        WebToComputer(sa, currentUser.Email, backupResult);
                }
                else if (syncDirection == ComToWeb)
                {
                    ComputerToWeb(currentUser.Email, sa);
                }
                else if (syncDirection == ExternalAndWeb)
                {
                }
            }
            return syncActionList;
        }
        public List<string> getGamesFromWeb(string user)
        {
            Storage s3 = new Storage();
            HashSet<string> gameSet = new HashSet<string>();
            foreach (string file in s3.ListFiles(user))
            {
                Console.WriteLine(file);
                string f = file.Substring(file.IndexOf('/') + 1);
                string game, type;
                if (f.IndexOf('/') != -1)
                {
                    game = f.Substring(0, f.IndexOf('/'));
                    type = f.Substring(f.IndexOf('/') + 1);
                    gameSet.Add(game + "/" + type.Substring(0, type.IndexOf('/')));
                }
            }
            return gameSet.ToList<string>();
        }
        private void ComputerToWeb(string email, SyncAction syncAction)
        {
            string gameName = syncAction.MyGame.Name;
            string saveParentPath = syncAction.MyGame.SaveParentPath;
            string configParentPath = syncAction.MyGame.ConfigParentPath;
            List<string> savePathList = syncAction.MyGame.SavePathList;
            List<string> configPathList = syncAction.MyGame.ConfigPathList;
            int action = syncAction.Action;

            //Delete game dir in user's account
            DeleteGameDirectory(email, gameName);

            //Saved Files - SyncAction
            if (action == SavedGame || action == AllFiles)
            {
                Upload(email, gameName, SyncFolderSavedGameFolderName, saveParentPath, savePathList);
            }

            //Config Files - SyncAction
            if (action == Config || action == AllFiles)
            {
                Upload(email, gameName, SyncFolderConfigFolderName, configParentPath, configPathList);
            }
        }
        private void Upload(string email, string gameName, string saveFolder, string parentPath, List<string> pathList)
        {
            string gamePath = email + "/" + gameName + "/" + saveFolder + "/";

            foreach (string path in pathList)
            {
                if (Directory.Exists(path))
                {
                    s3.UploadDirectory(gamePath, parentPath, path);
                }
                else if (File.Exists(path))
                {
                    string keyName = gamePath + path.Replace(parentPath, "").Replace(@"\", "/");
                    s3.UploadFile(path, keyName);
                }
                else
                {
                    DeleteGameDirectory(email, gameName);
                }
            }
        }
        private void DeleteGameDirectory(string email, string gameName)
        {
            string key = email + "/" + gameName;
            s3.DeleteDirectory(key);
        }
        private void WebToComputer(SyncAction sa, string user, int backupItem)
        {
            //Check for valid option
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles);
            Debug.Assert(sa.MyGame != null);

            List<SyncError> errorList = null;
            if (backupItem == Config || backupItem == AllFiles)
            {
                string processName = "Sync Config files from Web to Computer";
                string webFolderGamePath = user + "/" + sa.MyGame.Name;
                string webGameConfigPath = webFolderGamePath + "/" + SyncFolderConfigFolderName;

                //Copy over and add the error encounted into the error list
                errorList = DownloadToParent(webGameConfigPath, sa.MyGame.ConfigParentPath, processName);

                //Update the error list of the game
                sa.UnsuccessfulSyncFiles.AddRange(errorList);

                errorList = null;
            }

            if (backupItem == SavedGame || backupItem == AllFiles)
            {
                string processName = "Sync Saved Game files from Web to Computer";
                string webFolderGamePath = user + "/" + sa.MyGame.Name;
                string webGameSavedGamePath = webFolderGamePath + "/" + SyncFolderSavedGameFolderName;
                //Debug.Assert(Directory.Exists(webFolderGameSavedGamePath));

                //Copy over and add the error encounted into the error list
                errorList = DownloadToParent(webGameSavedGamePath, sa.MyGame.SaveParentPath, processName);
                //Update the error list of the game
                sa.UnsuccessfulSyncFiles.AddRange(errorList);

                errorList = null;
            }
        }
        private List<SyncError> DownloadToParent(string s3Path, string targetPath, string processName)
        {
            Debug.Assert(targetPath != null);

            List<SyncError> errorList = new List<SyncError>();
            List<string> files = s3.ListFiles(s3Path);
            string localDirName,webDirName;
            foreach (string file in files)
            {
                Console.WriteLine(file);
                webDirName = GetWebFolderName(file);
                localDirName = Path.Combine(targetPath, webDirName);
                Console.WriteLine(localDirName);
                try
                {
                    //Create the target directory if needed
                    Console.WriteLine(Path.GetDirectoryName(localDirName));
                    if (!Directory.Exists(Path.GetDirectoryName(localDirName)))
                        CreateDirectory(Path.GetDirectoryName(Path.Combine(targetPath, webDirName)));
                }
                catch (CreateFolderFailedException ex)
                {
                    errorList.AddRange(GetSyncError(file, processName, ex.Message));
                }
                try
                {
                    s3.DownloadFile(Path.Combine(targetPath, webDirName), file);
                }
                catch (Exception ex)
                {
                    errorList.AddRange(GetSyncError(file, processName, ex.Message));
                }
            }
            return errorList;
        }

        private string GetWebFolderName(string key)
        {
            //Gets the <folder>/<file> from <user>/<game>/<config or save folder>/<folder>/<file> on S3
            string filter;
            filter = key.Substring(key.IndexOf('/')+1);
            filter = filter.Substring(filter.IndexOf('/')+1);
            filter = filter.Substring(filter.IndexOf('/')+1);
            return filter;
        }
        /*
        private string GetGameFolderName(string key)
        {
            string filter;
            filter = key.Substring(key.IndexOf('/') + 1);
            return filter;
        }
        */
    }
}
        
        //Old code - Keep it here temporarily first
        /*
        //Code below copied from OfflineSync class with a few renames
        private int Backup(SyncAction sa)
        {
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles);
            Debug.Assert(sa.MyGame != null);
            List<SyncError> errorList;
            int result = NONE; //Result stores the item that has been backup
            Game game = null; //Game information on the computer

            //Find the game information on the computer, matched by name
            game = FindInstalledGame(sa.MyGame);

            //Determine the type of files to be sync
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                string processName = "Backup Config files on computer";
                //Backup and store any error encounted into the error list
                errorList = BackupHelper(game, processName, CONFIG);

                //Update backup result
                if (errorList.Count == 0) //Successfully backup with no error
                    result = CONFIG;
                else //Encounted error in backup
                    //Updated the list of sync error
                    sa.UnsuccessfulSyncFiles.AddRange(errorList);

                errorList = null;
            }

            //Determine the type of files to be sync
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                string processName = "BackupPathList Saved Game files on Computer";
                //Backup and store any error encounted into the error list
                errorList = BackupHelper(game, processName, SAVED_GAME);

                //Update backup result
                if (errorList.Count == 0) //Successfully backup with no error
                {
                    if (result == NONE) //No Config files was backup
                        result = SAVED_GAME;
                    else if (result > 0) //Config was backup
                        result = All_FILES;
                }
                else //Encounted error in backup
                    //Updated the list of sync error
                    sa.UnsuccessfulSyncFiles.AddRange(errorList);
            }
            return result;
        }
        /// <summary>
        /// Assist in backup method.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="processName"></param>
        /// <param name="backupAction"></param>
        /// <returns>The errors encounted in this process.</returns>
        private List<SyncError> BackupHelper(Game game, string processName, int backupAction)
        {
            Debug.Assert(game != null);
            Debug.Assert(!processName.Equals(""));
            Debug.Assert(backupAction == CONFIG || backupAction == SAVED_GAME);

            List<SyncError> errorList = null;
            string backupFolderParentPath = null;
            string backupFolderName = null;
            List<string> filesToBackup = null;

            //Set the variables for each case
            switch (backupAction)
            {
                case CONFIG:
                    backupFolderParentPath = game.ConfigParentPath;
                    backupFolderName = BackupConfigFolderName;
                    filesToBackup = game.ConfigPathList;
                    break;
                case SAVED_GAME:
                    backupFolderParentPath = game.SaveParentPath;
                    backupFolderName = BackupSavedGameFolderName;
                    filesToBackup = game.SavePathList;
                    break;
            }

            //Create backup if the no other backup was found
            if (!CheckBackupExists(backupFolderParentPath, backupFolderName))
            {
                if (filesToBackup.Count > 0)
                {
                    //Backup the game files in the computer
                    errorList = BackupPathList(filesToBackup, backupFolderName, processName);
                }
                else  //Create a empty backup folder when there is no game file to backup
                {
                    string backupFolderPath = Path.Combine(backupFolderParentPath, backupFolderName);
                    CreateDirectory(backupFolderPath);
                }
            }
            else //If backup folder exists, current game files on computer will be removed
            {
                Delete(filesToBackup);
            }


            if (errorList == null)
                errorList = new List<SyncError>();
            return errorList;
        }
        /// <summary>
        /// Determine if backup folder exists.
        /// </summary>
        /// <param name="path">Parent path.</param>
        /// <param name="backupFolderName">The name of the backup folder.</param>
        /// <returns>True if backup exists.</returns>
        public bool CheckBackupExists(string path, string backupFolderName)
        {
            string backupFolderPath = Path.Combine(path, backupFolderName);

            if (Directory.Exists(backupFolderPath))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Saves the given list of path into a backup folder in the same directory.
        /// </summary>
        /// <param name="gameItemList">List of path that are to be backup.</param>
        /// <param name="backupFolderName">The name of backup folder.</param>
        /// <param name="processDescription">Information on the backup process.</param>
        /// <returns>A list of SyncError that are encounted in this process.</returns>
        private List<SyncError> BackupPathList(List<string> gameItemList, string backupFolderName, string processDescription)
        {
            List<SyncError> errorList = new List<SyncError>();
            //Iterate through each path in the list
            foreach (string item in gameItemList)
            {
                if (File.Exists(item)) //Item is a file
                {
                    string backupFolderPath = Path.Combine(Path.GetDirectoryName(item), backupFolderName);
                    string fileName = Path.GetFileName(item);

                    try
                    {
                        //Create the destination folder to store the original file
                        if (!Directory.Exists(backupFolderPath))
                            CreateDirectory(backupFolderPath);
                        Debug.Assert(Directory.Exists(backupFolderPath));
                        //Move the file into the backup folder
                        File.Move(item, Path.Combine(backupFolderPath, fileName));
                    }
                    catch (CreateFolderFailedException ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                        break;
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                        break;
                    }
                }
                else //item is a directory
                {
                    string backupFolderPath = Path.Combine(Path.GetDirectoryName(item), backupFolderName);
                    string folderName = Path.GetFileName(item);

                    try
                    {
                        //Create the destination folder to store the original file
                        if (!Directory.Exists(backupFolderPath))
                            CreateDirectory(backupFolderPath);
                        Debug.Assert(Directory.Exists(backupFolderPath));
                        //Move the directory into the backup folder
                        Directory.Move(item, Path.Combine(backupFolderPath, folderName));
                    }
                    catch (CreateFolderFailedException ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                        break;
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                        break;
                    }
                }
            }
            return errorList;
        }
        /// <summary>
        /// Copy the list of path from computer to external storage device. Overwrites data in external device.
        /// </summary>
        /// <param name="gameItemList">The list of path to be copy over to external device.</param>
        /// <param name="processName">The process description.</param>
        /// <param name="targetPath">The destination path to copy to.</param>
        /// <returns>A list of SyncError that are encounted in this process.</returns>
        private List<SyncError> CopyPathListToExternal(List<String> gameItemList, string processName, string targetPath)
        {
            string syncFolderGameSubPath = targetPath;
            List<SyncError> errorList = new List<SyncError>();

            if (gameItemList.Count == 0) return errorList;

            // Overwrite existing data in the target path
            if (Directory.Exists(syncFolderGameSubPath))
                DeleteDirectory(syncFolderGameSubPath);
            CreateDirectory(syncFolderGameSubPath);

            //Iterate through each path in list and copy
            foreach (string gameItem in gameItemList)
            {
                if (File.Exists(gameItem)) //Item is a file
                {
                    string fileName = Path.GetFileName(gameItem);
                    try
                    {
                        File.Copy(gameItem, Path.Combine(syncFolderGameSubPath, fileName), true);
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(gameItem, processName, ex.Message));
                    }
                }
                else //Item is a folder
                {
                    string folderName = Path.GetFileName(gameItem);
                    string newTargetPath = Path.Combine(syncFolderGameSubPath, folderName);
                    errorList.AddRange(CopyDirectory(gameItem, newTargetPath, processName));
                }
            }
            return errorList;
        }
        /// <summary>
        /// Copy the content in the source directory to the target directory, overwriting old files in target directory.
        /// </summary>
        /// <param name="sourcePath">Source directory that is to copied.</param>
        /// <param name="targetPath">Target diectory that is to store the content of source directory.</param>
        /// <param name="processName">A process description.</param>
        /// <returns>A list of SyncError that are encounted in this process.</returns>
        private List<SyncError> CopyDirectory(string sourcePath, string targetPath, string processName)
        {
            Debug.Assert(Directory.Exists(sourcePath));
            Debug.Assert(targetPath != null);

            List<SyncError> errorList = new List<SyncError>();

            try
            {
                //Create the target directory if needed
                if (!Directory.Exists(targetPath))
                    CreateDirectory(targetPath);
            }
            catch (CreateFolderFailedException ex)
            {
                return GetSyncError(sourcePath, processName, ex.Message);
            }

            String[] files = Directory.GetFiles(sourcePath);
            String[] subDirectories = Directory.GetDirectories(sourcePath);

            //Copy files
            if (files.Length != 0)
                foreach (string f in files)
                {
                    string fileName = f.Substring(sourcePath.Length + 1);
                    try
                    {
                        File.Copy(Path.Combine(sourcePath, fileName), Path.Combine(targetPath, fileName), true);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        errorList.AddRange(GetSyncError(f, processName, ex.Message));
                    }
                    catch (IOException ex)
                    {
                        errorList.AddRange(GetSyncError(f, processName, ex.Message));
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(f, processName, ex.Message));
                    }
                }
            //Copy subfolders
            if (subDirectories.Length != 0)
                foreach (string d in subDirectories)
                {
                    string folderName = d.Substring(sourcePath.Length + 1);
                    if (Directory.Exists(Path.Combine(targetPath, folderName)))
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.Combine(targetPath, folderName));
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            errorList.AddRange(GetSyncError(d, processName, ex.Message));
                        }
                        catch (IOException ex)
                        {
                            errorList.AddRange(GetSyncError(d, processName, ex.Message));
                        }
                        catch (Exception ex)
                        {
                            errorList.AddRange(GetSyncError(d, processName, ex.Message));
                        }
                    }
                    errorList.AddRange(CopyDirectory(d, Path.Combine(targetPath, folderName), processName));

                }
            return errorList;
        }
        /// <summary>
        /// Creates a directory if it does not exists.
        /// </summary>
        /// <param name="newFolderPath">Path of the new directory.</param>
        /// <returns>Path of the new directory.</returns>
        private string CreateDirectory(string newFolderPath)
        {
            if (!Directory.Exists(newFolderPath))
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                }
                catch (Exception ex)
                {
                    throw new CreateFolderFailedException("Unable to create new folder: " + newFolderPath, ex);
                }
            return newFolderPath;
        }
        //TODO check if create folder need to return a string 
        /// <summary>
        /// Creates a directory if it does not exists.
        /// </summary>
        /// <param name="parentDirectory">Path of the target directory where the new folder will be created.</param>
        /// <param name="folderName">New folder name.</param>
        /// <returns>Path of the new directory.</returns>
        private string CreateDirectory(string parentDirectory, string folderName)
        {
            string newFolderPath = Path.Combine(parentDirectory, folderName);
            return CreateDirectory(newFolderPath);
        }
        /// <summary>
        /// Takes in a game object and look for a game in installedGameList that has the same game name.
        /// </summary>
        /// <param name="extGame">The game to match for.</param>
        /// <returns>Return the first matched game in installedGameList based on matched game name.</returns>
        private Game FindInstalledGame(Game extGame)
        {
            wantedGame = extGame;
            Game installedGame = installedGameList.Find(MatchWantedGameName);
            Debug.Assert(installedGame != null);

            return installedGame;
        }
        private List<SyncError> Delete(List<string> backupFolderParentPathList, string backupFolderName)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (string path in backupFolderParentPathList)
            {
                string backupFolderPath = Path.Combine(Path.GetDirectoryName(path), backupFolderName);
                try
                {
                    if (Directory.Exists(backupFolderPath))
                        DeleteDirectory(backupFolderPath);
                }
                catch (DeleteDirectoryErrorException ex)
                {
                    string processName = "Remove backup file";
                    errorList.AddRange(GetSyncError(path, processName, ex.Message));
                }
            }
            return errorList;
        }
        private List<SyncError> Delete(List<string> pathList)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (string path in pathList)
                try
                {
                    if (File.Exists(path)) //path is a file
                        File.Delete(path);
                    else //path is a directory
                        DeleteDirectory(path);
                }
                catch (DeleteDirectoryErrorException ex)
                {
                    string processName = "Remove backup file";
                    errorList.AddRange(GetSyncError(path, processName, ex.Message));
                }
            return errorList;
        }
        private List<SyncError> GetSyncError(string errorPath, string processName, string errorMessage)
        {
            List<SyncError> errorList = new List<SyncError>();
            Debug.WriteLine(errorMessage);
            if (File.Exists(errorPath)) //Path is a file
                errorList.Add(new SyncError(errorPath, processName, errorMessage));
            else //Path is a folder
            {
                //Add all files in the folder
                foreach (string errorFile in Directory.GetFiles(errorPath))
                    errorList.Add(new SyncError(errorPath, processName, errorMessage));
                //Add all subfolders in the folder
                foreach (string subFolder in Directory.GetDirectories(errorPath))
                    errorList.AddRange(GetSyncError(subFolder, processName, errorMessage));
            }
            return errorList;
        }
        /// <summary>
        /// Make the given list of path into a list of SyncError
        /// </summary>
        /// <param name="errorPath">The list of file path that failed in a process.</param>
        /// <param name="processName">A process description.</param>
        /// <param name="errorMessage">The error message encountered.</param>
        /// <returns>A list of SyncError of the given path list.</returns>
        private List<SyncError> GetSyncError(List<string> errorPathList, string processName, string errorMessage)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (string errorPath in errorPathList)
                errorList.AddRange(GetSyncError(errorPath, processName, errorMessage));
            return errorList;
        }
        private void DeleteDirectory(string directory)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(directory, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
            }
            catch (Exception ex)
            {
                throw new DeleteDirectoryErrorException(ex.Message);
            }
        }
        private bool MatchWantedGameName(Game game)
        {
            if (game.Name.Equals(wantedGame.Name))
                return true;
            else return false;

        }
        private void AddToUnsuccessfulSyncFiles(SyncAction sa, string gamePath, string errorMessage)
        {
            string targetSyncFiles = "";

            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                string processName = "Sync Config files from External Device to Computer";
                targetSyncFiles = Path.Combine(gamePath, SyncFolderConfigFolderName);
                //Add all files to the error list 
                sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(targetSyncFiles, processName, errorMessage));
            }
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                string processName = "Sync Saved Game files from External Device to Computer";
                targetSyncFiles = Path.Combine(gamePath, SyncFolderSavedGameFolderName);
                //Add all files to the error list 
                sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(targetSyncFiles, processName, errorMessage));
            }
        }
        public List<SyncAction> Restore()
        {
            List<SyncAction> syncActionList = DetermineGamesWithBackup(installedGameList);
            int errorCounter = 0;
            foreach (SyncAction sa in syncActionList)
            {
                errorCounter += RestoreGame(sa);
            }
            //When all files are safely copied over, then remove all backup folder
            if (errorCounter == 0)
                RemoveAllBackup(syncActionList);

            return syncActionList;
        }
        /// <summary>
        /// Replace existing config and/or saved game files of a game on the computer with the ones in the backup folder.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's file information.</param>
        /// <returns>The number of SyncError encounted.</returns>
        private int RestoreGame(SyncAction sa)
        {
            int errorCounter = 0;
            List<SyncError> errorList = new List<SyncError>();
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                //Remove all current config files in computer.
                foreach (string config in sa.MyGame.ConfigPathList)
                {
                    if (File.Exists(config))//Config is a file
                        File.Delete(config);
                    else //Config is a folder
                        DeleteDirectory(config);
                }

                //Copy all items from backup into config parent folder
                string sourcePath = Path.Combine(sa.MyGame.ConfigParentPath, BackupConfigFolderName);
                string targetPath = sa.MyGame.ConfigParentPath;
                string processName = "Copy original config files from backup folder to parent folder";
                errorList.AddRange(CopyDirectory(sourcePath, targetPath, processName));
                errorCounter += errorList.Count;
                sa.UnsuccessfulSyncFiles.AddRange(errorList);
                errorList.Clear();
            }
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                //Remove all current config files in computer
                foreach (string savedGame in sa.MyGame.SavePathList)
                {
                    if (File.Exists(savedGame))//Saved Game is a file
                        File.Delete(savedGame);
                    else //Saved Game is a folder
                        DeleteDirectory(savedGame);
                }

                //Copy all items from backup into config parent folder
                string sourcePath = Path.Combine(sa.MyGame.SaveParentPath, BackupSavedGameFolderName);
                string targetPath = sa.MyGame.SaveParentPath;
                string processName = "Copy original saved game files from backup folder to parent folder";
                errorList.AddRange(CopyDirectory(sourcePath, targetPath, processName));
                errorCounter += errorList.Count;
                sa.UnsuccessfulSyncFiles.AddRange(errorList);
                errorList.Clear();
            }
            return errorCounter;
        }
        /// <summary>
        /// For each installed game, go through their directories to find backup folders.
        /// </summary>
        /// <param name="gameList">List of installed games.</param>
        /// <returns>List of SyncActions that contains each game, and their corresponding status: 0,1,2,3.</returns>
        private List<SyncAction> DetermineGamesWithBackup(List<Game> gameList)
        {
            List<SyncAction> syncActionList = new List<SyncAction>();
            foreach (Game g in gameList)
            {
                SyncAction sa = new SyncAction();
                sa.MyGame = g;

                // Check for existence of backup folder for config
                if (Directory.Exists(Path.Combine(g.ConfigParentPath, BackupConfigFolderName))
                    && Directory.Exists(Path.Combine(g.SaveParentPath, BackupSavedGameFolderName)))
                {
                    sa.Action = SyncAction.AllFiles;
                }
                else if (Directory.Exists(Path.Combine(g.ConfigParentPath, BackupConfigFolderName)))
                {
                    sa.Action = SyncAction.ConfigFiles;
                }
                else if (Directory.Exists(Path.Combine(g.SaveParentPath, BackupSavedGameFolderName)))
                {
                    sa.Action = SyncAction.SavedGameFiles;
                }
                else
                    sa.Action = SyncAction.DoNothing;

                syncActionList.Add(sa);
            }
            return syncActionList;
        }
        /// <summary>
        /// Deletes all backup folder created by GameAnywhere after syncing.
        /// 
        /// Exception: DeleteDirectoryErrorException() - Unable to remove backup folder.
        /// </summary>
        /// <param name="syncActionList">The list of SyncAction</param>
        /// <returns>A list of SyncError of the given path.</returns>
        private List<SyncError> RemoveAllBackup(List<SyncAction> syncActionList)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (SyncAction sa in syncActionList)
            {
                if (sa.Action > 0)
                {
                    errorList.AddRange(Delete(sa.MyGame.ConfigPathList, BackupConfigFolderName));
                    errorList.AddRange(Delete(sa.MyGame.SavePathList, BackupSavedGameFolderName));
                }
            }
            return errorList;
        }*/