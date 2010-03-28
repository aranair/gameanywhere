﻿using System;
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
using System.Text.RegularExpressions;

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

        /// <summary>
        /// Controller class to communicate with other layers.
        /// </summary>
        private Controller controller;

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

        /// <summary>
        /// Overloaded Constructor.
        /// </summary>
        public GameLibrary(Controller controller)
        {
            this.controller = controller;
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
            /*
            InitializeAbuse();
            InitializeWarcraft3();
            InitializeFIFA2010();
            InitializeWOW();
            InitializeFM2010();*/
            
            InitGamesFromFile("gamev3.txt");

            //InitializeCODModernWarfare();
            //InitializeDragonAge();       
            //InitializeL4D2();
        }

        /// <summary>
        /// Returns a game list according to the direction passed in.
        /// This is the default GetGameList function that handles only OFFLINE synchronizations.
        /// Pre-condition: Direction passed in must be valid: either ExternalToCom or ComToExternal
        /// Post-condition: A new list of installed games that are compatible with the direction will be returned.
        /// </summary>
        /// <param name="direction">Direction of sync</param>
        /// <returns>List of installed games that are compatible with the direction</returns>
        public List<Game> GetGameList(int direction) 
        {
            Debug.Assert(direction == OfflineSync.ExternalToCom || direction == OfflineSync.ComToExternal 
                || direction == OfflineSync.Uninitialize || direction == OnlineSync.ComToWeb);
            RefreshList();

            if (direction == OfflineSync.ExternalToCom)
            {
                List<Game> newList = new List<Game>();
                //NOTE: do not modify installedGameList        
                AddGamesSupportedByThumbdrive(ref newList);
                return newList;
            }
            else // ComToExternal, ComToWeb,
                return this.InstalledGameList;
        }
        /// <summary>
        /// Returns a game list according to the direction passed in.
        /// This is the overloaded GetGameList function that handles only WEB TO COM direction synchronizations.
        /// Pre-condition: Direction passed in must be valid: either WebToCom, ComToWeb or ExternalAndWeb.
        /// Post-condition: A new list of installed games that are compatible with the direction will be returned.
        /// </summary>
        /// <param name="direction">Direction of sync.</param>
        /// <param name="webGamesList">List of web games.</param>
        /// <returns>List of installed games that are compatible with the direction</returns>
        public List<Game> GetGameList(int direction, List<string> webGamesList)
        {
            Debug.Assert(direction == OnlineSync.WebToCom);
            RefreshList();
            List<Game> newList = new List<Game>();

            AddGamesSupportedByWeb(ref newList, webGamesList);

            return newList;
            
        }
  
        /// <summary>
        /// Gets the corresponding games/files from the online database for each game in the master installedGameList
        /// to see if there are config or saved game files available for them.
        /// </summary>
        /// <param name="newList">List to add the games supported by web</param>
        private void AddGamesSupportedByWeb(ref List<Game> newList, List<string> webGamesList)
        {     
            //takes the current installedGameList. checks each game with web's and return all the games that is available.
            foreach (Game installedGame in installedGameList)
            {
                MatchWebGameAndFiles(installedGame, webGamesList, ref newList);
            }//end foreach
        }

        /// <summary>
        /// Finds a match for an installed game, with the list of games on the web passed in as strings 
        /// and creates a new game to be added into the list of common games, and sets the config list/ saved game list
        /// of the new game according to the web's availability.
        /// </summary>
        /// <param name="installedGame">Game to be searched for.</param>
        /// <param name="webGameList">List of games (strings) on the web.</param>
        /// <param name="newList">List for the game to be added in.</param>
        private void MatchWebGameAndFiles(Game installedGame, List<string> webGameList, ref List<Game> newList)
        {
            bool gameExists = false;

            List<string> newConfigPathList = new List<string>();
            List<string> newSavePathList = new List<string>();

            // Finds the games and file types that are available on the web.
            foreach (string line in webGameList)
            {
                // line = e.g "Warcraft 3/Config"
                string gameName = line.Remove(line.IndexOf('/'));
                string gameFileType = line.Substring(line.IndexOf("/") + 1);
                
                // Game name matches.
                if (gameName.Equals(installedGame.Name))
                {
                    gameExists = true;
                    if (gameFileType.Equals("config"))
                        newConfigPathList.Add("Available");

                    if (gameFileType.Equals("savedGame"))
                        newSavePathList.Add("Available");
                }
            }

            if (gameExists)
            {
                Game newGame = new Game(newConfigPathList, newSavePathList, installedGame.Name, installedGame.InstallPath, installedGame.ConfigParentPath, installedGame.SaveParentPath);
                newList.Add(newGame);
            }

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
            EditGenericConfigAndSavedGameLists(ref newGame);

            // add it to list to be returned;
            newList.Add(newGame);           
        }

        /// <summary>
        /// Edits the ConfigList and SaveGameList of the game passed in.(generic)
        /// </summary>
        /// <param name="newGame">The game to be editted.</param>
        public void EditGenericConfigAndSavedGameLists(ref Game newGame)
        {

            List<string> newConfigPathList = new List<string>();
            foreach (string s in newGame.ConfigPathList)
            {
                string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + newGame.Name;
                //if any of savedgame/config folder does not exist, make the fields blank
                string configPath = externalGameFolderPath + @"\Config";

                string n = s.Replace(newGame.ConfigParentPath, configPath);
                newConfigPathList.Add(n);
            }
            newGame.ConfigPathList = newConfigPathList;

            List<string> newSavePathList = new List<string>();
            foreach (string s in newGame.SavePathList)
            {
                string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + newGame.Name;
                //if any of savedgame/config folder does not exist, make the fields blank
                string savedGamePath = externalGameFolderPath + @"\SavedGame";

                string n = s.Replace(newGame.SaveParentPath, savedGamePath);
                newSavePathList.Add(n);
            }
            newGame.SavePathList = newSavePathList;
        }

        /// <summary>
        /// Edits the config and saved game lists of the new game for the direction: External To Computer.
        /// </summary>
        private void EditConfigAndSavedGameLists(ref Game newGame)
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
                        string configParentPath = installPath + @"\WTF";
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

                        //InitializeAbuseConfigList(ref newGame, configParentPath);
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

        /// <summary>
        /// Initializes config files list for Abuse
        /// </summary>
        /// <param name="newGame">Game instance to be editted.</param>
        /// <param name="saveParentPath">Config files parent path of the game.</param>
        private void InitializeAbuseConfigList(ref Game newGame, string configParentPath)
        {
            newGame.ConfigPathList = new List<string>();
            if (Directory.Exists(configParentPath + @"\Account"))
                newGame.ConfigPathList.Add(configParentPath + @"\Account");
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

        #region Text File Initialization
        /// <summary>
        /// Initializes a single game from one set of dictionary text file entries.
        /// </summary>
        /// <param name="variableListPassed">The set of dictionary values for one game entry.</param>
        private void InitializeGameFromTextFile(Dictionary<string, string> variableListPassed)
        {
            string gameName = "";
            string regKey = "";
            string regValue = "";
            string saveParentPath = "";
            string configParentPath = "";

            bool currentUser = false;
            bool localMachine = false;

            Dictionary<string, string> variableList = new Dictionary<string, string>();
            foreach (string key in variableListPassed.Keys)
            {
                string s = ReplaceStrings(variableListPassed[key]);
                variableList.Add(key, s);

            }
            gameName = variableList["Game"];
            regKey = variableList["RegKey"];
            regValue = variableList["RegValue"];

            if (variableList["RegType"].Equals("HKCU"))
                currentUser = true;
            else if (variableList["RegType"].Equals("HKLM"))
                localMachine = true;

            try
            {
                // Attempts to open the registry key to check existence of game.
                RegistryKey rk;
                if (currentUser)
                    rk = Registry.CurrentUser.OpenSubKey(regKey);
                else if (localMachine)
                    rk = Registry.LocalMachine.OpenSubKey(regKey);
                else
                    return;

                using (rk)
                {
                    // If game exists ( key is not null and value of InstallPath is not null )
                    if (rk != null && rk.GetValue(regValue) != null)
                    {
                        List<string> configList = new List<string>();
                        List<string> saveList = new List<string>();
                        string installPath = "" + rk.GetValue(regValue);
                        RemoveTrailingSlash(ref installPath);


                        Dictionary<string, string> variableListFinal = new Dictionary<string, string>();

                        foreach (string key in variableList.Keys)
                        {
                            string s = ReplaceInstallPath(variableList[key], installPath);
                            variableListFinal.Add(key, s);
                        }

                        if (variableListFinal.ContainsKey("ConfigParentPath"))
                            configParentPath = variableListFinal["ConfigParentPath"];

                        if (variableListFinal.ContainsKey("SaveParentPath"))
                            saveParentPath = variableListFinal["SaveParentPath"];

                        if (variableListFinal.ContainsKey("SavePathList"))
                            AddSaveFiles(ref saveList, variableListFinal);

                        if (variableListFinal.ContainsKey("ConfigPathList"))
                            AddConfigFiles(ref configList, variableListFinal);

                        if (variableListFinal.ContainsKey("SearchSaveParent"))
                            AddVariableSaveFiles(ref saveList, variableListFinal);

                        if (variableListFinal.ContainsKey("SearchConfigParent"))
                            AddVariableConfigFiles(ref configList, variableListFinal);


                        Game newGame = new Game(configList, saveList, gameName, installPath, configParentPath, saveParentPath);

                        installedGameList.Add(newGame);
                    }
                }
            }// end try
            catch (Exception)
            {
                // Game folders not accessible.
            }

        }

        /// <summary>
        /// Adds saved games files from a regex key given in value of key "SearchSaveParent" in the given dictionary.
        /// </summary>
        /// <param name="saveList">The List of saved games file to add the files to.</param>
        /// <param name="variableListFinal">The dictionary list of all data from text file.</param>
        private static void AddVariableSaveFiles(ref List<string> saveList, Dictionary<string, string> variableListFinal)
        {
            List<string> listOfVariableSaveList = SeperatePathsByDelimiter(variableListFinal["SearchSaveParent"]);

            foreach (string regex in listOfVariableSaveList)
            {
                if (Directory.Exists(variableListFinal["SaveParentPath"]))
                    AddFoldersAndFiles(ref saveList, variableListFinal["SaveParentPath"], regex);
            }
        }

        /// <summary>
        /// Searches a given regex, at the path provided, and add all files and folders that matches the regex to the list.
        /// </summary>
        /// <param name="list">The List for files and folders to be added to.</param>
        /// <param name="path">Path to search regex at.</param>
        /// <param name="regex">Regex string to search.</param>
        private static void AddFoldersAndFiles(ref List<string> list, string path, string regex)
        {
            foreach (string folder in Directory.GetDirectories(path, regex))
            {
                list.Add(folder);
            }
            foreach (string file in Directory.GetFiles(path, regex))
            {
                list.Add(file);
            }
        }

        /// <summary>
        /// Adds config files from a regex key given in value of key "SearchConfigParent" in the given dictionary.
        /// </summary>
        /// <param name="saveList">The List of config files to add the files  to.</param>
        /// <param name="variableListFinal">The dictionary list of all data from text file.</param>
        private static void AddVariableConfigFiles(ref List<string> configList, Dictionary<string, string> variableListFinal)
        {
            List<string> listOfVariableConfigList = SeperatePathsByDelimiter(variableListFinal["SearchConfigParent"]);

            foreach (string regex in listOfVariableConfigList)
            {
                if (Directory.Exists(variableListFinal["ConfigParentPath"]))
                    AddFoldersAndFiles(ref configList, variableListFinal["ConfigParentPath"], regex);
            }
        }

        /// <summary>
        /// Adds Saved game files from the dictionary list, reading from the SavePathList key.
        /// </summary>
        /// <param name="saveList">The List of saved game files to add the files to.</param>
        /// <param name="variableListFinal">The dictionary list of all data from text file.</param>
        private static void AddSaveFiles(ref List<string> saveList, Dictionary<string, string> variableListFinal)
        {
            List<string> delimitedConfigPaths = SeperatePathsByDelimiter(variableListFinal["SavePathList"]);
            foreach (string path in delimitedConfigPaths)
            {
                saveList.Add(path);
            }
        }

        /// <summary>
        /// Adds config files from the dictionary list, reading from the ConfigPathList key.
        /// </summary>
        /// <param name="configList">The List of config files to add the files to.</param>
        /// <param name="variableListFinal">The dictionary list of all data from text file.</param>
        private static void AddConfigFiles(ref List<string> configList, Dictionary<string, string> variableListFinal)
        {
            List<string> delimitedConfigPaths = SeperatePathsByDelimiter(variableListFinal["ConfigPathList"]);
            foreach (string path in delimitedConfigPaths)
            {
                configList.Add(path);
            }
        }

        /// <summary>
        /// Seperates a line into delimited paths to a character, and returns a list of strings.
        /// </summary>
        /// <param name="longPath">The string to delimit</param>
        /// <returns>List of strings delimited by ,</returns>
        private static List<string> SeperatePathsByDelimiter(string longPath)
        {
            List<string> delimitedPaths = new List<string>();

            while (longPath.IndexOf(",") != -1)
            {
                delimitedPaths.Add(longPath.Remove(longPath.IndexOf(",")));
                longPath = longPath.Substring(longPath.IndexOf(",") + 1);
            }
            delimitedPaths.Add(longPath);

            return delimitedPaths;
        }

        /// <summary>
        /// Replaces a list of strings, with a variable value, in the string passed in.
        /// </summary>
        /// <param name="s">String to edit.</param>
        /// <returns>Editted string</returns>
        public string ReplaceStrings(string s)
        {
            s = Regex.Replace(s, "DocumentsPath", DocumentsPath);
            s = Regex.Replace(s, "RegistrySoftwarePath", RegistrySoftwarePath);
            return s;
        }

        /// <summary>
        /// Replaces InstallPath variable in the given string with the actual install path passed in.
        /// </summary>
        /// <param name="s">String to edit.</param>
        /// <returns>Editted string</returns>
        public string ReplaceInstallPath(string s, string installPath)
        {
            s = Regex.Replace(s, "InstallPath", installPath);
            return s;
        }

        /// <summary>
        /// File parser for the game init text file.
        /// </summary>
        /// <param name="filename">File path/name to be parsed.</param>
        public void InitGamesFromFile(string filename)
        {
            System.IO.Stream fileStream = this.GetType().Assembly.GetManifestResourceStream("GameAnywhere.gamev3.txt");
            StreamReader r = new StreamReader(fileStream);
            
            Dictionary<string, string> game = new Dictionary<string, string>();
            string line;
            while (r.Peek() >= 0)
            {
                line = r.ReadLine();
                // # designated for comments
                if (line.IndexOf("#") == 0)
                    continue;

                // [ENDGAME] in file determines end of this game's information.
                if (line.Equals("[ENDGAME]"))
                {
                    InitializeGameFromTextFile(game);
                    game.Clear();
                    continue;
                }
                if (line.Equals(""))
                    continue;


                string[] kv = line.Split('=');
                //this trims white spaces from the entries in the text file.
                game[kv[0].Trim()] = kv[1].Trim();
            }

        }

        
        #endregion
    }
}
