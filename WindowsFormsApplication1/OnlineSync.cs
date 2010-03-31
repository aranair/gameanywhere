using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GameAnywhere
{
    /// <summary>
    /// Online synchronization contains one way sync methods over the web.
    /// Game files can be synchronized from Computer to Web / Web to Computer.
    /// </summary>
    class OnlineSync : Sync
    {
        #region Data Members
        /// <summary>
        /// Sync direction, from Web to Computer.
        /// </summary>
        public static readonly int WebToCom = 4;

        /// <summary>
        /// Sync direction, from Computer to Web.
        /// </summary>
        public static readonly int ComToWeb = 5;

        /// <summary>
        /// Sync direction, two way sync between External storage and Web.
        /// </summary>
        public static readonly int ExternalAndWeb = 6;

        /// <summary>
        /// Stores the current information of information.
        /// </summary>
        private User currentUser;

        /// <summary>
        /// To access Amazon Web Services S3.
        /// </summary>
        private Storage s3;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes data members.
        /// </summary>
        /// <param name="direction">Synchronization direction.</param>
        /// <param name="gameList">A list of installed games on the computer.</param>
        /// <param name="user">User account.</param>
        public OnlineSync(int direction, List<Game> gameList, User user)
        {
            SyncDirection = direction;
            installedGameList = gameList;
            currentUser = user;
            s3 = new Storage();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Synchronize all games in the sync action list, from Web to Computer / Computer to Web.
        /// </summary>
        /// <param name="list">List of games which are to be synchronized.</param>
        /// <returns>List of games and their sync results.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public override List<SyncAction> SynchronizeGames(List<SyncAction> list)
        {
            //List of sync action to be carried out.
            syncActionList = list;

            if (syncActionList.Count == 0)
                return syncActionList;

            //Carry out each sync action.
            foreach (SyncAction sa in syncActionList)
            {
                Debug.Assert(sa.MyGame != null);
                
                string syncFolderGamePath = Path.Combine(syncFolderPath, sa.MyGame.Name);

                if (SyncDirection == WebToCom)
                {
                    //Backup files on computer.
                    int backupResult = Backup(sa);
                    if (backupResult == None)
                        //Backup was not successful - Add all game files to error list.
                        AddToUnsuccessfulSyncFiles(sa, syncFolderGamePath, "Unable to backup original game files");
                    else
                        //Synchronize from Web to Computer.
                        try
                        {
                            WebToComputer(sa, currentUser.Email, backupResult);
                        }
                        catch(ConnectionFailureException)
                        {
                            throw;
                        }
                }
                else if (SyncDirection == ComToWeb)
                {
                    //Synchronize from Computer to Web.
                    try
                    {
                        ComputerToWeb(currentUser.Email, sa);
                    }
                    catch (ConnectionFailureException)
                    {
                        throw;
                    }
                }
           
            }
            return syncActionList;
        }

        /// <summary>
        /// Get the list of game names that are synced on the user's web account.
        /// </summary>
        /// <param name="user">Email of user's account.</param>
        /// <returns>List of game names on the user's web account.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="WebTransferException"></exception>
        public List<string> GetGamesFromWeb(string user)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(user.Trim()))
                throw new ArgumentException("Parameter cannot be empty/null", "user");

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
                //Exceptions: ConnectionFailureException, WebTransferException
                throw;
            }
        }

        /// <summary>
        /// Get the list of game names and types that are synced on the user's web account.
        /// </summary>
        /// <remarks>A sample of the game name with the type "Warcraft 3/config".</remarks>
        /// <param name="user">Email of user's account.</param>
        /// <returns>List of game names and types on the user's web account.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="WebTransferException"></exception>
        public static List<string> GetGamesAndTypesFromWeb(string user)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(user))
                throw new ArgumentException("Parameter cannot be empty/null", "user");

            try
            {
                Storage s3 = new Storage();
                HashSet<string> gameSet = new HashSet<string>();

                //Get game name and type(config/save), and add to gameset
                List<string> files = s3.ListFiles(user);
                foreach (string file in files)
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
                //Exceptions: ConnectionFailureException, WebTransferException
                throw;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Upload game files from computer to the web.
        /// </summary>
        /// <param name="email">Email of user's account.</param>
        /// <param name="syncAction">SyncAction that contains a game and it's synchronizing information.</param>
        /// <exception cref="ConnectionFailureException"></exception>
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
            catch (Exception)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }


            //Upload Saved Files
            if (action == SyncAction.SavedGameFiles || action == SyncAction.AllFiles)
            {
                try
                {
                    saveGameErrorList = Upload(email, gameName, SyncFolderSavedGameFolderName, saveParentPath, savePathList);
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }
            }

            //Upload Config Files
            if (action == SyncAction.ConfigFiles || action == SyncAction.AllFiles)
            {
                try
                {
                    configFileErrorList = Upload(email, gameName, SyncFolderConfigFolderName, configParentPath, configPathList);
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }
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
        /// Upload config/save files to user's web account.
        /// </summary>
        /// <param name="email">Email of user's account.</param>
        /// <param name="gameName">Name of a game.</param>
        /// <param name="webSaveFolder">SyncSavedGameFolderName or SyncConfigFolderName.</param>
        /// <param name="parentPath">Config/Save parent path.</param>
        /// <param name="pathList">List of Config/Save game files & directories.</param>
        /// <returns>
        /// List of SyncError which contains errors during uploading.
        /// </returns>
        /// <exception cref="ConnectionFailureException"></exception>
        private List<SyncError> Upload(string email, string gameName, string webSaveFolder, string parentPath, List<string> pathList)
        {
            string gamePath = email + "/" + gameName + "/" + webSaveFolder;
            List<SyncError> errorList = new List<SyncError>();

            //Upload list of config/save files/dir
            foreach (string path in pathList)
            {
                if (Directory.Exists(path))
                {
                    if (!IsLocked(path))
                    {
                        //Upload directory
                        try
                        {
                            UploadDirectory(gamePath, parentPath, path, ref errorList);
                        }
                        catch (ConnectionFailureException)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        errorList.Add(new SyncError(path, "Upload directory", "Access is denied to " + path + "."));
                    }
                }
                else if (File.Exists(path))
                {
                    string keyName = gamePath + path.Replace(parentPath, "").Replace(@"\", "/");
                    if (!IsLocked(path))
                    {
                        try
                        {
                            //Upload file
                            s3.UploadFile(path, keyName);
                        }
                        catch (Exception)
                        {
                            throw new ConnectionFailureException("Unable to connect to web server.");
                        }
                    }
                    else
                    {
                        errorList.Add(new SyncError(path, "ComputerToWeb Sync - Upload file", "Access is denied to " + path + "."));
                    }
                }
                else
                {
                    //File/Dir does not exists - add to error list
                    errorList.Add(new SyncError(path, "ComputerToWeb Sync", "File/Directory does not exist - " + path + "."));
                }
            }

            return errorList;
        }

        /// <summary>
        /// Upload a directory to user's web account.
        /// </summary>
        /// <param name="key">Key of user's save/config web directory.</param>
        /// <param name="parent">Config/Save parent path.</param>
        /// <param name="dir">Directory to be uploaded.</param>
        /// <param name="errorList">List of errors.</param>
        /// <exception cref="ConnectionFailureException"></exception>
        private void UploadDirectory(string key, string parent, string dir, ref List<SyncError> errorList)
        {
            //Upload every file in current directory
            foreach (string filePath in Directory.GetFiles(dir))
            {
                string keyName = key + filePath.Replace(parent, "").Replace(@"\", "/");
                try
                {
                    if(!IsLocked(filePath))
                    {
                        //Upload a single file
                        s3.UploadFile(filePath, keyName);
                    }
                    else
                    {
                        errorList.Add(new SyncError(filePath, "UploadDirectory", "Access is denied to " + filePath + "."));
                    }
                }
                catch (Exception)
                {
                    throw new ConnectionFailureException("Unable to connect to web server.");
                }
                
            }
            //Upload every sub-directory in current directory
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                try
                {
                    UploadDirectory(key, parent, subdir, ref errorList);
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete a game directory in S3.
        /// </summary>
        /// <param name="email">Email of user's account.</param>
        /// <param name="gameName">Name of a game.</param>
        /// <returns>
        /// True - Deletes game directory successfully on S3.
        /// False - Fail to delete game directory on S3.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="WebTransferException"></exception>
        private void DeleteGameDirectory(string email, string gameName)
        {
            try
            {
                //Checks user and gamename validity
                if (email.Equals("") && email == null && !email.Equals(currentUser.Email))
                    throw new ArgumentException("Parameter cannot be empty/null. Invalid user/User not logged in", email);
                if (gameName.Equals("") && gameName == null)
                    throw new ArgumentException("Parameter cannot be empty/null. Invalid game directory.", gameName);

                //Delete game directory
                s3.DeleteDirectory(email + "/" + gameName);
            }
            catch(Exception)
            {
                //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException
                throw;
            }
        }

        /// <summary>
        /// Download game files and directories to computer.
        /// </summary>
        /// <param name="sa">SyncAction that contains a game and it's synchronizing information.</param>
        /// <param name="user">Email of user's account.</param>
        /// <param name="backupItem">Backup-type of files: AllFiles / SavedGame / Config.</param>
        /// <exception cref="ConnectionFailureException"></exception>
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

                try
                {
                    //Copy over and add the error encounted into the error list
                    errorList = DownloadToParent(webGameConfigPath, sa.MyGame.ConfigParentPath, processName);
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }
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

                try
                {
                    //Copy over and add the error encounted into the error list
                    errorList = DownloadToParent(webGameSavedGamePath, sa.MyGame.SaveParentPath, processName);
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }

                //Update the error list of the game
                sa.UnsuccessfulSyncFiles.AddRange(errorList);

                errorList = null;
            }
        }

        /// <summary>
        /// Download files from user's web account to computer.
        /// </summary>
        /// <param name="s3Path">Path of file in S3.</param>
        /// <param name="targetPath">Path to download to on computer.</param>
        /// <param name="processName">Name of sync action.</param>
        /// <returns>List of errors during download.</returns>
        /// <exception cref="ConnectionFailureException"></exception>
        private List<SyncError> DownloadToParent(string s3Path, string targetPath, string processName)
        {
            //Pre-conditions
            Debug.Assert(targetPath != null);

            List<string> files = new List<string>();
            List<SyncError> errorList = new List<SyncError>();
            string localDirName, webDirName;

            try
            {
                files = s3.ListFiles(s3Path);
            }
            catch (Exception)
            {
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
            

            foreach (string file in files)
            {
                try
                {
                    //Path of file - <folder>/<file> from <user>/<game>/<config or save folder>/<folder>/<file>
                    webDirName = GetWebFolderName(file);

                    //Path of file on computer
                    localDirName = Path.Combine(targetPath, webDirName);

                    //Create the target directory if needed
                    if (!Directory.Exists(Path.GetDirectoryName(localDirName)))
                        CreateDirectory(Path.GetDirectoryName(Path.Combine(targetPath, webDirName)));

                    //Download file from S3 to computer
                    s3.DownloadFile(Path.Combine(targetPath, webDirName), file);
                }
                catch (IOException ex)
                {
                    errorList.Add(new SyncError(file, processName, ex.Message));
                }
                catch (UnauthorizedAccessException ex)
                {
                    errorList.Add(new SyncError(file, processName, ex.Message));
                }
                catch (CreateFolderFailedException ex)
                {
                    errorList.Add(new SyncError(file, processName, ex.errorMessage));
                }
                catch (ConnectionFailureException)
                {
                    throw;
                }
                
            }

            return errorList;
        }

        /// <summary>
        /// Gets the "<folder>/<file>" from "<user>/<game>/<config or save folder>/<folder>/<file>".
        /// </summary>
        /// <param name="key">Path to a file or folder.</param>
        /// <returns>File path to a file or folder.</returns>
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

        /// <summary>
        /// Check if file access is denied.
        /// </summary>
        /// <param name="filePath">Path to a file.</param>
        /// <returns>
        /// True - file is not read only.
        /// False - file is read only.
        /// </returns>
        private bool IsLocked(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        #endregion
    }
}