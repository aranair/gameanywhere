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
        private int yAxisLocation = 37;

        /// <summary>
        /// List of Conflicts to be resolved.
        /// </summary>
        private Dictionary<string, int> conflictsList;

        /// <summary>
        /// Sorted and grouped list of conflicts.
        /// </summary>
        private Dictionary<string, string> allConflictedGamesAndType;

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

            allConflictedGamesAndType = FindConflictedGamesAndType(conflictsList);
            // 1st string is       : gameName
            // 2nd string is either: config, savedGame, both

            this.SuspendLayout();

            // Creates a set of GUI elements per game, namely: 
            // Each key in allConflictedGamesAndType is one game.
            foreach (string key in allConflictedGamesAndType.Keys)
            {                
                string gameName = key;
                string type = allConflictedGamesAndType[gameName];

                // Creates and edit a new label to display each game name and add it to the display panel
                CreateLabel(gameName, showConflictsPanel);
                CreateGameIconPictureBox(gameName, showConflictsPanel);
                yAxisLocation += 20;

                if (type.Equals("config") || type.Equals("both"))
                {
                    // Create a tag to say Config files.
                    // Create 2 checkbox for upload/download
                    CreateConfigItems(gameName);
                    yAxisLocation += 40;
                }

                if (type.Equals("savedGame") || type.Equals("both"))
                {
                    // Create a tag to say Saved Game Files
                    // Create 2 checkbox for upload/download
                    CreateSavedGameItems(gameName);
                    yAxisLocation += 40;
                }
                
                yAxisLocation += 50;

            }//end foreach 

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Creates one label and 2 checkboxes for the saved game files for a particular game
        /// </summary>
        /// <param name="gameName">Name of the game to create the config items for.</param>
        private void CreateSavedGameItems(string gameName)
        {
            CreateLabel(new System.Drawing.Point(85, yAxisLocation), "Saved Game Files:", showConflictsPanel);
            CreateUploadCheckBox(gameName, "savedGame");
            CreateDownloadCheckBox(gameName, "savedGame");
        }

        /// <summary>
        /// Creates one label and 2 checkboxes for the config files for a particular game
        /// </summary>
        /// <param name="gameName">Name of the game to create the config items for.</param>
        private void CreateConfigItems(string gameName)
        {
            CreateLabel(new System.Drawing.Point(85, yAxisLocation), "Config Files:", showConflictsPanel);
            CreateUploadCheckBox(gameName, "config");
            CreateDownloadCheckBox(gameName, "config");
        }

        /// <summary>
        /// Creates one checkbox for the direction upload.
        /// </summary>
        /// <param name="g">Game to create the checkbox for.</param>
        /// /// <param name="type">Type of files that this checkbox is handling</param>
        private void CreateUploadCheckBox(string gameName, string type)
        {
            System.Windows.Forms.CheckBox uploadCheckBox;
            uploadCheckBox = new CheckBox();
            uploadCheckBox.Name = "upload/" + gameName + type;
            //uploadCheckBox.Text = "Keep Thumbdrive Version";
            //SetBackgroundImage(uploadCheckBox, "GameAnywhere.Resources.upload.gif", ImageLayout.Center);
            SetImageForBackground(uploadCheckBox, "GameAnywhere.Resources.upload.gif", ContentAlignment.MiddleLeft);
            uploadCheckBox.Font = new System.Drawing.Font("Verdana", 8F);
            uploadCheckBox.Size = new System.Drawing.Size(150, 38);
            uploadCheckBox.Location = new System.Drawing.Point(270, yAxisLocation-10);
            uploadCheckBox.BackColor = System.Drawing.Color.Transparent;
            uploadCheckBox.Click += new EventHandler(groupCheckBox_Click);
            uploadCheckBox.CheckedChanged += new EventHandler(groupCheckBox_CheckChanged);

            showConflictsPanel.Controls.Add(uploadCheckBox);
            checkBoxList.Add(uploadCheckBox);
        }

        /// <summary>
        /// Creates one checkbox for the direction download.
        /// </summary>
        /// <param name="g">Name of the game to create the checkbox for.</param>
        /// <param name="type">Type of files that this checkbox is handling</param>
        private void CreateDownloadCheckBox(string gameName, string type)
        {
            System.Windows.Forms.CheckBox downloadCheckBox;
            downloadCheckBox = new CheckBox();
            downloadCheckBox.Name = "download/" + gameName + type;
            downloadCheckBox.CheckAlign = ContentAlignment.MiddleRight;
            //downloadCheckBox.Text = "Keep Web Version";

            //SetBackgroundImage(downloadCheckBox, "GameAnywhere.Resources.download.gif", ImageLayout.Center);
            SetImageForBackground(downloadCheckBox, "GameAnywhere.Resources.download.gif", ContentAlignment.MiddleRight);

            downloadCheckBox.Size = new System.Drawing.Size(120, 38);
            downloadCheckBox.Font = new System.Drawing.Font("Verdana", 8F);
            downloadCheckBox.Location = new System.Drawing.Point(410, yAxisLocation-10);
            downloadCheckBox.BackColor = System.Drawing.Color.Transparent;
            downloadCheckBox.Click += new EventHandler(groupCheckBox_Click);
            downloadCheckBox.CheckedChanged += new EventHandler(groupCheckBox_CheckChanged);

            showConflictsPanel.Controls.Add(downloadCheckBox);
            checkBoxList.Add(downloadCheckBox);
        }

        /// <summary>
        /// Sorts the conflictsList into Games, and according to the availability of the type of files for that game, 
        /// it will edit the dictionary value for the game accordingly.
        /// </summary>
        /// <param name="conflictsList">Dictionary list to read and sort.</param>
        /// <returns>The grouped dictionary list of games, and their conflict type.</returns>
        private Dictionary<string, string> FindConflictedGamesAndType (Dictionary<string, int> conflictsList)
        {
            Dictionary<string, string> conflictedGamesAndType = new Dictionary<string,string>();
            foreach (string key in conflictsList.Keys)
            {
                string s = key;
                string gameName = s.Remove(s.IndexOf("/"));
                string fileType = s.Substring(s.IndexOf("/") + 1);
                if (!conflictedGamesAndType.ContainsKey(gameName))
                    conflictedGamesAndType.Add(gameName, "none");
                
                if (fileType.Equals ("config"))
                    AssignDictionaryValue(ref conflictedGamesAndType, gameName, "config");

                if (fileType.Equals("savedGame"))
                    AssignDictionaryValue(ref conflictedGamesAndType, gameName, "savedGame");            
            }
            return conflictedGamesAndType;
        }

        /// <summary>
        /// Assigns the appropriate value to the dictionary key according to what current status it is in.
        /// </summary>
        /// <param name="conflictedGamesAndType">The dictionary list to be editted.</param>
        /// <param name="gameName">The name of the game: also the dictionary key.</param>
        /// <param name="type">The type of file to add into the key.</param>
        private void AssignDictionaryValue(ref Dictionary<string, string> conflictedGamesAndType, string gameName, string type)
        {
            if (type.Equals("config"))
            {
                if (conflictedGamesAndType[gameName].Equals("none"))
                    conflictedGamesAndType[gameName] = "config";
                else if (conflictedGamesAndType[gameName].Equals("savedGame"))
                    conflictedGamesAndType[gameName] = "both";
            }

            if (type.Equals("savedGame"))
            {
                if (conflictedGamesAndType[gameName].Equals("none"))
                    conflictedGamesAndType[gameName] = "savedGame";
                else if (conflictedGamesAndType[gameName].Equals("config"))
                    conflictedGamesAndType[gameName] = "both";
            }
        }

        /// <summary>
        /// Takes a string and takes the left most characters until the first / and returns that string,
        /// at the same time modifies the string sent in.
        /// </summary>
        /// <param name="key">The string to be substringed.</param>
        /// <returns>The resultant string from the substring operation.</returns>
        private string SubstringFromLeft(ref string key)
        {
            string s = key.Remove(key.IndexOf("/"));
            key = key.Substring(key.IndexOf("/"));
            return s;
            
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
            gameLabel.Location = new System.Drawing.Point(85, yAxisLocation-10);
            gameLabel.Name = "gameLabel" + gameName;
            gameLabel.Size = new System.Drawing.Size(200, 20);
            gameLabel.Text = gameName;
            gameLabel.ForeColor = System.Drawing.Color.LimeGreen;
            container.Controls.Add(gameLabel);
        }

        /// <summary>
        ///  Creates a label to display the string and add it to the appropriate container.
        /// </summary>
        /// <param name="location">Location to add this label to in the container.</param>
        /// <param name="txt">Text to be displayed.</param>
        /// <param name="container">Container for the label to be added in.</param>
        private void CreateLabel(Point location, string txt, Control container)
        {
            Label newLabel = new Label();
            newLabel = new Label();
            newLabel.BackColor = System.Drawing.Color.Black;
            newLabel.Size = new System.Drawing.Size(130, 30);
            newLabel.Location = location;
            newLabel.Text = txt;
            newLabel.ForeColor = System.Drawing.Color.White;
            container.Controls.Add(newLabel);
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
            gameIcon.Location = new System.Drawing.Point(0, yAxisLocation - 15);
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

        public void SetImageForBackground(CheckBox o, string resourcePath, ContentAlignment contentAlignment)
        {

            System.IO.Stream imageStream = this.GetType().Assembly.GetManifestResourceStream(resourcePath);
            o.Image = Image.FromStream(imageStream);
            o.ImageAlign = contentAlignment;
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


        /// <summary>
        /// Sets the fonts of the checkBox according to their checked state.
        /// </summary>
        /// <param name="checkBox">Checkbox to be analyzed and editted.</param>
        private void SetImage(CheckBox checkBox)
        {
            string type = checkBox.Name.Remove(checkBox.Name.IndexOf("/"));
            string imagePathChecked = "";
            string imagePathUnChecked = "";

            if (type.Equals("upload"))
            {
                imagePathChecked   = "GameAnywhere.Resources.uploadArrowed.gif";
                imagePathUnChecked = "GameAnywhere.Resources.upload.gif";

                if (checkBox.Checked)
                    SetImageForBackground(checkBox, imagePathChecked, ContentAlignment.MiddleLeft);
                else // Set to original without arrow
                    SetImageForBackground(checkBox, imagePathUnChecked, ContentAlignment.MiddleLeft);
            }
            else if (type.Equals("download"))
            {
                imagePathChecked   = "GameAnywhere.Resources.downloadArrowed.gif";
                imagePathUnChecked = "GameAnywhere.Resources.download.gif";

                if (checkBox.Checked)
                    SetImageForBackground(checkBox, imagePathChecked, ContentAlignment.MiddleRight);
                else
                    SetImageForBackground(checkBox, imagePathUnChecked, ContentAlignment.MiddleRight);
            }

       
            
        }

        /// <summary>
        /// Region handles the event that ANY checkBox is ticked or unticked.
        /// </summary>
        private void groupCheckBox_CheckChanged(object sender, EventArgs e)
        {
            bool allGamesVerified = true;
            // Go through every set of checkBoxes -> grouped by per game per type.
            foreach (string key in allConflictedGamesAndType.Keys)
            {
                //each key is a game and file.
                string value = allConflictedGamesAndType[key];
                if (value.Equals("config") || value.Equals("both"))
                    VerifyConfigCheckBoxes(key, ref allGamesVerified);
   
                if (value.Equals ("savedGame") || value.Equals ("both"))
                    VerifySavedCheckBoxes(key, ref allGamesVerified);
            }

            if (allGamesVerified)
            {
                SetVisibilityAndUsability(confirmButton, true, true);
                SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButton.gif", ImageLayout.Zoom);
            }
            else // not all games and types are verified yet.
            {
                SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonDulled.gif", ImageLayout.Zoom);
                SetVisibilityAndUsability(confirmButton, true, false);
            }

        }

        /// <summary>
        /// Verifies that either the upload or download checkbox is ticked for this particular game's saved game files.
        /// </summary>
        /// <param name="key">Name of the game that is being checked.</param>
        /// <param name="allGamesVerified">Boolean variable to be modified if it is not verified.</param>
        private void VerifySavedCheckBoxes(string key, ref bool allGamesVerified)
        {
            // find corresponding checkbox first.
            CheckBox uploadCheckBox = FindCheckBox("upload/" + key + "savedGame");
            CheckBox downloadCheckBox = FindCheckBox("download/" + key + "savedGame");

            if (!uploadCheckBox.Checked && !downloadCheckBox.Checked)
                allGamesVerified = false;
        }

        /// <summary>
        /// Verifies that either the upload or download checkbox is ticked for this particular game's config files.
        /// </summary>
        /// <param name="key">Name of the game that is being checked.</param>
        /// <param name="allGamesVerified">Boolean variable to be modified if it is not verified.</param>
        private void VerifyConfigCheckBoxes(string key, ref bool allGamesVerified)
        {
            // find corresponding checkbox first.
            CheckBox uploadCheckBox = FindCheckBox("upload/" + key + "config");
            CheckBox downloadCheckBox = FindCheckBox("download/" + key + "config");

            if (!uploadCheckBox.Checked && !downloadCheckBox.Checked)
                allGamesVerified = false;
        }

        /// <summary>
        /// Finds a checkbox from the list of checkboxes that has the name passed in.
        /// </summary>
        /// <param name="p">Name of checkbox to be found.</param>
        /// <returns>Checkbox with the name passed in.</returns>
        private CheckBox FindCheckBox(string checkBoxName)
        {
            foreach (CheckBox checkBox in checkBoxList)
            {
                if (checkBox.Name.Equals(checkBoxName))
                    return checkBox;
            }
            return new CheckBox();
            
        }

        /// <summary>
        /// Region handles the event that ANY checkBox is clicked by user.
        /// </summary>
        private void groupCheckBox_Click(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            SetImage(senderCheckBox);

            // Untick partner checkBox
            UntickPartnerCheckBox(senderCheckBox.Name);    
        }

        /// <summary>
        /// Unticks and sets the font of the checkbox that is of the other direction,
        /// Defines the mutual exclusiveness of each of the two checkBoxes.
        /// </summary>
        /// <param name="senderName">The name of the checkbox that has been ticked, (unticks the partner of this checkbox)</param>
        private void UntickPartnerCheckBox(string senderName)
        {
            string nameFrontPortion = senderName.Remove(senderName.IndexOf("/")); // upload or download 
            string nameBackPortion = senderName.Substring(senderName.IndexOf("/")); // game name  + config or savedGame

            if (nameFrontPortion.Equals("upload"))
                nameFrontPortion = "download";
            else if (nameFrontPortion.Equals("download"))
                nameFrontPortion = "upload";

            // Find checkbox and untick it.
            CheckBox checkBox = FindCheckBox(nameFrontPortion + nameBackPortion);

            checkBox.Checked = false;
            SetImage(checkBox);
            
        }

        

        #region confirmButton mouse events
        private void confirmButton_MouseClick(object sender, MouseEventArgs e)
        {
            Dictionary<string, int> newDictionary = new Dictionary<string, int>();

            foreach (string key in conflictsList.Keys)
            {
                string s = key;
                string gameName = s.Remove(s.IndexOf("/"));
                string fileType = s.Substring(s.IndexOf("/") + 1);
                
                CheckBox uploadCheckBox   = FindCheckBox("upload/" + gameName + fileType);
                CheckBox downloadCheckBox = FindCheckBox("download/" +gameName + fileType);

                if (uploadCheckBox.Checked)
                    newDictionary.Add(key, 1);
                else if (downloadCheckBox.Checked)
                    newDictionary.Add(key, 2);
            }
        }


        private void confirmButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonMouseDown.gif", ImageLayout.Zoom);
        }
        private void confirmButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonMouseOver.gif", ImageLayout.Zoom);
        }
        private void confirmButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButton.gif", ImageLayout.Zoom);
        }
        private void confirmButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonMouseOver.gif", ImageLayout.Zoom);
        }
        #endregion
    }
}
