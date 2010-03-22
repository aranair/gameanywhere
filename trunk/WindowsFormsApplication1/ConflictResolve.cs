using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameAnywhere
{
    public partial class ConflictResolve : Form
    {
        #region Properties
        /// <summary>
        /// Controller to facilitate all the communication between other layers.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// List of checkBoxes which are used for determining user's choice for game files.
        /// </summary>
        private List<CheckBox> checkBoxList;

        /// <summary>
        /// yAxis location to put the current new dynamic control at.
        /// </summary>
        private int yAxisLocation = 50;

        /// <summary>
        /// List of Conflicts to be resolved.
        /// </summary>
        private Dictionary<string, int> conflictsList;
        #endregion

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="controller">Controller to do comunicate with the other classes</param>
        /// <param name="conflictsList">List of conflicts to be resolved.</param>
        public ConflictResolve (Controller controller, Dictionary<string,int> conflictsList)
        {
            InitializeComponent();
            this.controller = controller;
            this.conflictsList = conflictsList;

            DisplayConflicts();
        }

        private void DisplayConflicts()
        {
            checkBoxList = new List<CheckBox>();

            this.SuspendLayout();
            // Creates a set of GUI elements per game, namely: 
            // 1 game name label, 1 checkBox if config files are available, 1 checkBox if saved game files are available.
            foreach (string key in conflictsList.Keys)
            {
                string s = key;
                string gameName = SubstringFromLeft(ref s);
                string fileType = SubstringFromLeft(ref s);

                // Creates and edit a new label to display each game name and add it to the display panel
                CreateLabel(gameName, showConflictsPanel);
                CreateGameIconPictureBox(gameName, showConflictsPanel);

                yAxisLocation += 70;

            }//end foreach (Game g in gameList)

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Takes a string and takes the left most characters until the first / and returns that string,
        /// at the same time modifies the string sent in.
        /// </summary>
        /// <param name="key">The string to be substringed.</param>
        /// <returns>The resultant string from the substring operation.</returns>
        private string SubstringFromLeft(ref string key)
        {
            key = key.Remove(key.IndexOf("/"));
            return key;
            
        }

        /// <summary>
        /// Creates a green game label, to be added into the appropriate container.
        /// </summary>
        /// <param name="gameName">Name of the game to be displayed.</param>
        /// <param name="container">Container for the label to be added in.</param>
        private void CreateLabel(string gameName, Control container)
        {
            System.Windows.Forms.Label gameLabel;
            gameLabel = new Label();
            gameLabel.BackColor = System.Drawing.Color.Black;
            gameLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            gameLabel.Location = new System.Drawing.Point(85, yAxisLocation);
            gameLabel.Name = "gameLabel" + gameName;
            gameLabel.Size = new System.Drawing.Size(200, 20);
            gameLabel.Text = gameName;
            gameLabel.ForeColor = System.Drawing.Color.LimeGreen;
            container.Controls.Add(gameLabel);
        }

        /// <summary>
        /// Creates an Picture box to display an icon of the game in and add it to the appropriate container.
        /// </summary>
        /// <param name="gameName">Name of the game to be displayed.</param>
        /// <param name="container">Container for the picturebox to be added in.</param>
        private void CreateGameIconPictureBox(string gameName, Control container)
        {
            System.Windows.Forms.PictureBox gameIcon;
            gameIcon = new PictureBox();
            gameIcon.Location = new System.Drawing.Point(0, yAxisLocation - 20);
            gameIcon.Name = "gameIcon" + gameName;
            gameIcon.Size = new System.Drawing.Size(50, 50);
            SetIcon(gameIcon, gameName);
            container.Controls.Add(gameIcon);
        }

        #region Helper Functions

        public void SetBackgroundImage(System.Windows.Forms.Control o, string resourcePath, ImageLayout imageLayout)
        {
            System.IO.Stream imageStream = this.GetType().Assembly.GetManifestResourceStream(resourcePath);
            o.BackgroundImage = Image.FromStream(imageStream);
            o.BackgroundImageLayout = imageLayout;
            imageStream.Close();
        }

        private void SetVisibilityAndUsability(System.Windows.Forms.Control c, bool makeVisible, bool makeEnabled)
        {
            c.Visible = makeVisible;
            c.Enabled = makeEnabled;
        }

        /// <summary>
        /// Assigns the appropriate icon image to the icon box sent in as argument.
        /// </summary>
        /// <param name="gameIconPictureBox">Icon box to be set the background image.</param>
        /// <param name="gameName">Name of the game to be displayed.</param>
        private void SetIcon(PictureBox gameIconPictureBox, string gameName)
        {
            if (gameName.Equals(GameLibrary.FIFA10GameName))
                SetBackgroundImage(gameIconPictureBox, "GameAnywhere.Resources.fifa2010.png", ImageLayout.Zoom);

            if (gameName.Equals(GameLibrary.Warcraft3GameName))
                SetBackgroundImage(gameIconPictureBox, "GameAnywhere.Resources.warcraft3.png", ImageLayout.Zoom);

            if (gameName.Equals(GameLibrary.FM2010GameName))
                SetBackgroundImage(gameIconPictureBox, "GameAnywhere.Resources.footballManagerIcon1.gif", ImageLayout.Zoom);

            if (gameName.Equals(GameLibrary.WOWGameName))
                SetBackgroundImage(gameIconPictureBox, "GameAnywhere.Resources.worldOfWarcraftIcon1.gif", ImageLayout.Zoom);

        }
        #endregion
    }
}
