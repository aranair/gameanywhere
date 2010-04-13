//Goh Haoyu Gerald
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.IO;
using GameAnywhere.Data;
using GameAnywhere.Interface;

namespace GameAnywhere.Process
{
    /// <summary>
    /// This class is the middle-man between the GUI class and the other classes. It determines which
    /// methods to call at the appropriate moment.
    /// </summary>
    public class Controller
    {
        #region Data Members

        /// <summary>
        /// These data members help to mediate processing between GUI class and the other classes.
        /// </summary>
        private GameLibrary gameLibrary;
        private User user;
        private WebAndThumbSync wats;

        /// <summary>
        /// These values need to be set and get by GUI for the correct flow of the program.
        /// </summary>
        /// <value>
        /// Synchronization direction.
        /// </value>
        public int direction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes data members and calls a function to start up GUI.
        /// </summary>
        public Controller()
        {
            gameLibrary = new GameLibrary();
            direction = 0;
            user = new User();
            StartGUI();
        }

        #endregion

        #region Synchronization Methods

        /// <summary>
        /// Loads the GUI start page.
        /// </summary>
        private void StartGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new startPage(this));
        }

        /// <summary>
        /// Determines if the executable is running on the computer or an external drive.
        /// </summary>
        /// <returns>True if it is running on the computer, false otherwise.</returns>
        public bool IsFixedMedia()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            DriveInfo d = null;

            string driveName = Application.StartupPath.Substring(0, 3);

            foreach (DriveInfo e in allDrives)
            {
                if (e.Name.Equals(driveName))
                {
                    d = e;
                    break;
                }
            }

            if (d.DriveType.ToString().Equals("Fixed"))
                return true;

            return false;

        }

        /// <summary>
        /// Calls the appropriate GetGameList method of GameLibrary class depending on the direction
        /// of the synchronization.
        /// </summary>
        /// <returns>List of games available to be synchronized for the specific direction.</returns>
        public List<Game> GetGameList()
        {
            Debug.Assert(direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom
                || direction == OnlineSync.WebToCom || direction == OnlineSync.ComToWeb
                , "Direction is not valid.");

            List<Game> gameList = null;

            if (direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom || direction == OnlineSync.ComToWeb)
            {
                gameList = gameLibrary.GetGameList(direction);
            }

            else if (direction == OnlineSync.WebToCom)
            {
                try
                {
                    // Obtain games' information from the web first then pass to GameLibrary to process
                    List<string> webGamesList = OnlineSync.GetGamesAndTypesFromWeb(user.Email);
                    gameList = gameLibrary.GetGameList(direction, webGamesList);
                }
                catch (Exception)
                {
                    throw new ConnectionFailureException();
                }
            }

            Debug.Assert(gameList != null, "Game list is null.");

            return gameList;
        }


        /// <summary>
        /// Declares either an OnlineSync or OfflineSync object and calls SynchronizeGames of that object
        /// depending on the direction of the synchronization.
        /// </summary>
        /// <param name="syncActionList">List of games with their synchronization actions.</param>
        /// <returns>List of games with their synchronization actions updated with synchronization results.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public List<SyncAction> SynchronizeGames (List<SyncAction> syncActionList)
        {
            Debug.Assert(syncActionList != null, "syncActionList is null.");

            if (direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom)
            {
                OfflineSync offlineSync = new OfflineSync(direction, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                
                syncActionList = offlineSync.SynchronizeGames(syncActionList);
            }
            else if (direction == OnlineSync.ComToWeb || direction == OnlineSync.WebToCom)
            {
                OnlineSync onlineSync = new OnlineSync(direction, gameLibrary.GetGameList(OfflineSync.Uninitialize), user);

                try
                {
                    syncActionList = onlineSync.SynchronizeGames(syncActionList);
                }

                catch (Exception)
                {
                    onlineSync.Restore();
                    throw new ConnectionFailureException();
                }
            }

            return syncActionList;
        }


        /// <summary>
        /// Calls CheckBackupExists of OfflineSync to see if backup files exist for any game.
        /// </summary>
        /// <returns>True if backup exists, false if backup does not exist.</returns>
        public bool EndProgram()
        {
            OfflineSync offlinesync = new OfflineSync();
            
            foreach(Game game in gameLibrary.InstalledGameList)
            {
                if (offlinesync.CheckBackupExists(game.ConfigParentPath, OfflineSync.BackupConfigFolderName))
                {
                    return true;
                }

                if (offlinesync.CheckBackupExists(game.SaveParentPath, OfflineSync.BackupSavedGameFolderName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Restores available backup files for the games by declaring an OfflineSync object and calling
        /// its Restore method.
        /// </summary>
        /// <returns>List of SyncAction objects that includes results of restoration.</returns>
        public List<SyncAction> Restore()
        {
            OfflineSync offlineSync
                = new OfflineSync(OfflineSync.Uninitialize, gameLibrary.GetGameList(OfflineSync.Uninitialize));

            return offlineSync.Restore();

        }

        /// <summary>
        /// Removes backup files by calling RemoveAllBackup method of OfflineSync.
        /// </summary>
        /// <returns>List of errors when removing backup files.</returns>
        public List<SyncError> RemoveAllBackup()
        {
            OfflineSync offlinesync = new OfflineSync(OfflineSync.Uninitialize, gameLibrary.InstalledGameList);
            return offlinesync.RemoveAllBackup();
        }

        /// <summary>
        /// Calls SynchronizeGames method of WebAndThumbSync class.
        /// </summary>
        /// <param name="conflictsList">A list of conflicts.</param>
        /// <returns>List of errors that occurred during synchronization.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public List<SyncError> SynchronizeWebAndThumb(Dictionary<string, int> conflictsList)
        {
            try
            {
                List<SyncError> syncErrorList = wats.SynchronizeGames(conflictsList);
                return syncErrorList;
            }
            catch (Exception)
            {
                throw new ConnectionFailureException();
            }

        }

        /// <summary>
        /// Calls CheckConflicts of WebAndThumbSync class.
        /// </summary>
        /// <returns>A list of conflicts.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public Dictionary<string, int> CheckConflicts()
        {
            
            try
            {
                wats = new WebAndThumbSync(user);
                Dictionary<string, int> conflictsList = wats.CheckConflicts();
                return conflictsList;
            }
            catch (Exception)
            {
                throw new ConnectionFailureException();
            }
        }

        #endregion

        #region User Account Methods

        /// <summary>
        /// Calls Register method of User class.
        /// </summary>
        /// <param name="email">User's e-mail.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Code indicating if registration is successful or the errors that have occurred.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public int Register(string email, string password)
        {
            Debug.Assert(email != null && password != null, "Parameters cannot be null.");

            try
            {
                 return user.Register(email, password);
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }
        }
        
        /// <summary>
        /// Calls Login method of User class.
        /// </summary>
        /// <param name="email">User's e-mail.</param>
        /// <param name="password">User's password.</param>
        /// <returns>True if login successful, false otherwise.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public bool Login(string email, string password)
        {
            Debug.Assert(email != null && password != null, "Parameters cannot be null.");

            try
            {
                return user.Login(email, password);
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }

        }

        /// <summary>
        /// Calls Logout method of User class.
        /// </summary>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public void Logout()
        {
            try
            {
                user.Logout();
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }

        }

        /// <summary>
        /// Calls ResendActivation method of User class.
        /// </summary>
        /// <param name="email">User's e-mail.</param>
        /// <param name="inputPassword">User's password.</param>
        /// <returns>Code to indicate result.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public int ResendActivation(string email, string inputPassword)
        {
            Debug.Assert(email != null && inputPassword != null, "Parameters cannot be null.");

            try
            {
                return user.ResendActivation(email, inputPassword);
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }

        }

        /// <summary>
        /// Calls RetrievePassword method of User class.
        /// </summary>
        /// <param name="email">User's e-mail.</param>
        /// <returns>Code to indicate result.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public int RetrievePassword (string email)
        {
            Debug.Assert(email != null, "Email cannot be null.");

            try
            {
                return user.RetrievePassword(email);
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }
        }

        /// <summary>
        /// Calls ChangePassword of User class.
        /// </summary>
        /// <param name="email">User's e-mail.</param>
        /// <param name="oldPassword">User's previous password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>Code to indicate result.</returns>
        /// <exception cref="ConnectionFailureException">Unable to connect to web server.</exception>
        public int ChangePassword(string email, string oldPassword, string newPassword)
        {
            Debug.Assert(email != null && oldPassword != null && newPassword != null, "Parameters cannot be null.");

            try
            {
                return user.ChangePassword(email, oldPassword, newPassword);
            }

            catch (Exception)
            {
                throw new ConnectionFailureException();
            }
        }

        /// <summary>
        /// Calls IsLoggedIn method of User class.
        /// </summary>
        /// <returns>True if user is logged in, false otherwise.</returns>
        public bool IsLoggedIn()
        {
            return user.IsLoggedIn();             
        }

        
        #endregion

    }
}