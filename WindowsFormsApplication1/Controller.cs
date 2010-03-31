using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace GameAnywhere
{
    public class Controller
    {
        #region Data Members

        /// <summary>
        /// GameLibrary object that stores all the game objects and their information.
        /// </summary>
        private GameLibrary gameLibrary;

        /// <summary>
        /// Stores the direction of the latest synchronization job.
        /// </summary>
        private int direction;

        /// <summary>
        /// To manage the user's online account.
        /// </summary>
        private User user;

        #endregion

        #region Constructors

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: Data members initialized and GUI started.
        /// 
        /// Description: Default constructor.
        /// 
        /// Exceptions: None.
        /// </summary>
        public Controller()
        {
            gameLibrary = new GameLibrary();
            direction = 0;
            user = new User();
            StartGUI();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: startPage GUI is running.
        /// 
        /// Description: Runs graphical user interface.
        /// 
        /// Exceptions: None.
        /// </summary>
        private void StartGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new startPage(this));
        }

        /// <summary>
        /// Pre-Conditions: direction is OfflineSync.ComToExternal or OfflineSync.ExternalToCom.
        /// Post-Conditions: direction is set.
        /// 
        /// Description: Sets synchronization direction.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="direction">synchronization direction</param>
        public void SetSyncDirection(int direction)
        {
            // Assert that direction is valid.
            Debug.Assert(direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom
                , "Direction is not valid.");

            // Set direction.
            this.direction = direction;
        }

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: Available backup files are restored and a SyncAction list is returned.
        /// 
        /// Description: Calls Restore() of OfflineSync class.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns>list of SyncAction objects that includes results of restoration</returns>
        public List<SyncAction> Restore()
        {
            OfflineSync offlineSync 
                = new OfflineSync(OfflineSync.Uninitialize, gameLibrary.GetGameList(OfflineSync.Uninitialize));
            
            return offlineSync.Restore();

        }
     

        /// <summary>
        /// Pre-Conditions: direction is OfflineSync.ExternalToCom or OfflineSync.ComToExternal.
        /// Post-Conditions: List of games is successfully returned to GUI class.
        /// 
        /// Description: Calls GetGameList of GameLibrary.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns>list of games available to be synchronized</returns>
        public List<Game> GetGameList()
        {
            // Assert that direction is valid.
            Debug.Assert(direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom
                , "Direction is not valid.");

            // Declare a list.
            List<Game> gameList;
     
            // Call GetGameList and set the list.
            gameList = gameLibrary.GetGameList(direction);

            // Assert that list is not null.
            Debug.Assert(gameList != null, "Game list is null.");

            // Return list to GUI class.
            return gameList;
        }


        /// <summary>
        /// Pre-Conditions: syncActionList is not null.
        /// Post-Conditions: Provides GUI class with updated List<SyncAction>
        /// 
        /// Description: Calls SynchronizeGames of OfflineSync.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <param name="syncActionList">list of games with their synchronization actions</param>
        /// <returns>list of games with their synchronization actions updated with results</returns>
        public List<SyncAction> SynchronizeGames (List<SyncAction> syncActionList)
        {
            // Assert that syncActionList is not null.
            Debug.Assert(syncActionList != null, "syncActionList is null.");

            // Offline synchronization.
            if (direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom)
            {
                // Initialize an OfflineSync object.
                OfflineSync offlineSync = new OfflineSync(direction, gameLibrary.GetGameList(OfflineSync.Uninitialize));

                // Call SynchronizeGames of OfflineSync class to process.
                syncActionList = offlineSync.SynchronizeGames(syncActionList);
            }

            return syncActionList;
        }


        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: Returns true if backup exists, false if backup does not exist.
        /// 
        /// Description: Informs caller if backup exists or not when the program is ended. Calls
        ///              CheckBackupExists of OfflineSync.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns>true if backup exists, false if backup does not exist</returns>
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
        /// Pre-Conditions: None.
        /// Post-Conditions: A non-null SyncError list is returned.
        /// 
        /// Description: Calls RemoveAllBackup of OfflineSync.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns></returns>
        public List<SyncError> RemoveAllBackup()
        {
            OfflineSync offlinesync = new OfflineSync(OfflineSync.Uninitialize, gameLibrary.InstalledGameList);
            return offlinesync.RemoveAllBackup();
        }
  
        
        /// <summary>
        /// Pre-Conditions: email and password are not null.
        /// Post-Conditions: Provides confirmation if user has been registered successfully.
        /// 
        /// Description: Calls Register of User class to process.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="email">e-mail</param>
        /// <param name="password">password</param>
        /// <returns>a code indicating if registration is successful or not</returns>
        public int Register(string email, string password)
        {
            Debug.Assert(email != null && password != null, "Parameters cannot be null.");

            int code = 0;

            try
            {
                 code = user.Register(email, password);
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

            return code;
        }
        
        /// <summary>
        /// Pre-Conditions: email and password are not null.
        /// Post-Conditions: Provides confirmation if user is logged in.
        /// 
        /// Description: Calls Login of User class to process.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="password">password</param>
        /// <returns>true if login successful, false if not</returns>
        public bool Login(string email, string password)
        {
            Debug.Assert(email != null && password != null, "Parameters cannot be null.");

            try
            {
                return user.Login(email, password);
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: User is logged out.
        /// 
        /// Description: Calls Logout of User class.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        public void Logout()
        {
            try
            {
                user.Logout();
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: Activation email is resent.
        /// 
        /// Description: Calls ResendActivation of User class.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="inputPassword">password</param>
        /// <returns>code to indicate results</returns>
        public int ResendActivation(string email, string inputPassword)
        {
            Debug.Assert(email != null && inputPassword != null, "Parameters cannot be null.");

            try
            {
                return user.ResendActivation(email, inputPassword);
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Pre-Conditions: email is not null.
        /// Post-Conditions: Password is retrieved.
        /// 
        /// Description: Calls RetrievePassword of User class.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>code to indicate results</returns>
        public int RetrievePassword (string email)
        {
            Debug.Assert(email != null, "Email cannot be null.");

            try
            {
                return user.RetrievePassword(email);
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Pre-Conditions: email, oldPassword, newPassword are not null.
        /// Post-Conditions: Password is changed.
        /// 
        /// Description: Calls ChangePassword of User class.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="oldPassword">previous password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>code to indicate results</returns>
        public int ChangePassword(string email, string oldPassword, string newPassword)
        {
            Debug.Assert(email != null && oldPassword != null && newPassword != null, "Parameters cannot be null.");

            try
            {
                return user.ChangePassword(email, oldPassword, newPassword);
            }

            catch (ConnectionFailureException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Pre-Conditions: None.
        /// Post-Conditions: direction is returned.
        /// 
        /// Description: Returns synchronization direction.
        /// 
        /// Exceptions: None.
        /// </summary>
        /// <returns>direction</returns>
        public int GetDirection()
        {
            return direction;
        }

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

        #endregion

        #region v2.0
        /*
        /// <summary>
        /// Pre-Condition: syncActionList is not null.
        /// Post-Condition: Provides GUI class with the same syncActionList with the results updated.
        /// 
        /// Calls SynchronizeGames of OfflineSync or OnlineSync class to do the processing.
        /// 
        /// Exceptions: ConnectionFailureException - unable to connect to web server.
        /// </summary>
        /// <param name="syncActionList">list of games with their synchronization actions</param>
        /// <returns>list of games with their synchronization actions updated with results</returns>
        public List<SyncAction> SynchronizeGames(List<SyncAction> syncActionList)
        {
            // Assert that syncActionList is not null.
            Debug.Assert(syncActionList != null, "syncActionList is null.");

            // Offline synchronization.
            if (direction == OfflineSync.ComToExternal || direction == OfflineSync.ExternalToCom)
            {

                // Initialize an OfflineSync object.
                OfflineSync offlineSync = new OfflineSync(direction);

                // Call SynchronizeGames of OfflineSync class to process.

                syncActionList = offlineSync.SynchronizeGames(syncActionList);

                // Original files have been stored as backup.
                if (direction == OfflineSync.ExternalToCom)
                    backupExists = true;
            }

            // Online synchronization.
            else if (direction == OnlineSync.COM_TO_WEB || direction == OnlineSync.WEB_TO_COM
                                     || direction == OnlineSync.WEB_AND_EXTERNAL)
            {
                // Initialize an OfflineSync object.
                OnlineSync onlineSync = new OnlineSync(direction);

                try
                {
                    // Call SynchronizeGames of OnlineSync class to process.
                    syncActionList = onlineSync.SynchronizeGames(syncActionList);
                }

                catch (ConnectionFailureException e)
                {
                    throw e;
                }
            }

            return syncActionList;

        }
        */
        #endregion

    }
}
