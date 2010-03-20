using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace GameAnywhere
{
    /// <summary>
    /// Method to synchronize Game files between computer and external storage device. 
    /// 
    /// </summary>
    class OfflineSync : Sync
    {
        //Sync direction
        /// <summary>
        /// Sync from external storage to computer.
        /// </summary>
        public static readonly int ExternalToCom = 1;

        /// <summary>
        /// Sync from compter to external storage.
        /// </summary>
        public static readonly int ComToExternal = 2;

        /// <summary>
        /// Uninitialize sync direction. Default value.
        /// </summary>
        public const int Uninitialize = 0; //Default

        /// <summary>
        /// Diretion of sync, can only be set once.
        /// </summary>
        private int syncDirection = Uninitialize;

        /// <summary>
        /// List of sync action to be carried out.
        /// </summary>
        private List<SyncAction> syncActionList;

        /// <summary>
        /// Default syncFolder is located in the same directory as the current executable program.
        /// </summary>
        private string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");

        /// <summary>
        /// Stores game information on the computer.
        /// </summary>
        private List<Game> installedGameList;

        /// <summary>
        /// Constructor.
        /// 
        /// Exception:
        /// CreateFolderFailedException() throw when unable to create a syncFolder in the current GameAnywhere.exe directory.
        /// </summary>
        public OfflineSync()
        {
            //Make a default storage directory on the external storage device
            //Throws exception when failed
            CreateDirectory(syncFolderPath);
        }

        /// <summary>
        /// Overloaded Constructor.
        ///      
        /// Exception:
        /// CreateFolderFailedException() throw when unable to create a syncFolder in the current GameAnywhere.exe directory.
        /// </summary>
        /// <param name="direction">Synchronization direction</param>
        /// <param name="gameList">A list of installed games on the computer</param>
        public OfflineSync(int direction, List<Game> gameList)
        {
            syncDirection = direction;
            installedGameList = gameList;

            //Make a default storage directory on the external storage device
            //Throws exception when failed
            CreateDirectory(syncFolderPath);
        }

        /// <summary>
        /// Execute the synchronization for all the games in the sync action list.
        /// 
        /// Given the direction of synchronization, SynchronizeGames will copy saved game files and/or game configuration files between computer and external storage device. 
        /// Original game files on computer will be backup into a GA-Backup folder in the same directory as the game file before overwriting.
        /// 
        /// syncActionList.unsuccessfulSyncFiles would contain the list of game files that could not be synced.
        /// </summary>
        /// <param name="list">List of games which are to be synchronized.</param>
        /// <returns>List of games and their sync results.</returns>
        public override List<SyncAction> SynchronizeGames(List<SyncAction> list)
        {
            Debug.Assert(syncDirection == ComToExternal || syncDirection == ExternalToCom);

            syncActionList = list;

            if (syncActionList.Count == 0)
                return syncActionList;

            //Iterate through each game
            foreach (SyncAction sa in syncActionList)
            {
                Debug.Assert(sa.MyGame != null);

                string syncFolderGamePath = Path.Combine(syncFolderPath, sa.MyGame.Name);

                //Copy game files from external device to computer
                if (syncDirection == ExternalToCom)
                {
                    //Backup original game files
                    int backupResult = Backup(sa);
                    CopyToComputer(sa, syncFolderGamePath, backupResult);
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
        /// Copy game files from computer to external storage device.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <param name="targetGamePath">The destination folder in the external storage device.</param>
        private void CopyToExternal(SyncAction sa, string targetGamePath)
        {
            string processName;
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
                List<SyncError> errorList;
                processName = "Sync Config files from Computer to External Device";
                errorList = CopyToExternalHelper(sa.MyGame.ConfigPathList, targetGamePath, processName, SyncFolderConfigFolderName);
                sa.UnsuccessfulSyncFiles.AddRange(errorList);
            }
            //Copy saved game files
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                List<SyncError> errorList;
                processName = "Sync Saved Game files from Computer to External Device";
                errorList = CopyToExternalHelper(sa.MyGame.SavePathList, targetGamePath, processName, SyncFolderSavedGameFolderName);
                sa.UnsuccessfulSyncFiles.AddRange(errorList);
            }
        }

        /// <summary>
        /// Private method that assist CopyToExternal.
        /// Help to set the parameter for the different kind of game file to sync.
        /// Copy a list of path and update the error list if sync problem encounted.
        /// </summary>
        /// <param name="gameItemList">The list of game config/saved path.</param>
        /// <param name="targetGamePath">Destination path.</param>
        /// <param name="processName">Process description.</param>
        /// <param name="targetFolderName">The name of the folder to copy to.</param>
        /// <returns>The list of sync error encountered in this process.</returns>
        private List<SyncError> CopyToExternalHelper(List<string> gameItemList, string targetGamePath, string processName, string targetFolderName)
        {
            //Check for space
            bool enoughSpace = true;
            try
            {
                enoughSpace = CheckForEnoughSpace(gameItemList);
            }
            catch (DirectoryNotFoundException) { }
            catch (UnauthorizedAccessException) { }

            //When not enough space in the target location, add all files to error list
            if (!enoughSpace)
            {
                processName = "Checking for enough space";
                string errorMessage = "Not enough space in external storage device";
                return GetSyncError(gameItemList, processName, errorMessage);
            }

            // Might have enough space

            Debug.Assert(gameItemList != null);
            Debug.Assert(targetFolderName != null);

            //Copy a list of path and update the error list if sync problem encounted
            string targetGameSubPath = Path.Combine(targetGamePath, targetFolderName);

            return CopyPathListToExternal(gameItemList, processName, targetGameSubPath);
        }

        /// <summary>
        /// Takes in a game object and look for a game in installedGameList that has the same game name.
        /// </summary>
        /// <param name="externalGame">The game to match for.</param>
        /// <returns>Return the first matched game in installedGameList based on matched game name.</returns>
        public Game FindInstalledGame(Game externalGame)
        {
            return FindInstalledGame(externalGame, installedGameList);
        }

        /// <summary>
        /// Saves original game files on the computer into a backup folder in the same directory.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <returns>The backup status status, indicating what has been successfully backup.</returns>
        public int Backup(SyncAction sa)
        {
            return Backup(sa, installedGameList);
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
            return RemoveAllBackup(installedGameList);
        }

        /// <summary>
        /// Replace the saved game files and game configuration files with the backup files on the computer.
        /// Backup folders will be removed.
        /// </summary>
        /// <returns>The list of SyncAction that contain failed restore files.</returns>
        public List<SyncAction> Restore()
        {
            return Restore(installedGameList);
        }

        /// <summary>
        /// Copy the game files from external storage device to computer, provided backup was successful.
        /// </summary>
        /// <param name="sa">The SyncAction that contains a game and it's synchronizing information.</param>
        /// <param name="gameSourcePath">The directory in the external device where the games files are stored.</param>
        /// <param name="backupItem">The backup status, indicating what has been successsfully backup.</param>
        private void CopyToComputer(SyncAction sa, string gameSourcePath, int backupItem)
        {
            if (backupItem == None) //Backup was not successful
            {
                //Add all game files to error list
                AddToUnsuccessfulSyncFiles(sa, gameSourcePath, "Unable to backup original game files.");
                return;
            }

            //Check for valid option
            Debug.Assert(sa != null);
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles);
            Debug.Assert(sa.MyGame != null);
            Debug.Assert(Directory.Exists(gameSourcePath));

            string processName;

            //When original game files on the computer are safely backup, continue copy files over
            if (backupItem == Config || backupItem == AllFiles)
            {
                processName = "Sync Config files from External Device to Computer";
                List<SyncError> errorList;
                errorList = CopyToComputerHelper(sa.MyGame.ConfigPathList, processName, gameSourcePath, SyncFolderConfigFolderName, sa.MyGame.ConfigParentPath);

                sa.UnsuccessfulSyncFiles.AddRange(errorList);
            }

            if (backupItem == SavedGame || backupItem == AllFiles)
            {
                processName = "Sync Saved Game files from External Device to Computer";
                List<SyncError> errorList;
                errorList = CopyToComputerHelper(sa.MyGame.SavePathList, processName, gameSourcePath, SyncFolderSavedGameFolderName, sa.MyGame.SaveParentPath);

                sa.UnsuccessfulSyncFiles.AddRange(errorList);
            }
        }

        /// <summary>
        /// Copy a list of path from syncFolder to the game directory on the computer.
        /// </summary>
        /// <param name="gameItemList">Either list of config path or saved game path.</param>
        /// <param name="processName">A process description.</param>
        /// <param name="gameSourcePath">The game path in syncFolder.</param>
        /// <param name="sourceFolderName">The source folder name in the gameSourcePath.</param>
        /// <param name="gameItemParentPath">The destination path on the computer.</param>
        /// <returns>The list of sync error encountered in this process.</returns>
        private List<SyncError> CopyToComputerHelper(List<string> gameItemList, string processName, string gameSourcePath, string sourceFolderName, string gameItemParentPath)
        {
            List<SyncError> errorList = new List<SyncError>();
            bool enoughSpace = true;
            try
            {
                enoughSpace = CheckForEnoughSpace(gameItemList);
            }
            //Exception will be handled later in CopyDirectory()
            catch (DirectoryNotFoundException) { }
            catch (UnauthorizedAccessException) { }

            if (enoughSpace)//Enough space, contine copy.
            {
                string syncFolderGameItemPath = Path.Combine(gameSourcePath, sourceFolderName);

                Debug.Assert(Directory.Exists(syncFolderGameItemPath));

                //Copy over and add the error encounted into the error list
                errorList.AddRange(CopyDirectory(syncFolderGameItemPath, gameItemParentPath, processName));

            }
            else //When not enough space in the target location, add all files to error list
            {
                string currentProcessName = "Checking for enough space";
                string errorMessage = "Not enough space in computer";
                errorList.AddRange(GetSyncError(gameItemList, currentProcessName, errorMessage));
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
        /// <param name="errorPathList">The list of file path that failed in a process.</param>
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
        /// <param name="backupFolderParentPath">The list of path that contains backup folder</param>
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
            if (d != null)
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
        /// <param name="pathList">List of path</param>
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


    }
}