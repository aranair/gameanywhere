using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace GameAnywhere
{
    /// <summary>
    /// An abstract synchronization class. Contain methods to create and delete directory. And also backup and restore original game files. 
    /// </summary>
    abstract class Sync
    {
        /// <summary>
        /// Folder name for the config backup folder, located on the computer.
        /// </summary>
        public static readonly string BackupConfigFolderName = "GA-configBackup";

        /// <summary>
        /// Folder name for saved game backup folder, located on the computer.
        /// </summary>
        public static readonly string BackupSavedGameFolderName = "GA-savedGameBackup";

        /// <summary>
        /// Folder name for saved game folder, located on the external storage.
        /// </summary>
        public static readonly string SyncFolderSavedGameFolderName = "savedGame";

        /// <summary>
        /// Folder name for game configuration folder, located on the external storage.
        /// </summary>
        public static readonly string SyncFolderConfigFolderName = "config";

        /// <summary>
        /// Default syncFolder is located in the same directory as the current executable program.
        /// </summary>
        public static readonly string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");

        /// <summary>
        /// Sync direction uninitialize. Default value.
        /// </summary>
        public const int Uninitialize = 0; //Default

        /// <summary>
        /// Backup Status: No files.
        /// </summary>
        protected const int None = 0;
        /// <summary>
        /// Backup Status: Game configuration files.
        /// </summary>
        protected const int Config = 1;
        /// <summary>
        /// Backup Status: Saved game files.
        /// </summary>
        protected const int SavedGame = 2;
        /// <summary>
        /// Backup Status: All game files.
        /// </summary>
        protected const int AllFiles = 3;

        /// <summary>
        /// For matching game name.
        /// Used by find
        /// </summary>
        private Game wantedGame;

        /// <summary>
        /// Stores game information on the computer.
        /// </summary>
        protected List<Game> installedGameList;

        /// <summary>
        /// List of sync action to be carried out.
        /// </summary>
        protected List<SyncAction> syncActionList;

        /// <summary>
        /// Direction of sync. 
        /// </summary>
        protected int syncDirection;

        /// <summary>
        /// Property for SyncDirection
        /// </summary>
        public int SyncDirection
        {
            get { return syncDirection; }
            set { syncDirection = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sync"/> class.
        /// </summary>
        protected Sync()
        {
            syncDirection = Uninitialize;
        }


        /// <summary>
        /// This is an abstract class. It should handle the synchronization for the given list of sync action. 
        /// </summary>
        /// <param name="list">List of sync action</param>
        /// <returns>The result of synchronization</returns>
        public abstract List<SyncAction> SynchronizeGames(List<SyncAction> list);



        /// <summary>
        /// Pre-Condition: None.
        /// Post-Condition: Files that were copied over are erased.
        /// 
        /// Description: Goes through config path list and saved past list and delete the files that were copied over.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="sa">SyncAction object</param>
        protected void DeleteCopiedFiles(SyncAction sa)
        {
            List<string> configPathList = new List<string>();
            List<string> savePathList = new List<string>();
            bool existInErrorList = false;

            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                // Delete config files that were copied over.
                foreach (string s in sa.MyGame.ConfigPathList)
                {
                    string newConfigPath;
                    newConfigPath = s.Substring(s.LastIndexOf("\\"));
                    newConfigPath = sa.MyGame.ConfigParentPath + newConfigPath;

                    existInErrorList = CheckIfExistInErrorList(sa.UnsuccessfulSyncFiles, newConfigPath);

                    if (!existInErrorList)
                    {
                        configPathList.Add(newConfigPath);
                    }
                    existInErrorList = false;

                }

                Delete(configPathList);
            }

            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                // Delete saved game files that were copied over.
                foreach (string s in sa.MyGame.SavePathList)
                {
                    string newSavePath;
                    newSavePath = s.Substring(s.LastIndexOf("\\"));
                    newSavePath = sa.MyGame.SaveParentPath + newSavePath;

                    existInErrorList = CheckIfExistInErrorList(sa.UnsuccessfulSyncFiles, newSavePath);

                    if (!existInErrorList)
                    {
                        savePathList.Add(newSavePath);
                    }
                    existInErrorList = false;
                }

                Delete(savePathList);
            }
        }

        /// <summary>
        /// Check if a given path exists in the unsuccessful sync file list.
        /// </summary>
        /// <param name="errorList">The list of unsuccessfuk sync file</param>
        /// <param name="path">The path in question</param>
        /// <returns>True if path found in error list</returns>
        private static bool CheckIfExistInErrorList(List<SyncError> errorList, string path)
        {
            foreach (SyncError syncError in errorList)
            {

                if (syncError.FilePath == path)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Takes in a game object and look for a game in installedGameList that has the same game name.
        /// Method fails if game does not belong in the list of installed game.
        ///  
        /// Pre-condition: Game must be in the list of installed game. 
        /// </summary>
        /// <param name="externalGame">The game to match for.</param>
        /// <returns>Return the first matched game in installedGameList based on matched game name.</returns>
        protected Game FindInstalledGame(Game externalGame)
        {
            wantedGame = externalGame;
            Game installedGame = installedGameList.Find(MatchWantedGameName);
            Debug.Assert(installedGame != null,"Game not found in the installedGameList.");

            return installedGame;
        }

        /// <summary>
        /// A search predicate for the wanted game.
        /// Used by List.Find.
        /// </summary>
        /// <param name="game">Game object.</param>
        /// <returns>Returns true if game name matches the name of wantedGame.</returns>
        private bool MatchWantedGameName(Game game)
        {
            if (game.Name.Equals(wantedGame.Name))
                return true;
            return false;

        }

        /// <summary>
        /// Add all targeted sync files into error list.
        /// </summary>
        /// <param name="sa">The SyncAction which its game files (depending on it's sync action) will be added to the unsuccessful sync files list</param>
        /// <param name="gamePath">The game directory in the external device</param>
        /// <param name="errorMessage">The reason for call this method.</param>
        protected void AddToUnsuccessfulSyncFiles(SyncAction sa, string gamePath, string errorMessage)
        {
            string targetSyncFiles;

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

        /// <summary>
        /// Saves original game files on the computer into a backup folder in the same directory.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <returns>The backup status status, indicating what has been successfully backup.</returns>
        protected int Backup(SyncAction sa)
        {
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles, "Invalid Sync Action.");
            Debug.Assert(sa.MyGame != null, "Game object not instantiated in Sync Action");
            List<SyncError> errorList;
            int result = None; //Result stores the item that has been backup
            Game game; //Game information on the computer

            //Find the game information on the computer, matched by name
            game = FindInstalledGame(sa.MyGame);

            //Determine the type of files to be sync
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                string processName = "Backup Config files on computer";
                //Backup and store any error encounted into the error list
                errorList = BackupHelper(game, processName, Config);

                //Update backup result
                if (errorList.Count == 0) //Successfully backup with no error
                    result = Config;
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
                errorList = BackupHelper(game, processName, SavedGame);

                //Update backup result
                if (errorList.Count == 0) //Successfully backup with no error
                {
                    if (result == None) //No Config files was backup
                        result = SavedGame;
                    else if (result > 0) //Config was backup
                        result = AllFiles;
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
        /// <param name="game">The game information.</param>
        /// <param name="processName">Process desciption.</param>
        /// <param name="backupAction">What is to be backup.</param>
        /// <returns>The errors encounted in this process.</returns>
        private List<SyncError> BackupHelper(Game game, string processName, int backupAction)
        {
            Debug.Assert(game != null, "Game object not instantiated in Sync Action");
            Debug.Assert(!processName.Equals(""), "No process name.");
            Debug.Assert(backupAction == Config || backupAction == SavedGame, "Invalid backup action.");

            List<SyncError> errorList = null;
            string backupFolderParentPath = null;
            string backupFolderName = null;
            List<string> filesToBackup = null;

            //Set the variables for each case
            switch (backupAction)
            {
                case Config:
                    backupFolderParentPath = game.ConfigParentPath;
                    backupFolderName = BackupConfigFolderName;
                    filesToBackup = game.ConfigPathList;
                    break;
                case SavedGame:
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
                    try
                    {
                        CreateDirectory(backupFolderPath);
                    }
                    catch (CreateFolderFailedException ex)
                    {
                        errorList = GetSyncError(filesToBackup, "Creating empty backup folder in parent path", ex.errorMessage);
                    }
                }
            }
            else //If backup folder exists, current game files on computer will be removed
            {
                errorList = Delete(filesToBackup);
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

                string backupFolderPath = Path.Combine(Path.GetDirectoryName(item), backupFolderName);

                //Create the destination folder to store the original file
                try
                {
                        CreateDirectory(backupFolderPath);
                }
                catch (CreateFolderFailedException ex)
                {
                    errorList.AddRange(GetSyncError(gameItemList, processDescription, ex.errorMessage));
                    return errorList;
                }

                Debug.Assert(Directory.Exists(backupFolderPath), "Backup folder does not exists.");

                if (File.Exists(item)) //Item is a file
                {

                    string fileName = Path.GetFileName(item);

                    try
                    {
                        //Move the file into the backup folder
                        File.Move(item, Path.Combine(backupFolderPath, fileName));
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                    }
                }
                else //item is a directory
                {

                    string folderName = Path.GetFileName(item);

                    try
                    {
                        //Move the directory into the backup folder
                        Directory.Move(item, Path.Combine(backupFolderPath, folderName));
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
                    }
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
            if (!Directory.Exists(sourcePath))
                return GetSyncError(sourcePath, processName, "Unable to create directory/Directory is missing - " + sourcePath + ".");

            Debug.Assert(targetPath != null, "Invalid target destination.");

            List<SyncError> errorList = new List<SyncError>();
            String[] files;
            String[] subDirectories;

            try
            {
                //Create the target directory if needed
                CreateDirectory(targetPath);
            }
            catch (CreateFolderFailedException ex)
            {
                return GetSyncError(sourcePath, processName, ex.errorMessage);
            }

            try
            {
                files = Directory.GetFiles(sourcePath);
                subDirectories = Directory.GetDirectories(sourcePath);
            }
            catch (Exception e)
            {
                return GetSyncError(sourcePath, processName, e.Message);
            }

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
        /// 
        /// CreateFolderFailedException thrown whenever unsuccessful.
        /// </summary>
        /// <param name="newFolderPath">Path of the new directory.</param>
        protected void CreateDirectory(string newFolderPath)
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
        }

        /// <summary>
        /// Creates a directory if it does not exists.
        /// </summary>
        /// <param name="parentDirectory">Path of the target directory where the new folder will be created.</param>
        /// <param name="folderName">New folder name.</param>
        protected void CreateDirectory(string parentDirectory, string folderName)
        {
            string newFolderPath = Path.Combine(parentDirectory, folderName);
            CreateDirectory(newFolderPath);
        }

        /// <summary>
        /// Deletes a directory to Recycle bin.
        /// 
        /// NOTE: Referencing to Microsoft.VisualBasic assembly is needed for this method to work.
        /// </summary>
        /// <param name="directory">Path of the directory to be deleted.</param>
        private void DeleteDirectory(string directory)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(directory, Microsoft.VisualBasic.FileIO.DeleteDirectoryOption.DeleteAllContents);
            }
            catch (Exception)
            {
                throw new DeleteDirectoryErrorException("Access to directory is denied.");
            }
        }

        /// <summary>
        /// Make the given path into a list of SyncError
        /// </summary>
        /// <param name="errorPath">The file path that failed in a process.</param>
        /// <param name="processName">A process description.</param>
        /// <param name="errorMessage">The error message encountered.</param>
        /// <returns>A list of sync error of the given path list.</returns>
        protected List<SyncError> GetSyncError(string errorPath, string processName, string errorMessage)
        {
            List<SyncError> errorList = new List<SyncError>();
            Debug.WriteLine(errorMessage);

            if (File.Exists(errorPath)) //Path is a file
                errorList.Add(new SyncError(errorPath, processName, errorMessage));
            else //Path is a folder
            {
                if (Directory.Exists(errorPath))
                {
                    /*//Add all files in the folder
                    foreach (string errorFile in Directory.GetFiles(errorPath))
                        errorList.Add(new SyncError(errorPath, processName, errorMessage));
                    //Add all subfolders in the folder
                    foreach (string subFolder in Directory.GetDirectories(errorPath))
                        errorList.AddRange(GetSyncError(subFolder, processName, errorMessage));*/

                    errorList.Add(new SyncError(errorPath, processName, errorMessage));
                }
                else
                    errorList.Add(new SyncError(errorPath, processName, errorMessage));

            }
            return errorList;
        }

        /// <summary>
        /// Make the given list of path into a list of SyncError
        /// </summary>
        /// <param name="errorPathList">The list of file path that failed in a process.</param>
        /// <param name="processName">A process description.</param>
        /// <param name="errorMessage">The error message encountered.</param>
        /// <returns>A list of sync error of the given path list.</returns>
        protected List<SyncError> GetSyncError(List<string> errorPathList, string processName, string errorMessage)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (string errorPath in errorPathList)
                errorList.AddRange(GetSyncError(errorPath, processName, errorMessage));
            return errorList;
        }


        /// <summary>
        /// Replace the saved game files and game configuration files with the backup files on the computer.
        /// Backup folders will be removed.
        /// </summary>
        /// <returns>The list of SyncAction that contain failed restore files.</returns>
        public List<SyncAction> Restore()
        {
            List<SyncAction> syncActionList = DetermineGamesWithBackup(installedGameList);
            List<SyncError> syncErrorList = new List<SyncError>();
            int errorCounter = 0;

            foreach (SyncAction sa in syncActionList)
            {
                errorCounter += RestoreGame(sa, true);

                //When all files are safely copied over, then remove all backup folder
                if (errorCounter == 0)
                {
                    List<SyncAction> oneSyncAction = new List<SyncAction>();
                    oneSyncAction.Add(sa);
                    sa.UnsuccessfulSyncFiles.AddRange(RemoveAllBackup(oneSyncAction));
                }

                errorCounter = 0;
            }

            return syncActionList;
        }

        /// <summary>
        /// Replace existing config and/or saved game files of a game on the computer with the ones in the backup folder.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's file information.</param>
        /// <param name="deleteCurrentFiles">To delete current files, set to true.</param>
        /// <returns>The number of SyncError encounted.</returns>
        protected int RestoreGame(SyncAction sa, bool deleteCurrentFiles)
        {
            int errorCounter = 0;
            List<SyncError> errorList = new List<SyncError>();
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {

                if (deleteCurrentFiles)
                {
                    //Remove all current config files in computer.
                    errorList = Delete(sa.MyGame.ConfigPathList);
                }

                //Copy all items from backup folder into config parent folder
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
                if (deleteCurrentFiles)
                {
                    //Remove all current saved game files in computer
                    errorList = Delete(sa.MyGame.SavePathList);
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
        /// Delete the list of path that may contain backup folders into Recycle bin.
        /// 
        /// Exception: DeleteDirectoryErrorException() - Unable to remove backup folder.
        /// </summary>
        /// <param name="parentPath">The list of path that contains backup folder</param>
        /// <param name="backupFolderName">The backup folder name</param>
        /// <returns>A list of sync error of the given path list.</returns>
        private List<SyncError> Delete(string parentPath, string backupFolderName)
        {
            List<SyncError> errorList = new List<SyncError>();
            //foreach (string path in backupFolderParentPathList)
            {
                string backupFolderPath = Path.Combine(parentPath, backupFolderName);
                try
                {
                    if (Directory.Exists(backupFolderPath))
                        DeleteDirectory(backupFolderPath);
                }
                catch (DeleteDirectoryErrorException ex)
                {
                    string processName = "Remove backup file";
                    errorList.AddRange(GetSyncError(parentPath + backupFolderName, processName, ex.errorMessage));
                }
            }
            return errorList;
        }

        /// <summary>
        /// Moves all the path in the path list into Recycle bin.
        /// </summary>
        /// <param name="pathList">The list of path to be deleted.</param>
        /// <returns>A list of sync error of the given path list.</returns>
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
                    errorList.AddRange(GetSyncError(path, processName, ex.errorMessage));
                }
                catch (Exception)
                {
                }


            return errorList;
        }


        /// <summary>
        /// Pre-Condition: None.
        /// Post-Condition: Backup are removed and a SyncError list is returned.
        /// 
        /// Description: Determines which games have backup and remove the backup of the games.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns>A list of sync error of the given path list.</returns>
        public List<SyncError> RemoveAllBackup()
        {
            List<SyncAction> syncActionList = DetermineGamesWithBackup(installedGameList);

            return RemoveAllBackup(syncActionList);
        }

        /// <summary>
        /// Deletes all backup folder created by GameAnywhere after syncing.
        /// 
        /// Exception: DeleteDirectoryErrorException() - Unable to remove backup folder.
        /// </summary>
        /// <param name="syncActionList">The list of SyncAction</param>
        /// <returns>A list of sync error of the given path list.</returns>
        private List<SyncError> RemoveAllBackup(List<SyncAction> syncActionList)
        {
            List<SyncError> errorList = new List<SyncError>();
            foreach (SyncAction sa in syncActionList)
            {
                if (sa.Action > 0)
                {
                    errorList.AddRange(Delete(sa.MyGame.ConfigParentPath, BackupConfigFolderName));
                    errorList.AddRange(Delete(sa.MyGame.SaveParentPath, BackupSavedGameFolderName));
                }
            }
            return errorList;
        }
    }
}
