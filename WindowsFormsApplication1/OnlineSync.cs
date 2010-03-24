using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GameAnywhere
{
    class OnlineSync : Sync
    {
        //Sync directions
        public static readonly int WebToCom = 4;
        public static readonly int ComToWeb = 5;

        /// <summary>
        /// Data Members
        /// </summary>
        private User currentUser;
        private Storage s3;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="direction">direction of online sync</param>
        /// <param name="gameList">list of games</param>
        /// <param name="user">user object</param>
        public OnlineSync(int direction, List<Game> gameList, User user)
        {
            SyncDirection = direction;
            installedGameList = gameList;
            currentUser = user;
            s3 = new Storage();
        }

        /// <summary>
        /// Synchronize games from web to computer / computer to web
        /// </summary>
        /// <param name="list">list of syncactions</param>
        /// <returns>list of syncactions containing the syncerrors if there is any</returns>
        public override List<SyncAction> SynchronizeGames(List<SyncAction> list)
        {
            //List of sync action to be carried out
            syncActionList = list;

            if (syncActionList.Count == 0)
                return syncActionList;

            //Carry out each sync action
            foreach (SyncAction sa in syncActionList)
            {
                Debug.Assert(sa.MyGame != null);
                
                string syncFolderGamePath = Path.Combine(syncFolderPath, sa.MyGame.Name);

                if (SyncDirection == WebToCom)
                {
                    //Backup files on computer
                    int backupResult = Backup(sa);
                    if (backupResult == None)
                        //Backup was not successful - Add all game files to error list
                        AddToUnsuccessfulSyncFiles(sa, syncFolderGamePath, "Unable to backup original game files");
                    else
                        //Synchronize from Web to Computer
                        WebToComputer(sa, currentUser.Email, backupResult);
                }
                else if (SyncDirection == ComToWeb)
                {
                    //Synchronize from Computer to Web
                    ComputerToWeb(currentUser.Email, sa);
                }
           
            }
            return syncActionList;
        }

        /// <summary>
        /// Get the list of game names that are synced on the user's web account (S3)
        /// </summary>
        /// <param name="user">email of user</param>
        /// <returns>list of game names on the user's web account</returns>
        public List<string> GetGamesFromWeb(string user)
        {
            try
            {
                List<string> games = s3.ListFiles(user);
                HashSet<string> gamesList = new HashSet<string>();
                foreach (string game in games)
                {
                    string gameName = game.Replace(user + "/", "");
                    int length = gameName.IndexOf("/");
                    if (length < 0) continue;
                    gameName = gameName.Substring(0, length);
                    if (!gamesList.Contains(gameName))
                    {
                        gamesList.Add(gameName);
                    }
                }

                return gamesList.ToList<string>();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get the list of game names and types that are synced on the user's web account (S3)
        /// e.g. Warcraft3/config
        /// </summary>
        /// <param name="user">email of user</param>
        /// <returns>list of game names and types on the user's web account</returns>
        public static List<string> GetGamesAndTypesFromWeb(string user)
        {
            Storage s3 = new Storage();
            try
            {
                //Pre-conditions
                if (user.Trim().Equals("") || user == null)
                    throw new ArgumentException("Parameter cannot be empty/null", "user");

                HashSet<string> gameSet = new HashSet<string>();

                //Get game name and type(config/save), and add to gameset
                foreach (string file in s3.ListFiles(user))
                {
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
            catch
            {
                //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                throw;
            }
        }
        /// <summary>
        /// Upload files from computer to the web(S3)
        /// </summary>
        /// <param name="email">email of user</param>
        /// <param name="syncAction">SyncAction object</param>
        private void ComputerToWeb(string email, SyncAction syncAction)
        {
            //Pre-conditions
            Debug.Assert(email != null && !email.Equals(""));
            Debug.Assert(syncAction.Action == SyncAction.ConfigFiles || syncAction.Action == SyncAction.SavedGameFiles || syncAction.Action == SyncAction.AllFiles);
            Debug.Assert(syncAction.MyGame != null);

            //Initialise variables from syncAction
            string gameName = syncAction.MyGame.Name;
            string saveParentPath = syncAction.MyGame.SaveParentPath;
            string configParentPath = syncAction.MyGame.ConfigParentPath;
            List<string> savePathList = syncAction.MyGame.SavePathList;
            List<string> configPathList = syncAction.MyGame.ConfigPathList;
            int action = syncAction.Action;
            List<SyncError> saveGameErrorList = new List<SyncError>();
            List<SyncError> configFileErrorList = new List<SyncError>();

            try
            {
                //Check if game is available on user's web account(S3)
                if (GetGamesFromWeb(email).Contains(gameName))
                {
                    //Delete game directory in user's account
                    DeleteGameDirectory(email, gameName);
                }
            }
            catch (Exception ex)
            {
                //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                syncAction.UnsuccessfulSyncFiles.Add(new SyncError(@"email/gameName", "ComputerToWeb Sync - " + ex.GetType().ToString(), ex.Message));
            }

            //Upload Saved Files - SyncAction
            if (action == SyncAction.SavedGameFiles|| action == SyncAction.AllFiles)
            {
                saveGameErrorList = Upload(email, gameName, SyncFolderSavedGameFolderName, saveParentPath, savePathList);
            }

            //Upload Config Files - SyncAction
            if (action == SyncAction.ConfigFiles || action == SyncAction.AllFiles)
            {
                configFileErrorList = Upload(email, gameName, SyncFolderConfigFolderName, configParentPath, configPathList);
            }

            //Updates list of unsuccessful sync files
            syncAction.UnsuccessfulSyncFiles.AddRange(saveGameErrorList);
            syncAction.UnsuccessfulSyncFiles.AddRange(configFileErrorList);
            savePathList = null;
            configPathList = null;
            saveGameErrorList = null;
            configFileErrorList = null;
        }

        /// <summary>
        /// Upload config/save files to web(user's S3 account)
        /// </summary>
        /// <param name="email">email of user</param>
        /// <param name="gameName">name of game</param>
        /// <param name="webSaveFolder">SyncSavedGameFolderName or SyncConfigFolderName</param>
        /// <param name="parentPath">Config/Save parent path</param>
        /// <param name="pathList">List of Config/Save game files & directories</param>
        /// <returns>
        /// List of SyncError objects which contains errors during uploading
        /// </returns>
        private List<SyncError> Upload(string email, string gameName, string webSaveFolder, string parentPath, List<string> pathList)
        {
            string gamePath = email + "/" + gameName + "/" + webSaveFolder;
            List<SyncError> errorList = new List<SyncError>();

            //Upload list of config/save files/dir
            foreach (string path in pathList)
            {
                if (Directory.Exists(path))
                {
                    //Upload directory
                    UploadDirectory(gamePath, parentPath, path, ref errorList);
                }
                else if (File.Exists(path))
                {
                    string keyName = gamePath + path.Replace(parentPath, "").Replace(@"\", "/");
                    try
                    {
                        //Upload file
                        s3.UploadFile(path, keyName);
                    }
                    catch (Exception ex)
                    {
                        //Fail to upload file - add to error list
                        //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                        errorList.AddRange(GetSyncError(path, "ComputerToWeb Sync - Upload->UploadFile", ex.Message));
                    }
                }
                else
                {
                    //File/Dir does not exists - add to error list
                    errorList.AddRange(GetSyncError(path, "ComputerToWeb Sync - Upload", "File/Directory does not exist."));
                }
            }

            return errorList;
        }

        /// <summary>
        /// Upload a directory to web(S3)
        /// </summary>
        /// <param name="key">key of user's save/config web directory</param>
        /// <param name="parent">Config/Save parent path</param>
        /// <param name="dir">directory to upload</param>
        /// <param name="errorList">reference to errorList</param>
        private void UploadDirectory(string key, string parent, string dir, ref List<SyncError> errorList)
        {
            //Upload every file in current directory
            foreach (string filePath in Directory.GetFiles(dir))
            {
                string keyName = key + filePath.Replace(parent, "").Replace(@"\", "/");
                try
                {
                    //Upload a single file
                    s3.UploadFile(filePath, keyName);
                }
                catch (Exception ex)
                {
                    //Adds error due to file upload
                    //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                    errorList.Add(new SyncError(filePath, "UploadDirectory", ex.Message));
                }
            }
            //Upload every sub-directory in current directory
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                UploadDirectory(key, parent, subdir, ref errorList);
            }
        }

        /// <summary>
        /// Delete a game directory in S3
        /// </summary>
        /// <param name="email">email of user</param>
        /// <param name="gameName">name of game</param>
        /// <returns>
        /// true - Deletes game directory successfully on S3
        /// false - Fail to delete game directory on S3
        /// </returns>
        /// Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
        private void DeleteGameDirectory(string email, string gameName)
        {
            try
            {
                //Pre-conditions
                Debug.Assert(GetGamesFromWeb(email).Contains(gameName));

                //Checks user and gamename validity
                if (email.Equals("") && email == null && !email.Equals(currentUser.Email))
                    throw new ArgumentException("Parameter cannot be empty/null. Invalid user/User not logged in", email);
                if (gameName.Equals("") && gameName == null)
                    throw new ArgumentException("Parameter cannot be empty/null. Invalid game directory.", gameName);

                //Delete game directory
                s3.DeleteDirectory(email + "/" + gameName);
            }
            catch
            {
                //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                throw;
            }
        }

        /// <summary>
        /// Download files/dirs to computer
        /// </summary>
        /// <param name="sa">SyncAction object</param>
        /// <param name="user">email of user</param>
        /// <param name="backupItem">Backup-type of files: AllFiles / SavedGame / Config</param>
        private void WebToComputer(SyncAction sa, string user, int backupItem)
        {
            //Check for valid option
            Debug.Assert(sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles);
            Debug.Assert(sa.MyGame != null);

            List<SyncError> errorList = null;

            //Sync config files
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

            //Sync saved game files
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

        /// <summary>
        /// Download files from S3 to computer
        /// </summary>
        /// <param name="s3Path">path of file</param>
        /// <param name="targetPath">path to download to on computer</param>
        /// <param name="processName">name of sync action</param>
        /// <returns>list of SyncError objects</returns>
        private List<SyncError> DownloadToParent(string s3Path, string targetPath, string processName)
        {
            //Pre-conditions
            Debug.Assert(targetPath != null);

            List<SyncError> errorList = new List<SyncError>();
            List<string> files = s3.ListFiles(s3Path);
            string localDirName, webDirName;

            foreach (string file in files)
            {
                //Console.WriteLine(file);
                //Path of file - <folder>/<file> from <user>/<game>/<config or save folder>/<folder>/<file>
                webDirName = GetWebFolderName(file);

                //Path of file on computer
                localDirName = Path.Combine(targetPath, webDirName);
                //Console.WriteLine(localDirName);

                try
                {
                    //Create the target directory if needed
                    //Console.WriteLine(Path.GetDirectoryName(localDirName));
                    if (!Directory.Exists(Path.GetDirectoryName(localDirName)))
                        CreateDirectory(Path.GetDirectoryName(Path.Combine(targetPath, webDirName)));
                }
                catch (CreateFolderFailedException ex)
                {
                    //Add to error list -  fail to create folder
                    errorList.AddRange(GetSyncError(file, processName, ex.Message));
                }

                try
                {
                    //Download file from S3 to computer
                    s3.DownloadFile(Path.Combine(targetPath, webDirName), file);
                }
                catch (Exception ex)
                {
                    //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, AmazonS3Exception
                    //Add to error list
                    errorList.AddRange(GetSyncError(file, processName, ex.Message));
                }
            }

            return errorList;
        }

        /// <summary>
        /// Gets the <folder>/<file> from <user>/<game>/<config or save folder>/<folder>/<file> on S3
        /// </summary>
        /// <param name="key">path of file/folder</param>
        /// <returns>file path without user/game/config or save folder</returns>
        private string GetWebFolderName(string key)
        {
            //Pre-conditions
            Debug.Assert(!key.Trim().Equals("") && key != null);

            string filter;
            filter = key.Substring(key.IndexOf('/') + 1);
            filter = filter.Substring(filter.IndexOf('/') + 1);
            filter = filter.Substring(filter.IndexOf('/') + 1);

            return filter;
        }
    }
}