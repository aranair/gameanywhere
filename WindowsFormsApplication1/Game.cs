using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere.Data
{
    /// <summary>
    /// This class is primarily a data encapsulation of all information regarding a single Game.
    /// </summary>
    public class Game
    {
        #region Properties
        /// <summary>
        /// List of config files or folders for the game.
        /// </summary>
        private List<string> configPathList;

        /// <summary>
        /// List of saved game files or folders for the game.
        /// </summary>
        private List<string> savePathList;

        /// <summary>
        /// Name of the game.
        /// </summary>
        private string name;

        /// <summary>
        /// Install path of the game.
        /// </summary>
        private string installPath;

        /// <summary>
        /// The parent folder where the list of config files or folders of the game are located.
        /// </summary>
        private string configParentPath;

        /// <summary>
        /// The parent folder where the list of saved game files or folders of the game are located.
        /// </summary>
        private string saveParentPath;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public List<string> ConfigPathList
        {
            get { return configPathList; }
            set { configPathList = value; }
        }
        public List<string> SavePathList
        {
            get { return savePathList; }
            set { savePathList = value; }
        }
        public string InstallPath
        {
            get { return installPath; }
            set { installPath = value; }
        }
        public string ConfigParentPath
        {
            get { return configParentPath; }
            set { configParentPath = value; }
        }
        public string SaveParentPath
        {
            get { return saveParentPath; }
            set { saveParentPath = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Overloaded constructor to instantiate a Game with the given information.
        /// </summary>
        /// <param name="configPathList">List of config file paths for the game.</param>
        /// <param name="savePathList">List of saved game paths for the game</param>
        /// <param name="name">Name of game</param>
        /// <param name="installPath">Install path of the game</param>
        /// <param name="configParentPath">Parent folder path for the config files</param>
        /// <param name="saveParentPath">Parent saved game paths for the config files</param>
        public Game(List<string> configPathList, List<string> savePathList, string name, string installPath, string configParentPath, string saveParentPath)
        {
            this.configPathList = configPathList;
            this.savePathList = savePathList;
            this.name = name;
            this.installPath = installPath;
            this.configParentPath = configParentPath;
            this.saveParentPath = saveParentPath;
        }

        /// <summary>
        /// Default constructor: instantiates a game object with blank lists for configPathList, savePathList
        /// and empty strings for name, installPath, saveParentPath, and configParentPath.
        /// </summary>
        public Game()
        {
            configPathList = new List<string>(); 
            savePathList = new List<string> ();
            name = "";
            installPath = "";
            saveParentPath = "";
            configParentPath = "";
        }
        #endregion

        
    }
}
