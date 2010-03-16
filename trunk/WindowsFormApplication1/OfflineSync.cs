using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Windows.Forms;


namespace GameAnywhere
{
    /// <summary>
    /// Method to synchronize Game files between computer and external storage device. 
    /// 
    /// </summary>
    class OfflineSync
    {
        //Folder name
        public static readonly string BackupConfigFolderName = "GA-configBackup";
        public static readonly string BackupSavedGameFolderName = "GA-savedGameBackup";
        public static readonly string SyncFolderSavedGameFolderName = "savedGame";
        public static readonly string SyncFolderConfigFolderName = "config";

        //Sync direction
        public static readonly int Uninitialize = 0; //Default
        public static readonly int ExternalToCom = 1;
        public static readonly int ComToExternal = 2;

        //Constant
        private const int NONE = 0;
        private const int CONFIG = 1;
        private const int SAVED_GAME = 2;
        private const int All_FILES = 3;

        //Data member
        private int syncDirection = Uninitialize; //Can only be set once
        private List<SyncAction> syncActionList; //Stores game information on the external syncFolder
        private List<Game> installedGameList; //Stores game information on the computer
        //Default syncFolder is located in the same directory as the current executable program
        private string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");
        private Game wantedGame = null;

        //Property
        public int SyncDirection
        {
            get { return syncDirection; }
            set
            {
                //syncDirection cannot be change once it has been set
                if (syncDirection == Uninitialize)
                    syncDirection = value;
            }
        }

        /// <summary>
        /// Constructor.
        /// 
        /// Exception:
        /// CreateFolderFailedException() throw when unable to create a syncFolder in the current GameAnywhere.exe directory.
        /// </summary>
        public OfflineSync()
        {
            CreateDirectory(syncFolderPath);
        }

        /// <summary>
        /// Overloaded Constructor.
        ///      
        /// Exception:
        /// CreateFolderFailedException() throw when unable to create a syncFolder in the current GameAnywhere.exe directory.
        /// </summary>
        /// <param name="direction">Synchronization direction</param>
        /// <param name="extPath">Path of the syncfolder in the external device</param>
        public OfflineSync(int direction, List<Game> gameList)
        {
            syncDirection = direction;
            installedGameList = gameList;
            CreateDirectory(syncFolderPath);

        }

        /// <summary>
        /// Given the direction of synchronization, SynchronizeGames will copy 
        /// saved game files and/or game configuration files between computer and 
        /// external storage device. Original game files on computer will be backup 
        /// into a GA-Backup folder in the same directory as the game file before overwriting.
        /// 
        /// syncActionList.unsuccessfulSyncFiles would contain the list of game files that could not be synced.
        ///             
        /// </summary>
        /// <param name="syncActionList">List of games which are to be synchronized.</param>
        /// <returns>List of games and their sync results.</returns>
        public List<SyncAction> SynchronizeGames(List<SyncAction> list)
        {
            syncActionList = list;

            if (syncActionList.Count == 0)
                return syncActionList;

            //Iterate through each game
            foreach (SyncAction sa in syncActionList)
            {
                Debug.Assert(sa.MyGame != null);

                string syncFolderGamePath = Path.Combine(syncFolderPath, sa.MyGame.Name);
                int backupResult = 0;

                //Copy game files from external device to computer
                if (syncDirection == ExternalToCom)
                {
                    //Backup original game files
                    backupResult = Backup(sa);

                    if (backupResult == NONE) //Backup was not successful
                    {
                        //Add all game files to error list
                        AddToUnsuccessfulSyncFiles(sa, syncFolderGamePath, "Unable to backup original game files.");
                    }
                    else
                    {
                        CopyToComputer(sa, syncFolderGamePath, backupResult);
                    }

                }
                else if (syncDirection == ComToExternal)
                    CopyToExternal(sa, syncFolderGamePath);


                // This section "restores" original status to computer if an error occurred in the synchronization of any file of the specific game
                if (syncDirection == ExternalToCom)
                {

                    if (sa.UnsuccessfulSyncFiles.Count > 0)
                    {

                        DeleteCopiedFiles(sa);

                        RestoreGame(sa, false);

                        List<SyncAction> singleSyncAction = new List<SyncAction>();


                        sa.MyGame = FindInstalledGame(sa.MyGame);
                        singleSyncAction.Add(sa);

                        RemoveAllBackup(singleSyncAction);
                    }

                }
            }
            return syncActionList;
        }

        /// <summary>
        /// Pre-Condition: None.
        /// Post-Condition: Files that were copied over are erased.
        /// 
        /// Description: Goes through config path list and saved past list and delete the files that were copied over.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="sa">SyncAction object</param>
        private void DeleteCopiedFiles(SyncAction sa)
        {
            List<string> configPathList = new List<string>();
            List<string> savePathList = new List<string>();
            bool doNotDelete = false;


            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                // Delete config files that were copied over.
                foreach (string s in sa.MyGame.ConfigPathList)
                {
                    string newConfigPath = "";
                    newConfigPath = s.Substring(s.LastIndexOf("\\"));
                    newConfigPath = sa.MyGame.ConfigParentPath + newConfigPath;

                    foreach (SyncError syncError in sa.UnsuccessfulSyncFiles)
                    {
                        
                        if (syncError.FilePath == newConfigPath)
                        {
                            doNotDelete = true;
                        }
                    }

                    if (!doNotDelete)
                    {
                        configPathList.Add(newConfigPath);
                    }
                    doNotDelete = false;

                }

                Delete(configPathList);
            }

            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                // Delete saved game files that were copied over.
                foreach (string s in sa.MyGame.SavePathList)
                {
                    string newSavePath = "";
                    newSavePath = s.Substring(s.LastIndexOf("\\"));
                    newSavePath = sa.MyGame.SaveParentPath + newSavePath;
                    foreach (SyncError syncError in sa.UnsuccessfulSyncFiles)
                    {
                        if (syncError.FilePath == newSavePath)
                        {
                            doNotDelete = true;
                        }
                    }

                    if (!doNotDelete)
                    {
                        savePathList.Add(newSavePath);
                    }
                    doNotDelete = false;
                }

                Delete(savePathList);
            }
        }


        /// <summary>
        /// Copy game files from computer to external storage device.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <param name="targetGamePath">The destination folder in the external storage device.</param>
        private void CopyToExternal(SyncAction sa, string targetGamePath)
        {
            string processName = "";
            try
            {
                //Create a game folder in the destination folder 
                if (!Directory.Exists(targetGamePath))
                    CreateDirectory(targetGamePath);
            }
            catch (CreateFolderFailedException ex)
            {
                //When unable to create new folder, add all files to be copied into error list
                processName = "Creating game folder in syncFolder";
                if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.ConfigPathList, processName, ex.errorMessage));
                if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.SavePathList, processName, ex.errorMessage));
                return;
            }

            Debug.Assert(Directory.Exists(targetGamePath));

            //Copy config files
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                bool enoughSpace = true;
                try
                {
                    enoughSpace = CheckForEnoughSpace(sa.MyGame.ConfigPathList);
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException) { }

                //When not enough space in the target location, add all files to error list
                if (!enoughSpace)
                {
                    processName = "Checking for enough space";
                    string errorMessage = "Not enough space in external storage device";
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.ConfigPathList, processName, errorMessage));
                }
                else // Might have enough space.
                {
                    processName = "Sync Config files from Computer to External Device";
                    CopyToExternalHelper(sa, targetGamePath, processName, CONFIG);
                }
            }
            //Copy saved game files
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                bool enoughSpace = true;
                try
                {
                    enoughSpace = CheckForEnoughSpace(sa.MyGame.SavePathList);
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException) { }

                //When not enough space in the target location, add all files to error list
                if (!enoughSpace)
                {
                    processName = "Checking for enough space";
                    string errorMessage = "Not enough space in external storage device";
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.SavePathList, processName, errorMessage));
                }
                else // Might have enough space
                {
                    processName = "Sync Saved Game files from Computer to External Device";
                    CopyToExternalHelper(sa, targetGamePath, processName, SAVED_GAME);
                }
            }
        }

        /// <summary>
        /// Private method that assist CopyToExternal.
        /// Help to set the parameter for the different kind of game file to sync.
        /// Copy a list of path and update the error list if sync problem encounted.
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="targetGamePath"></param>
        /// <param name="processName"></param>
        /// <param name="action"></param>
        private void CopyToExternalHelper(SyncAction sa, string targetGamePath, string processName, int action)
        {

            List<string> gameItemList = null;
            string targetFolderName = null;

            Debug.Assert(action == CONFIG || action == SAVED_GAME);

            //Set the variable for each case
            switch (action)
            {
                case CONFIG:
                    //Get the path list from intalled games
                    gameItemList = sa.MyGame.ConfigPathList;
                    //Set the name of the folder to copy to
                    targetFolderName = SyncFolderConfigFolderName;
                    break;

                case SAVED_GAME:
                    //Get the path list from intalled games
                    gameItemList = sa.MyGame.SavePathList;
                    //Set the name of the folder to copy to
                    targetFolderName = SyncFolderSavedGameFolderName;
                    break;
            }

            Debug.Assert(gameItemList != null);
            Debug.Assert(targetFolderName != null);

            //Copy a list of path and update the error list if sync problem encounted
            string targetGameSubPath = Path.Combine(targetGamePath, targetFolderName);
            List<SyncError> errorList;
            errorList = CopyPathListToExternal(gameItemList, processName, targetGameSubPath);
            sa.UnsuccessfulSyncFiles.AddRange(errorList);
        }

        /// <summary>
        /// Takes in a game object and look for a game in installedGameList that has the same game name.
        /// </summary>
        /// <param name="externalGame">The game to match for.</param>
        /// <returns>Return the first matched game in installedGameList based on matched game name.</returns>
        private Game FindInstalledGame(Game externalGame)
        {
            wantedGame = externalGame;
            Game installedGame = installedGameList.Find(MatchWantedGameName);
            Debug.Assert(installedGame != null);

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
            else return false;

        }

        /// <summary>
        /// Add all targeted sync files into error list.
        /// </summary>
        /// <param name="sa">The SyncAction which its game files (depending on it's sync action) will be added to the unsuccessful sync files list</param>
        /// <param name="gamePath">The game directory in the external device</param>
        /// <param name="errorMessage">The reason for call this method.</param>
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


        /// <summary>
        /// Copy the game files from external storage device to computer, provided backup was successful.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <param name="gameSourcePath">The directory in the external device where the games files are stored.</param>
        private void CopyToComputer(SyncAction sa, string gameSourcePath, int backupItem)
        {
            //Check for valid option
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles);
            Debug.Assert(sa.MyGame != null);

            string syncFolderGamePath = gameSourcePath;
            List<SyncError> errorList = null;


            //When original game files on the computer are safely backup, continue copy files over
            if (backupItem == CONFIG || backupItem == All_FILES)
            {
                string processName = "";
                bool enoughSpace = true;
                try
                {
                    enoughSpace = CheckForEnoughSpace(sa.MyGame.ConfigPathList);
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException) { }

                //When not enough space in the target location, add all files to error list
                if (!enoughSpace)
                {
                    processName = "Checking for enough space";
                    string errorMessage = "Not enough space in computer";
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.ConfigPathList, processName, errorMessage));
                }
                else
                {
                    processName = "Sync Config files from External Device to Computer";
                    string syncFolderGameConfigPath = Path.Combine(syncFolderGamePath, SyncFolderConfigFolderName);


                    //Copy over and add the error encounted into the error list
                    errorList = CopyDirectory(syncFolderGameConfigPath, sa.MyGame.ConfigParentPath, processName);

                    //Update the error list of the game
                    sa.UnsuccessfulSyncFiles.AddRange(errorList);

                    errorList = null;
                }
            }

            if (backupItem == SAVED_GAME || backupItem == All_FILES)
            {
                string processName = "";
                bool enoughSpace = true;
                try
                {
                    enoughSpace = CheckForEnoughSpace(sa.MyGame.ConfigPathList);
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException) { }

                //When not enough space in the target location, add all files to error list
                if (!enoughSpace)
                {
                    processName = "Checking for enough space";
                    string errorMessage = "Not enough space in computer";
                    sa.UnsuccessfulSyncFiles.AddRange(GetSyncError(sa.MyGame.SavePathList, processName, errorMessage));
                }
                else
                {
                    processName = "Sync Saved Game files from External Device to Computer";
                    string syncFolderGameSavedGamePath = Path.Combine(syncFolderGamePath, SyncFolderSavedGameFolderName);

                    Debug.Assert(Directory.Exists(syncFolderGameSavedGamePath));

                    //Copy over and add the error encounted into the error list
                    errorList = CopyDirectory(syncFolderGameSavedGamePath, sa.MyGame.SaveParentPath, processName);
                    //Update the error list of the game
                    sa.UnsuccessfulSyncFiles.AddRange(errorList);

                    errorList = null;
                }
            }
        }

        /// <summary>
        /// Saves original game files on the computer into a backup folder in the same directory.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
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
                        errorList.AddRange(GetSyncError(item, processDescription, ex.errorMessage));
                    }
                    catch (Exception ex)
                    {
                        errorList.AddRange(GetSyncError(item, processDescription, ex.Message));
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
                        errorList.AddRange(GetSyncError(item, processDescription, ex.errorMessage));
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

            if (gameItemList.Count == 0)
                return errorList;

            // Delete existing data in the target path
            if (Directory.Exists(syncFolderGameSubPath))
                try
                {
                    DeleteDirectory(syncFolderGameSubPath);
                }
                catch (DeleteDirectoryErrorException ex)
                {
                    errorList = GetSyncError(gameItemList, "Removing external game directory", ex.errorMessage);
                }

            try
            {
                CreateDirectory(syncFolderGameSubPath);
            }
            catch (CreateFolderFailedException ex)
            {
                return GetSyncError(gameItemList, processName, ex.errorMessage);
            }

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
            if (!Directory.Exists(sourcePath))
                return GetSyncError(sourcePath, processName, "Unable to create directory/Directory is missing - " + sourcePath + ".");

            Debug.Assert(targetPath != null);

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
        /// <returns>Path of the new directory.</returns>
        private void CreateDirectory(string newFolderPath)
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
        /// <returns>Path of the new directory.</returns>
        private void CreateDirectory(string parentDirectory, string folderName)
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
        /// <returns>A list of SyncError of the given path.</returns>
        private List<SyncError> GetSyncError(string errorPath, string processName, string errorMessage)
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


        /// <summary>
        /// Replace the saved game files and game configuration files with the backup files on the computer.
        /// Backup folders will be removed.
        /// </summary>
        /// <returns>The list of SyncAction that contain failed restore files.</returns>
        public List<SyncAction> Restore()
        {
            List<SyncAction> syncActionList = DetermineGamesWithBackup(installedGameList);
            int errorCounter = 0;
            foreach (SyncAction sa in syncActionList)
            {
                errorCounter += RestoreGame(sa, true);

                //When all files are safely copied over, then remove all backup folder
                if (errorCounter == 0)
                {
                    List<SyncAction> oneSyncAction = new List<SyncAction>();
                    oneSyncAction.Add(sa);
                    RemoveAllBackup(oneSyncAction);
                }

                errorCounter = 0;
            }

            return syncActionList;
        }

        /// <summary>
        /// Replace existing config and/or saved game files of a game on the computer with the ones in the backup folder.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's file information.</param>
        /// <returns>The number of SyncError encounted.</returns>
        private int RestoreGame(SyncAction sa, bool deleteCurrentFiles)
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
                    errorList.AddRange(Delete(sa.MyGame.ConfigParentPath, BackupConfigFolderName));
                    errorList.AddRange(Delete(sa.MyGame.SaveParentPath, BackupSavedGameFolderName));
                }
            }
            return errorList;
        }


        /// <summary>
        /// Delete the list of path that may contain backup folders into Recycle bin.
        /// 
        /// Exception: DeleteDirectoryErrorException() - Unable to remove backup folder.
        /// </summary>
        /// <param name="list">The list of path that contains backup folder</param>
        /// <param name="backupFolderName">The backup folder name</param>
        /// <returns>A list of SyncError of the given path list.</returns>
        private List<SyncError> Delete(string backupFolderParentPath, string backupFolderName)
        {
            List<SyncError> errorList = new List<SyncError>();
            //foreach (string path in backupFolderParentPathList)
            {
                string backupFolderPath = Path.Combine(backupFolderParentPath, backupFolderName);
                try
                {
                    if (Directory.Exists(backupFolderPath))
                        DeleteDirectory(backupFolderPath);
                }
                catch (DeleteDirectoryErrorException ex)
                {
                    string processName = "Remove backup file";
                    errorList.AddRange(GetSyncError(backupFolderParentPath + backupFolderName, processName, ex.errorMessage));
                }
            }
            return errorList;
        }

        /// <summary>
        /// Moves all the path in the path list into Recycle bin.
        /// </summary>
        /// <param name="pathList">The list of path to be deleted.</param>
        /// <returns></returns>
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
        /// Pre-Condition: direction is valid and pathList is not null.
        /// Post-Condition: Returns true if there is adequate space on corresponding hardware, false otherwise.
        /// 
        /// Description: Checks if the destination hardware has enough space to accommodate the new files.
        /// 
        /// Exceptions: DirectoryNotFoundException - directory is missing.
        ///             UnauthorizedAccessException - access is denied.
        /// </summary>
        /// <returns>true if there is adequate space, false otherwise</returns>
        private bool CheckForEnoughSpace(List<string> pathList)
        {
            if (pathList.Count == 0)
                return true;

            // Assert that direction is valid.
            Debug.Assert(syncDirection == ComToExternal || syncDirection == ExternalToCom, "Direction is invalid.");

            // Get all available drives detected by computer.
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            DriveInfo d = null;

            long sizeRequired = 0;

            // Get the space required by list of files for the particular game.
            sizeRequired += CalculateSpaceForFiles(pathList);

            // Get the destination drive information.
            if (syncDirection == ExternalToCom)
            {
                d = FindMatchingDrive(allDrives, Directory.GetDirectoryRoot(pathList[0]));
            }

            else if (syncDirection == ComToExternal)
            {
                d = FindMatchingDrive(allDrives, Application.StartupPath.Substring(0, 3));
            }

            // Check that the destination drive has enough space for the new files, with 2MB safety net.
            if (d.AvailableFreeSpace < sizeRequired)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// Pre-Conditions: allDrives and path are not null.
        /// Post-Conditions: Return a DriveInfo object matching the path's drive.
        /// 
        /// Description: Find the drive associated with the path given a list of drives.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="allDrives"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private DriveInfo FindMatchingDrive(DriveInfo[] allDrives, string path)
        {
            foreach (DriveInfo dinfo in allDrives)
            {
                if (dinfo.Name == path)
                {
                    return dinfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Pre-Condition: List is not null.
        /// Post-Condition: Total space required by list of files is returned in a long variable.
        /// 
        /// Description: Calculates the total space required for list of files.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="sa">SyncAction object</param>
        /// <returns>total space required for config files</returns>
        private long CalculateSpaceForFiles(List<string> pathList)
        {
            long sizeRequired = 0;

            foreach (string gameItem in pathList)
            {
                // Path is a file.
                if (File.Exists(gameItem))
                {
                    FileInfo f = new FileInfo(gameItem);
                    sizeRequired += f.Length;
                }

                // Path is a folder.
                else
                {
                    sizeRequired += DirSize(new DirectoryInfo(@gameItem));
                }
            }

            return sizeRequired;
        }


        /// <summary>
        /// Pre-Condition: Directory exists.
        /// Post-Condition: Size of directory is returned.
        /// 
        /// Description: Calculates the size of the directory. Taken from MSDN.
        /// 
        /// Exceptions: UnauthorizedAccessException - access denied.
        /// </summary>
        /// <param name="d">directory path</param>
        /// <returns>size of directory</returns>
        private long DirSize(DirectoryInfo d)
        {
            long Size = 0;

            // Add file sizes.
            FileInfo[] fis = new FileInfo[5];
            fis = d.GetFiles();


            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }

            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();

            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }

            return (Size);
        }

        /// <summary>
        /// Pre-Condition: None.
        /// Post-Condition: Backup are removed and a SyncError list is returned.
        /// 
        /// Description: Determines which games have backup and remove the backup of the games.
        /// 
        /// Exceptions: None.
        /// </summary>
        public List<SyncError> RemoveAllBackup()
        {
            List<SyncAction> syncActionList = DetermineGamesWithBackup(installedGameList);

            return RemoveAllBackup(syncActionList);
        }
    }
}