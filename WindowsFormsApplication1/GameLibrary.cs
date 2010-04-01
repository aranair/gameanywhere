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
using System.Text.RegularExpressions;


namespace GameAnywhere
{
    /// <summary>
    /// This class handles all operations regarding the Game class. 
    /// Primarily, it initializes entries from the text files and 
    /// loads the crucial information about each game into game objects.
    /// </summary>
    public class GameLibrary
    {
        #region Properties and Constructor
        /// <summary>
        /// List of installed games on the computer, will always be updated if Propeties is called.
        /// </summary>
        private List<Game> installedGameList;

        /// <summary>
        /// List of all dictionary entries from textfile.
        /// </summary>
        private List<Dictionary<string, string>> entriesFromTextFile;
        
        /// <summary>
        /// Type of OS that the current system is running on.
        /// </summary>
        private string typeOS;

        /// <summary>
        /// Controller class to communicate with other layers.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// List of all currently supposed game names for use with GUI and other classes.
        /// </summary>
        #region static game names
        ///<value>Warcraft 3 game name.</value>
        public static readonly string Warcraft3GameName = "Warcraft 3";
        ///<value>FIFA10 game name.</value>
        public static readonly string FIFA10GameName = "FIFA 10";
        ///<value>Football Manager 2010 game name.</value>
        public static readonly string FM2010GameName = "Football Manager 2010";
        ///<value>World of Warcraft game name.</value>
        public static readonly string WOWGameName = "World of Warcraft";
        ///<value>Abuse game name.</value>
        public static readonly string AbuseGameName = "Abuse";
        #endregion

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public GameLibrary()
        {
            // Create an empty list of games.
            installedGameList = new List<Game>();
            entriesFromTextFile = new List<Dictionary<string, string>>();

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
            entriesFromTextFile = new List<Dictionary<string, string>>();

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
            entriesFromTextFile = new List<Dictionary<string, string>>();

            // Initializes each of the games again.
            Initialize();
        }

        /// <summary>
        /// Initializes each of the currently supported games.
        /// </summary>
        private void Initialize()
        {
            System.IO.Stream fileStream = this.GetType().Assembly.GetManifestResourceStream("GameAnywhere.gamev3.txt");
            StreamReader r = new StreamReader(fileStream);
            ParseGamesFromFile(r);
            string userTextFilePath = Directory.GetCurrentDirectory() + @"\userGames.txt";
            if (Directory.Exists(userTextFilePath))
            {
                r = new StreamReader("userGames.txt");
                ParseGamesFromFile(r);
            }
            
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

            // Checks if config/saved game files are available on the thumbdrive and sets list accordingly.
            Game newGame = EditGenericConfigAndSavedGameLists(installedGame);

            // add it to list to be returned;
            newList.Add(newGame);           
        }

        /// <summary>
        /// Edits the ConfigList and SaveGameList of the game passed in.(generic)
        /// </summary>
        /// <param name="newGame">The game to be editted.</param>
        public Game EditGenericConfigAndSavedGameLists(Game installedGame)
        {
            Game newGame = new Game();
            string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + installedGame.Name;
            string configParentPath = externalGameFolderPath + @"\config";
            string saveParentPath = externalGameFolderPath + @"\savedGame";

            foreach (Dictionary<string, string> entry in entriesFromTextFile)
            {
                if (entry.ContainsKey("Game") && entry["Game"].Equals(installedGame.Name))
                { 
                    newGame = InitializeGameFromTextFile(entry, saveParentPath, configParentPath, OfflineSync.ExternalToCom);
                }
            }

            return newGame;

        }

        #endregion

        #region Propeties and Helper functions

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
        /// This function will create and edit a new game in 2 ways according to the direction.
        /// 
        /// 1: (External to Computer) It will return the editted new game. This game object will
        ///    contain the game's generic information, saved game list/config files list of the EXTERNAL drive,
        ///    and the COMPUTER's saveParentPath,configParentPath.
        ///    
        /// 2: (All other directions) It will add editted new game into the installed game list. 
        ///    This game object will contain all the game's information from the COMPUTER.
        ///    
        /// 
        /// </summary>
        /// <param name="variableListPassed">The dictionary entry for one single game.</param>
        /// <param name="argSaveParentPath">The saveParentPath used for scenario 1)</param>
        /// <param name="argConfigParentPath">The configParentPath used for scenario 1)</param>
        /// <param name="direction">The direction of synchronization when using this function.</param>
        /// <returns>Newly editted game that was initialized from the dictionary entry.</returns>
        private Game InitializeGameFromTextFile(Dictionary<string, string> variableListPassed, string argSaveParentPath, string argConfigParentPath, int direction)
        {
            string gameName = "";
            string regKey = "";
            string regValue = "";
            string saveParentPath = argSaveParentPath;
            string configParentPath = argConfigParentPath;

            Dictionary<string, string> variableList = new Dictionary<string, string>();

            foreach (string key in variableListPassed.Keys)
            {
                string s = ReplaceStrings(variableListPassed[key]);
                variableList.Add(key, s);

            }
            gameName = variableList["Game"];
            regKey = variableList["RegKey"];
            regValue = variableList["RegValue"];

            if (direction == OfflineSync.ExternalToCom)            
                ChangeFixedListsToExternalPaths(ref variableList, gameName);

            try
            {
                // Attempts to open the registry key to check existence of game.
                RegistryKey rk;
                if (variableList["RegType"].Equals("HKCU"))
                    rk = Registry.CurrentUser.OpenSubKey(regKey);
                else if (variableList["RegType"].Equals("HKLM"))
                    rk = Registry.LocalMachine.OpenSubKey(regKey);
                else
                    return new Game();

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

                        // This portion replaces all "InstallPath" variable with the correct install path of the game.
                        foreach (string key in variableList.Keys)
                        {
                            string s = ReplaceInstallPath(variableList[key], installPath);
                            variableListFinal.Add(key, s);
                        }
         
                        if (variableListFinal.ContainsKey("ConfigParentPath"))
                        {
                            string originalConfigParentPath = variableListFinal["ConfigParentPath"];

                            if (direction != OfflineSync.ExternalToCom)
                                configParentPath = variableListFinal["ConfigParentPath"];
                            else
                                variableListFinal["ConfigParentPath"] = configParentPath;
                            
                            FindAllConfigFiles(ref configList, variableListFinal);

                            configParentPath = originalConfigParentPath;
                        }

                        if (variableListFinal.ContainsKey("SaveParentPath"))
                        {
                            string originalSaveParentPath = variableListFinal["SaveParentPath"];

                            if (direction != OfflineSync.ExternalToCom)
                                saveParentPath = variableListFinal["SaveParentPath"];
                            else
                                variableListFinal["SaveParentPath"] = saveParentPath;
                            
                            FindAllSavedGameFiles(ref saveList, variableListFinal);

                            saveParentPath = originalSaveParentPath;        
                        }

                        Game newGame = new Game(configList, saveList, gameName, installPath, configParentPath, saveParentPath);

                        if (direction != OfflineSync.ExternalToCom)
                            installedGameList.Add(newGame);
                        
                        return newGame;
                        
                    }
                    return null;

                } 
            }// end try
            catch (Exception)
            {
                // Game folders not accessible.
            }

            return null;
        }

        private void ChangeFixedListsToExternalPaths(ref Dictionary<string, string> variableList, string gameName)
        {
            // Replace all savePathList and configPathList into the source's paths (external)
            string externalGameFolderPath = Directory.GetCurrentDirectory() + @"\SyncFolder\" + gameName;
            string externalSavePath = externalGameFolderPath + @"\savedGame";
            string externalConfigPath = externalGameFolderPath + @"\config";

            if (variableList.ContainsKey("SavePathList"))
                variableList["SavePathList"] = variableList["SavePathList"].Replace(variableList["SaveParentPath"], externalSavePath);

            if (variableList.ContainsKey("ConfigPathList"))
                variableList["ConfigPathList"] = variableList["ConfigPathList"].Replace(variableList["ConfigParentPath"], externalConfigPath);

        }

        
        /// <summary>
        /// Adds both variable saved game files and the fixed ones given in "SavePathList" variable.
        /// </summary>
        /// <param name="saveList">The saved game list to add the files to.</param>
        /// <param name="variableListFinal">The final variable list, of which certain variables have been replaced with actual paths on the PC.</param>
        private static void FindAllSavedGameFiles(ref List<string> saveList, Dictionary<string, string> variableListFinal)
        {
            if (variableListFinal.ContainsKey("SavePathList"))
                AddSaveFiles(ref saveList, variableListFinal);

            if (variableListFinal.ContainsKey("SearchSaveParent"))
                AddVariableSaveFiles(ref saveList, variableListFinal);
        }

        /// <summary>
        /// Adds both variable config files and the fixed ones given in "ConfigPathList" variable.
        /// </summary>
        /// <param name="configList">The config file list to add the files to.</param>
        /// <param name="variableListFinal">The final variable list, of which certain variables have been replaced with actual paths on the PC.</param>
        private static void FindAllConfigFiles(ref List<string> configList, Dictionary<string, string> variableListFinal)
        {
            if (variableListFinal.ContainsKey("ConfigPathList"))
                AddConfigFiles(ref configList, variableListFinal);

            if (variableListFinal.ContainsKey("SearchConfigParent"))
                AddVariableConfigFiles(ref configList, variableListFinal);
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
                // Add files (function checks if it exists)
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
               if (Directory.Exists(path)) 
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
                if(Directory.Exists(path))
                    configList.Add(path);
            }
        }

        /// <summary>
        /// Seperates a line into delimited paths to a character, and returns a list of strings.
        /// </summary>
        /// <param name="longPath">The string to delimit.</param>
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
            s = Regex.Replace(s, "$InstallPath$", installPath);
            return s;
        }

        /// <summary>
        /// File parser for the game init text file.
        /// </summary>
        /// <param name="filename">File path/name to be parsed.</param>
        public void ParseGamesFromFile(StreamReader streamReader)
        {
            Dictionary<string, string> game = new Dictionary<string, string>();
            // One single line in the text file
            string line;

            //entriesFromTextFile = new List<Dictionary<string, string>>();
            while (streamReader.Peek() >= 0)
            {
                line = streamReader.ReadLine();
                // # designated for comments
                if (line.IndexOf("#") == 0)
                    continue;

                // [ENDGAME] in file determines end of this game's information.
                if (line.Equals("[ENDGAME]"))
                {
                    Dictionary<string, string> game2 = new Dictionary<string, string>(game);

                    if (HasDuplicate(game2))
                        continue;
                    else
                    {
                        entriesFromTextFile.Add(game2);
                        InitializeGameFromTextFile(game, "", "", OfflineSync.Uninitialize);
                        game.Clear();
                        continue;
                    }
                }
                
                // Empty line
                if (line.Equals(""))
                    continue;

                string[] kv = line.Split('=');
                //this trims white spaces from the entries in the text file.
                game[kv[0].Trim()] = kv[1].Trim();
            }

        }

        /// <summary>
        /// This function checks for duplicates
        /// </summary>
        /// <param name="game2"></param>
        /// <returns></returns>
        private bool HasDuplicate(Dictionary<string, string> gameEntry)
        {
            bool hasDuplicate = false;
            if (gameEntry.ContainsKey("Game"))
            {
                foreach (Dictionary<string, string> entry in entriesFromTextFile)
                {
                    if (entry.ContainsKey("Game") && entry["Game"].Equals(gameEntry["Game"]))
                        hasDuplicate = true;
                }
            }
            else
                hasDuplicate = true;

            return hasDuplicate;

        }

        
        #endregion
    }
}
