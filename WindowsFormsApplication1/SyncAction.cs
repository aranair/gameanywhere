using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GameAnywhere
{
    /// <summary>
    /// A data structure that contains a Game and its sync information.
    /// </summary>
    public class SyncAction
    {

        /// <summary>
        /// Sync action, do nothing.
        /// </summary>
        public static readonly int DoNothing = 0; //default

        /// <summary>
        /// Sync action, game configuration files.
        /// </summary>
        public static readonly int ConfigFiles = 1;
        /// <summary>
        /// Sync action, saved game files.
        /// </summary>
        public static readonly int SavedGameFiles = 2;
        /// <summary>
        /// Sync action, all game files.
        /// </summary>
        public static readonly int AllFiles = 3;

        //Data member
        Game myGame = null;
        int action = DoNothing;
        List<SyncError> unsuccessfulSyncFiles;

        #region Propeties
        /// <summary>
        /// A Game object.
        /// </summary>
        public Game MyGame
        {
            get { return myGame; }
            set { myGame = value; }
        }
        /// <summary>
        /// The sync action related to the Game object.
        /// </summary>
        public int Action
        {
            get { return action; }
            set { action = value; }
        }
        /// <summary>
        /// The list of files that cannot be sync.
        /// </summary>
        public List<SyncError> UnsuccessfulSyncFiles
        {
            get { return unsuccessfulSyncFiles; }
            set { unsuccessfulSyncFiles = value; }
        }
        #endregion


        /// <summary>
        /// Consructor.
        /// </summary>
        public SyncAction()
        {
            unsuccessfulSyncFiles = new List<SyncError>();
        }

        /// <summary>
        /// Overloaded consructor.
        /// </summary>
        /// <param name="game">Game object.</param>
        public SyncAction(Game game)
        {
            myGame = game;
            unsuccessfulSyncFiles = new List<SyncError>();
        }

       /// <summary>
        /// Overloaded consructor.
       /// </summary>
        /// <param name="game">Game object.</param>
       /// <param name="type">The sync action for the game.</param>
        public SyncAction(Game game, int type)
        {
            myGame = game; 
            action = type;
            unsuccessfulSyncFiles = new List<SyncError>();
        }
       
    }

    
}
