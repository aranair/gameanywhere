using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace GameAnywhere
{
    public class GameLibrary
    {
        #region Properties and Constructor
        /// <summary>
        /// List of installed games on the computer, will always be updated if Propeties is called.
        /// </summary>
        private List<Game> installedGameList;
        
        /// <summary>
        /// Type of OS that the current system is running on.
        /// </summary>
        private string typeOS;

        #region static game names
        public static readonly string Warcraft3GameName = "Warcraft 3";
        public static readonly string FIFA10GameName = "FIFA 10";
        public static readonly string FM2010GameName = "Football Manager 2010";
        public static readonly string WOWGameName = "World of Warcraft";
        public static readonly string AbuseGameName = "Abuse";
        #endregion

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public GameLibrary()
        {
            // Create an empty list of games.
            installedGameList = new List<Game>();

            // Find the OS name and stores it into the data member.
            typeOS = GetOSName();
        }

        #endregion

        #region Resolving of common games
        /// <summary>
        /// Reinitialize each game to look for new files, and updates the list of installed games.
        /// </summary>
        public void RefreshList()
        {
            // Replaces current installed games list with a new List.
            installedGameList = new List<Game>();

            // Initializes each of the games again.
            Initialize();
        }

        /// <summary>
        /// Initializes each of the currently supported games.
        /// </summary>
        private void Initialize()
        {
            InitializeAbuse();
            InitializeWarcraft3();
            InitializeFIFA2010();
            InitializeWOW();
            InitializeFM2010();

            //InitializeCODModernWarfare();
            //InitializeDragonAge();       
            //InitializeL4D2();
        }

        /// <summary>
        /// Returns a game list according to the direction passed in.
        /// </summary>
        /// <param name="direction">Direction of sync</param>
        /// <returns>List of installed games that are compatible with the direction</returns>
        public List<Game> GetGameList(int direction) 
        {
            //Debug.Assert(direction == OfflineSync.ExternalToCom || direction == OfflineSync.ComToExternal || direction == OfflineSync.Uninitialize);
            RefreshList();

            if (direction == OfflineSync.ExternalToCom)
            {
                //NOTE: do not modify installedGameList
                List<Game> newList = new List<Game>();
                AddGamesSupportedByThumbdrive(ref newList);
                return newList;
            }
            else 
                return this.InstalledGameList;
        }

        /// <summary>
        /// Searches the corresponding folders of the external thumbdrive for each game in the master installedGameList
        /// to see if there are config or saved game files available for them, and edit the config files list, saved game files list. 
        /// 
        /// </summary>
        /// <param name="newList">List to add the games supported by external thumbdrive</param>
        private void AddGamesSupportedByThumbdrive(ref List<Game> newList)
        {
            
            //takes the current installedGameList. checks each game with thumbdrive,  returns all the games that is available.
            foreach (Game installedGame in installedGameList)
            {
                // Finds the external game path.
                string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + installedGame.Name;

                // Checks for existence of game files in the thumbdrive and adds it to list if it exists.
                if (Directory.Exists(externalGameFolderPath))
                    AddGameToExternalCommonList(installedGame, ref newList);

            }//end foreach
        }

        /// <summary>
        /// Adds a game into the new list of common games.
        /// </summary>
        /// <param name="installedGame">Game to be added.</param>
        /// <param name="newList">New list of common games to be editted.</param>
        private void AddGameToExternalCommonList(Game installedGame, ref List<Game> newList)
        {
            string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + installedGame.Name;

            // Game exists in external drive.
            Debug.Assert(Directory.Exists(externalGameFolderPath));

            // Create a new instance of Game, identical to the current wantedGame.(in loop)
            Game newGame = new Game(installedGame.ConfigPathList, installedGame.SavePathList, installedGame.Name, installedGame.InstallPath, installedGame.ConfigParentPath, installedGame.SaveParentPath);

            // Checks if config/saved game files are available on the thumbdrive and sets list accordingly.
            EditConfigAndSavedGameLists(ref newGame, OfflineSync.ExternalToCom);

            // add it to list to be returned;
            newList.Add(newGame);           
        }

        /// <summary>
        /// Edits the config and saved game lists of the new game, according to the direction.
        /// </summary>
        private void EditConfigAndSavedGameLists(ref Game newGame, int direction)
        {
            if (direction == OfflineSync.ExternalToCom)
            {
                string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + newGame.Name;
                //if any of savedgame/config folder does not exist, make the fields blank
                string configPath = externalGameFolderPath + @"\Config";
                string savedGamePath = externalGameFolderPath + @"\SavedGame";

                try
                {
                    if (newGame.Name.Equals(FIFA10GameName))
                    {
                        InitializeFIFAConfigList(ref newGame, configPath);
                        InitializeFIFASavedGameList(ref newGame, savedGamePath);
                    }
                    else if (newGame.Name.Equals(Warcraft3GameName))
                    {
                        InitializeWarcraft3ConfigList(ref newGame, configPath);
                        InitializeWarcraft3SavedGameList(ref newGame, savedGamePath);
                    }
                    else if (newGame.Name.Equals(WOWGameName))
                    {
                        InitializeWOWConfigList(ref newGame, configPath);
                        InitializeWOWSavedGameList(ref newGame, savedGamePath);
                    }
                    else if (newGame.Name.Equals(FM2010GameName))
                    {
                        InitializeFM2010SavedGameList(ref newGame, savedGamePath);
                    }
                    else if (newGame.Name.Equals(AbuseGameName))
                    {
                        InitializeAbuseSavedGameList(ref newGame, savedGamePath);
                    }
                }

                catch (Exception)
                {
                }
            }
        }
        #endregion

        #region Currently supported games

        #region FIFA2010
        /// <summary>
       /// Initializes FIFA 2010 into list of installed (if present)
       /// </summary>
        private void InitializeFIFA2010()
        {
            string key = RegistrySoftwarePath + @"\EA Sports\FIFA 10";
            string registryName = "Install Dir";

            try
            {
                // Tries to open the registry key to check for existence of game.
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
                {
                    // Game exists.
                    if (rk != null && rk.GetValue(registryName) != null)
                    {
                        // Assigning of each of the paths for the creation of a new Game instance.
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(registryName);
                        RemoveTrailingSlash(ref installPath);
                        
                        string configParentPath = DocumentsPath + @"\FIFA 10";
                        string saveParentPath = DocumentsPath + @"\FIFA 10";

                        Game newGame = new Game(configList, saveList, FIFA10GameName, installPath, configParentPath, saveParentPath);

                        // Initialize FIFA config list/saved game list
                        InitializeFIFAConfigList(ref newGame, configParentPath);
                        InitializeFIFASavedGameList(ref newGame, saveParentPath);

                        AddToInstalledGames(newGame);
                    }

                }
            }
            catch (Exception)
            {
                // Game folder is not accessible.
            }
        }

        

        
        /// <summary>
        /// // Initializes the config list for FIFA 10
        /// </summary>
        /// <param name="newGame">Game instance to be editted</param>
        /// <param name="configPath">Config parent path of the game</param>
        private void InitializeFIFAConfigList(ref Game newGame, string configPath)
        {
            newGame.ConfigPathList = new List<string>();
            if (Directory.Exists(configPath + @"\user"))
                newGame.ConfigPathList.Add(configPath + @"\user");
        }

        /// <summary>
        /// // Initializes the saved game list for FIFA 10
        /// </summary>
        /// <param name="newGame">Game instance to be editted</param>
        /// <param name="configPath">Saved game parent path of the game</param>
        private void InitializeFIFASavedGameList(ref Game newGame, string savedGamePath)
        {
            newGame.SavePathList = new List<string>();
            // Default saved game folder ( should be always there )
            if (Directory.Exists(savedGamePath + @"\A. Profiles"))
                newGame.SavePathList.Add(savedGamePath + @"\A. Profiles");

            // Finds each of the variable I. Be A Pro - * saved game files and adds it to the list.
            if (Directory.Exists(savedGamePath))
            {
                // Generic folders
                foreach (string folder in Directory.GetDirectories(savedGamePath, "I. Be A Pro - *"))
                {
                    newGame.SavePathList.Add(folder);
                }
            }
        }
        #endregion

        #region Warcraft 3
        /// <summary>
        /// Initializes Warcraft 3 into list of installed (if present)
        /// </summary>
        private void InitializeWarcraft3()
        {
            string registryName = "InstallPath";
            string key = @"SOFTWARE\Blizzard Entertainment\Warcraft III";

            try
            {
                // Attempts to open the registry key to check existence of game.
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(key))
                {
                    // If game exists ( key is not null and value of InstallPath is not null )
                    if (rk != null && rk.GetValue(registryName) != null)
                    {
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(registryName);
                        RemoveTrailingSlash(ref installPath);

                        string configParentPath = installPath;
                        string saveParentPath = installPath;
                        Game newGame = new Game(configList, saveList, Warcraft3GameName, installPath, configParentPath, saveParentPath);

                        // Initiaialize config files list and saved game files list for Warcraft 3.
                        InitializeWarcraft3ConfigList(ref newGame, configParentPath);
                        InitializeWarcraft3SavedGameList(ref newGame, saveParentPath);

                        AddToInstalledGames(newGame);
                    }
                }
            }
            catch (Exception)
            {
                // Game folders not accessible.
            }
        }

        /// <summary>
        /// // Initializes the saved game list for Warcraft 3
        /// </summary>
        /// <param name="newGame">Game instance to be editted</param>
        /// <param name="configPath">Saved game parent path of the game</param>
        private void InitializeWarcraft3SavedGameList(ref Game newGame, string savedGamePath)
        {
            newGame.SavePathList = new List<string>();
            if (Directory.Exists(Path.Combine(savedGamePath,"Save")))
                newGame.SavePathList.Add(Path.Combine(savedGamePath,"Save"));
        }

        /// <summary>
        /// // Initializes the config files list for Warcraft 3
        /// </summary>
        /// <param name="newGame">Game instance to be editted</param>
        /// <param name="configPath">Config parent path of the game</param>
        private void InitializeWarcraft3ConfigList(ref Game newGame, string configPath)
        {
            newGame.ConfigPathList = new List<string>();
            if (File.Exists(configPath + @"\CustomKeys.txt"))
               newGame.ConfigPathList.Add(configPath + @"\CustomKeys.txt");
        }
        #endregion

        #region FM2010

        /// <summary>
        /// Initializes Football Manager 2010 into list of installed (if present)
        /// </summary>
        private void InitializeFM2010()
        {

            string key = RegistrySoftwarePath + @"\Sports Interactive Ltd\Installs\FM2010"; ;
            const string registryName = "Path";

            try
            {
                // Attempts to open registry key to check existence of game.
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
                {
                    if (rk != null && rk.GetValue(registryName) != null)
                    {
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(registryName);
                        RemoveTrailingSlash(ref installPath);
                        string configParentPath = "";
                        string saveParentPath = DocumentsPath + @"\Sports Interactive\Football Manager 2010";

                        Game newGame = new Game(configList, saveList, FM2010GameName, installPath, configParentPath, saveParentPath);

                        InitializeFM2010SavedGameList(ref newGame, saveParentPath);

                        AddToInstalledGames(newGame);
                    }
                }
            }
            catch (Exception)
            {
                // Game folders not accessible.
            }
        }

        /// <summary>
        /// Initializes saved game list for Football Manager 2010.
        /// </summary>
        /// <param name="newGame">Game instance to be editted.</param>
        /// <param name="saveParentPath">Saved game parent path of the game.</param>
        private void InitializeFM2010SavedGameList(ref Game newGame, string saveParentPath)
        {
            // Default saved game folder
            if (Directory.Exists(saveParentPath + @"\games"))
                newGame.SavePathList.Add(saveParentPath + @"\games");
 
        }
        #endregion

        #region WOW

        /// <summary>
        /// Initializes World of Warcraft into list of installed (if present)
        /// </summary>
        private void InitializeWOW()
        {

            string registryName = "InstallPath";
            string key = RegistrySoftwarePath + @"\Blizzard Entertainment\World of Warcraft";

            try
            {
                // Attempts to open registry key to check existence of game.
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
                {
                    // Game exists
                    if (rk != null && rk.GetValue(registryName) != null)
                    {
                        // Assigning of specific game information for the creation of the new instance of Game.
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(registryName);
                        RemoveTrailingSlash(ref installPath);
                        string configParentPath = installPath + @"WTF";
                        string saveParentPath = installPath;
                        Game newGame = new Game(configList, saveList, WOWGameName, installPath, configParentPath, saveParentPath);

                        InitializeWOWConfigList(ref newGame, configParentPath);
                        InitializeWOWSavedGameList(ref newGame, saveParentPath);

                        AddToInstalledGames(newGame);
                    }
                }
            }
            catch (Exception)
            {
                // Game folders not accessible.
            }
        }

        /// <summary>
        /// Initializes saved game list for World of Warcraft
        /// </summary>
        /// <param name="newGame">Game instance to be editted.</param>
        /// <param name="saveParentPath">Saved game parent path of the game.</param>
        private void InitializeWOWSavedGameList(ref Game newGame, string saveParentPath)
        {
            newGame.SavePathList = new List<string>();
            if (Directory.Exists(saveParentPath + @"\Interface"))
                newGame.SavePathList.Add(saveParentPath + @"\Interface");
        }

        /// <summary>
        /// Initializes config files list for World of Warcraft
        /// </summary>
        /// <param name="newGame">Game instance to be editted.</param>
        /// <param name="saveParentPath">Config files parent path of the game.</param>
        private void InitializeWOWConfigList(ref Game newGame, string configParentPath)
        {
            newGame.ConfigPathList = new List<string>();
            if (Directory.Exists(configParentPath + @"\Account"))
                newGame.ConfigPathList.Add(configParentPath + @"\Account");
        }
        #endregion

        #region Abuse

        /// <summary>
        /// Initializes World of Warcraft into list of installed (if present)
        /// </summary>
        private void InitializeAbuse()
        {

            string registryName = "Install_Dir";
            string key = RegistrySoftwarePath + @"\Abuse";

            try
            {
                // Attempts to open registry key to check existence of game.
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
                {
                    // Game exists
                    if (rk != null && rk.GetValue(registryName) != null)
                    {
                        // Assigning of specific game information for the creation of the new instance of Game.
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(registryName);
                        RemoveTrailingSlash(ref installPath);

                        string configParentPath = "";
                        string saveParentPath = installPath;
                        Game newGame = new Game(configList, saveList, AbuseGameName, installPath, configParentPath, saveParentPath);

                        InitializeAbuseSavedGameList(ref newGame, saveParentPath);

                        AddToInstalledGames(newGame);
                    }
                }
            }
            catch (Exception)
            {
                // Game folders not accessible.
            }
        }

        /// <summary>
        /// Initializes saved game list for Abuse
        /// </summary>
        /// <param name="newGame">Game instance to be editted.</param>
        /// <param name="saveParentPath">Saved game parent path of the game.</param>
        private void InitializeAbuseSavedGameList(ref Game newGame, string saveParentPath)
        {
            newGame.SavePathList = new List<string>();

            if (Directory.Exists(saveParentPath))
            {
                // Generic folders
                foreach (string file in Directory.GetFiles(saveParentPath, "*.spe"))
                {
                    newGame.SavePathList.Add(file);
                }
            }
        }

        #endregion

        #endregion

        #region Future Games
        /// <summary>
        /// Initializes Call of Duty 2 Modern Warfare into list of installed (if present)
        /// </summary>
        /*private  void InitializeCODModernWarfare()
        {
            string key = "";
            string installPath = "";
            List<string> configList = new List<string>();
            List<string> tempConfigList = new List<string>();
            List<string> saveList  = new List<string>();
            const string gameName = "Call of Duty 2 Modern Warfare";
            const string regName = "Path";
            string playersPath = ProgramFilesX86 + @"\Steam\steamapps\common\Call of Duty Modern Warfare 2\players";
            tempConfigList.Add(playersPath + @"\config.cfg)");
            tempConfigList.Add(playersPath + @"\config_mp.cfg");
            tempConfigList.Add(playersPath + @"\settings_c.zip.iw4");
            tempConfigList.Add(playersPath + @"\settings_s.zip.iw4");
            tempConfigList.Add(playersPath + @"\settings_m.zip.iw4");

            foreach (string s in tempConfigList)
            {
                if (File.Exists(s))
                    configList.Add(s);
            }
            if (Directory.Exists(playersPath + @"\save"))
                saveList.Add(playersPath + @"\save");

            
            if (typeOS.Equals("Windows 7") || typeOS.Equals("Windows Vista"))
            {
                key = RegistrySoftwarePath + @"\Activision\Modern Warfare 2";
            }
            else if (typeOS.Equals("Windows XP"))
                key = "";

            //decide where to look.
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
            {
                if (rk != null && rk.GetValue(regName) != null)
                {
                    installPath = installPath + rk.GetValue(regName);
                    Game newGame = new Game(configList, saveList, gameName, installPath);
                    installedGameList.Add(newGame);
                }
            }
            
        }*/
        

        //Wilson
        /*private  void InitializeDragonAge()
        {

            string key = "";
            string installPath = "";
            const string configPath = "";
            const string savePath = "";
            const string gameName = "DragonAge";
            const string regName = "";

            if (typeOS.Equals("Windows 7") || typeOS.Equals("Windows Vista") )
            {
                if (Is64BitOS)
                    key = "";
                else
                    key = "";
            }
            else if (typeOS.Equals("Windows XP"))
                key = "";

            //decide where to look.

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
            {

                if (rk.GetValue(regName) != null)
                {
                    installPath = installPath + rk.GetValue(regName);
                    Game newGame = new Game(configPath, savePath, gameName, installPath);
                    installedGameList.Add(newGame);
                }
            }
        }*/
        
        
        //Hong Zhou
        /*private  void InitializeL4D2()
        {

            string key = "";
            string installPath = "";
            const string configPath = "";
            const string savePath = "";
            const string gameName = "Left 4 Dead 2";
            const string regName = "";

            if (typeOS.Equals("Windows 7") || typeOS.Equals("Windows Vista"))
            {
                if (Is64BitOS)
                    key = "";
                else
                    key = "";
            }
            else if (typeOS.Equals("Windows XP"))
                key = "";

            //decide where to look.

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
            {
                if (rk.GetValue(regName) != null)
                {
                    installPath = installPath + rk.GetValue(regName);
                    Game newGame = new Game(configPath, savePath, gameName, installPath);
                    installedGameList.Add(newGame);
                }
            }
        }*/
        #endregion

        #region Propeties and Helper functions

        /// <summary>
        /// Adds a game into the installedGameList;
        /// </summary>
        /// <param name="game"></param>
        private void AddToInstalledGames(Game game)
        {
            installedGameList.Add(game);
        }

        /// <summary>
        /// Returns full list of installed games on current machine.
        /// </summary>
        public List<Game> InstalledGameList
        {
            get { RefreshList(); return installedGameList; }
            set { installedGameList = value; }
        }

        /// <summary>
        /// Finds current OS name through a series of checks
        /// </summary>
        /// <returns>Name of OS (current machine)</returns>
        private  string GetOSName()
        {
            System.OperatingSystem os = System.Environment.OSVersion;
            string osName = "Unknown";


            switch (os.Platform)
            {
                case System.PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            osName = "Windows 95";
                            break;
                        case 10:
                            osName = "Windows 98";
                            break;
                        case 90:
                            osName = "Windows ME";
                            break;
                    }
                    break;
                case System.PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 3:
                            osName = "Windws NT 3.51";
                            break;
                        case 4:
                            osName = "Windows NT 4";
                            break;
                        case 5:
                            if (os.Version.Minor == 0)
                                osName = "Windows 2000";
                            else if (os.Version.Minor == 1)
                                osName = "Windows XP";
                            else if (os.Version.Minor == 2)
                                osName = "Windows Server 2003";
                            break;
                        case 6:
                            osName = "Windows Vista";
                            if (os.Version.Minor == 0)
                                osName = "Windows Vista";
                            else if (os.Version.Minor == 1)
                                osName = "Windows 7";

                            break;

                    }
                    break;
            }

            return osName;// + ", " + os.Version.ToString();
        }

        /// <summary>
        /// Returns a bool to whether current machine is running on a 64 bit OS
        /// </summary>
        public  bool Is64BitOS
        {
            get { return (Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null); }
        }

        /// <summary>
        /// Returns a path (string) to the program files directory according to the type of OS.
        /// </summary>
        private  string ProgramFilesX86
        {
            get
            {
                string programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                if (programFiles == null)
                {
                    programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                }

                return programFiles;
            }
        }

        /// <summary>
        /// Returns a path (string) to the registry software directory according to the type of OS.
        /// </summary>
        private  string RegistrySoftwarePath
        {
            get
            {
                string registrySoftwarePath = "";
                if (Is64BitOS)
                {
                    registrySoftwarePath = @"SOFTWARE\Wow6432Node";
                }
                else
                    registrySoftwarePath = @"SOFTWARE";

                return registrySoftwarePath;
            }
        }

        /// <summary>
        /// Returns a path(string) to the shell folder: Documents directory according to the OS.
        /// </summary>
        private  string DocumentsPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); }
        }

        /// <summary>
        /// If install path contains \ at the end, this method will trim the path and remove the ending \
        /// </summary>
        /// <param name="s">String to be checked and editted.</param>
        private void RemoveTrailingSlash(ref string s)
        {
            if (s.LastIndexOf("\\") == s.Length - 1)
                s = s.Remove(s.LastIndexOf("\\"));
        }
        #endregion
    }
}
