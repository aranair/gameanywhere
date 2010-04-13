//Boa Ho Man
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using GameAnywhere.Process;
using GameAnywhere.Data;
using System.Threading;

namespace GameAnywhere.Interface
{
    /// <summary>
    /// This form allows the user to choose games and the file types to synchronize.
    /// </summary>
    public partial class ChooseGame : Form
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
        /// List of <class>Game</class> which are available for display.
        /// </summary>
        private List<Game> gameList;

        /// <summary>
        /// List of <class>SyncAction</class> to send for sync after users chooses the game files.
        /// </summary>
        private List<SyncAction> syncActionList;

        /// <summary>
        /// List of <class>SyncAction</class> to send for sync after users chooses the game files.
        /// </summary>
        private List<SyncAction> syncActionListResult;

        /// <summary>
        /// yAxis location to put the current new dynamic control at.
        /// </summary>
        private int yAxisLocation = 50;

        private startPage parent;

        private WaitingDialog waitDialog;

        private Thread waitThread;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor, intializes GUI elements and a blank game list.
        /// </summary>
        public ChooseGame(Controller controller)
        {  
            InitializeComponent();
            gameList = new List<Game>();
            this.controller = controller;
        }

        /// <summary>
        /// Overloaded constructor: 
        /// First, it puts the game list passed in, into the game list of this instance.
        /// Then , it initializes GUI elements and calls a helper function to display this game list to user.
        /// </summary>
        /// <param name="controller">Controller to do comunicate with the other classes</param>
        /// <param name="gList">List of games to be displayed</param>
        /// <param name="errorLabel">Error label of the startPage form</param>
        public ChooseGame(Controller controller, List<Game> gameList, startPage parent)
        {
            this.controller = controller;
            //this.errorLabel = errorLabel;
            this.gameList = gameList;

            InitializeComponent();
            this.parent = parent;

            // Displays the game list for user to choose files.
            DisplayGameList();

        }

        #endregion

        #region Game Files Display Pages
        /// <summary>
        /// Creates 2 checkBoxes for each game dynamically, according to the list of games present in this instance
        /// for users to choose the game files.
        /// 
        /// Pre-condition : There is a list of games ( with games in it ).
        /// Post-condition: List of games are displayed to user with checkBoxes created for the files available for synchronization.
        /// </summary>
        public void DisplayGameList()
        {
            checkBoxList = new List<CheckBox>();
            syncActionList = new List<SyncAction>();

            // No Games! 
            if (gameList.Count == 0)
                Close();   
            
            // This panel is to display list of games to user
            showGamePanel.BackColor = System.Drawing.Color.Black;

            // Passes in current Y axis to start the generation of game display.
            GenerateGameDisplay();
        }

        /// <summary>
        /// Dynamically generates the games display.
        /// </summary>
        /// <param name="gameList">List of games to be displayed</param>
        private void GenerateGameDisplay()
        {
            this.SuspendLayout();
            // Creates a set of GUI elements per game, namely: 
            // 1 game name label, 1 checkBox if config files are available, 1 checkBox if saved game files are available.
            foreach (Game g in gameList)
            {
                // Creates and edit a new label to display each game name and add it to the display panel
                CreateLabel(g, showGamePanel);

                CreateGameIconPictureBox(g, showGamePanel);

                // If config files exist for a game, create a checkBox for user to select.
                if (g.ConfigPathList.Count > 0)  
                    CreateConfigCheckBox(g);
                else // Config files do not exist, create an error label
                    CreateErrorConfigFileLabel(g);

                // If saved game files exist for a game, create a checkBox for user to select.
                if (g.SavePathList.Count > 0)
                    CreateSavedGameCheckBox(g);
                else // if saved game files do not exist, create an error label to alert user (checkBox should not be created)                   
                    CreateErrorSavedGameLabel(g);

                yAxisLocation += 70;

            }//end foreach (Game g in gameList)

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        /// <summary>
        /// Creates one error label if saved game files are not available to sync for the game.
        /// </summary>
        /// <param name="g">Game to create the label for.</param>
        private void CreateErrorSavedGameLabel(Game g)
        {
            System.Windows.Forms.Label errorLabel;
            string errorMessage = "";
            if (g.Name.Equals("World of Warcraft"))
                errorMessage = " - No Game UI Files -";
            else
                errorMessage = "- No Saved Games -";

            errorLabel = new Label();
            errorLabel.Name = "errorLabelSave" + g.Name;
            errorLabel.Text = errorMessage;
            errorLabel.Size = new System.Drawing.Size(200, 20);
            errorLabel.Location = new System.Drawing.Point(467, yAxisLocation);
            errorLabel.ForeColor = System.Drawing.Color.Gray;
            errorLabel.BackColor = System.Drawing.Color.Black;
            showGamePanel.Controls.Add(errorLabel);
        }

        /// <summary>
        /// Creates one error label if config files are not available to sync for the game.
        /// </summary>
        /// <param name="g">Game to create the label for.</param>
        private void CreateErrorConfigFileLabel(Game g)
        {
            System.Windows.Forms.Label errorLabelcg;
            errorLabelcg = new Label();
            errorLabelcg.Name = "errorLabelcgConfig" + g.Name;
            errorLabelcg.Text = " - No Config Files -";
            errorLabelcg.Size = new System.Drawing.Size(170, 20);
            errorLabelcg.Location = new System.Drawing.Point(297, yAxisLocation);
            errorLabelcg.ForeColor = System.Drawing.Color.Gray;
            errorLabelcg.BackColor = System.Drawing.Color.Black;
            showGamePanel.Controls.Add(errorLabelcg);
        }

        /// <summary>
        /// Creates one checkbox for saved game files option for the game.
        /// </summary>
        /// <param name="g">Game to create the checkbox for.</param>
        private void CreateSavedGameCheckBox(Game g)
        {
            System.Windows.Forms.CheckBox savedGameCheckBox;
            savedGameCheckBox = new CheckBox();
            savedGameCheckBox.Name = "savedGameCheckBox" + g.Name;
            savedGameCheckBox.Size = new System.Drawing.Size(200, 30);
            savedGameCheckBox.Location = new System.Drawing.Point(470, yAxisLocation-8);
            savedGameCheckBox.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            savedGameCheckBox.BackColor = System.Drawing.Color.Black;
            savedGameCheckBox.CheckedChanged += new EventHandler(groupCheckBox_CheckedChanged);

            // An exception for World of Warcraft has been made to this part
            // because the game does not consist of saved game files, only game UI files exist.
            if (g.Name.Equals("World of Warcraft"))
                savedGameCheckBox.Text = "Game UI";
            else
                savedGameCheckBox.Text = "Saved Games";
            showGamePanel.Controls.Add(savedGameCheckBox);
            checkBoxList.Add(savedGameCheckBox);
        }

        /// <summary>
        /// Creates one checkbox for config files option for the game.
        /// </summary>
        /// <param name="g">Game to create the checkbox for.</param>
        private void CreateConfigCheckBox(Game g)
        {
            System.Windows.Forms.CheckBox configCheckBox;
            configCheckBox = new CheckBox();
            configCheckBox.Name = "configCheckBox" + g.Name;
            configCheckBox.Text = "Config Files";
            configCheckBox.Size = new System.Drawing.Size(170, 30);
            configCheckBox.Location = new System.Drawing.Point(300, yAxisLocation-8);
            configCheckBox.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            configCheckBox.BackColor = System.Drawing.Color.Black;
            configCheckBox.CheckedChanged += new EventHandler(groupCheckBox_CheckedChanged);
            showGamePanel.Controls.Add(configCheckBox);
            checkBoxList.Add(configCheckBox);
        }

        /// <summary>
        /// Assigns game to the syncAction and adds it to the final list to be sent.
        /// </summary>
        /// <param name="syncAction">SyncAction to be finalized and added.</param>
        /// <param name="g">Game to be added to the syncAction.</param>
        private void FinalizeSyncAction(SyncAction syncAction, Game g)
        {
            syncAction.MyGame = g;
            syncActionList.Add(syncAction);
        }

        /// <summary>
        /// Sets the action in the syncAction.
        /// </summary>
        /// <param name="syncAction">SyncAction to be modified</param>
        /// <param name="s">Either "Config" or "SavedGame"</param>
        private static void SetSyncAction(SyncAction syncAction, string s)
        {
            if (s.Equals("Config"))
            {
                if (syncAction.Action == SyncAction.DoNothing)
                    syncAction.Action = SyncAction.ConfigFiles;
                else if (syncAction.Action == SyncAction.SavedGameFiles)
                    syncAction.Action = SyncAction.AllFiles;
            }
            else if (s.Equals("SavedGame"))
            {
                if (syncAction.Action == SyncAction.DoNothing)
                    syncAction.Action = SyncAction.SavedGameFiles;
                else if (syncAction.Action == SyncAction.ConfigFiles)
                    syncAction.Action = SyncAction.AllFiles;
            }
        }

        /// <summary>
        /// Creates an Picture box to display an icon of the game in and add it to the appropriate container.
        /// </summary>
        /// <param name="g">Game to be displayed (in icon).</param>
        /// <param name="container">Container for the picturebox to be added in.</param>
        private void CreateGameIconPictureBox(Game g, Control container)
        {
            System.Windows.Forms.PictureBox gameIcon;
            gameIcon = new PictureBox();
            gameIcon.Location = new System.Drawing.Point(0, yAxisLocation-20);
            gameIcon.Name = "gameIcon" + g.Name;
            gameIcon.Size = new System.Drawing.Size(50, 50);
            SetIcon(gameIcon, g);
            container.Controls.Add(gameIcon);
        }

        /// <summary>
        /// Creates a green game label, to be added into the appropriate container.
        /// </summary>
        /// <param name="g">Game, of which the Game Name to be displayed in words.</param>
        /// <param name="container">Container for the label to be added in.</param>
        private void CreateLabel(Game g, Control container)
        {
            System.Windows.Forms.Label gameLabel;
            gameLabel = new Label();
            gameLabel.BackColor = System.Drawing.Color.Black;
            gameLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            gameLabel.Location = new System.Drawing.Point(85, yAxisLocation);
            gameLabel.Name = "gameLabel" + g.Name;
            gameLabel.Size = new System.Drawing.Size(200, 20);
            gameLabel.Text = g.Name;
            gameLabel.ForeColor = System.Drawing.Color.LimeGreen;
            container.Controls.Add(gameLabel);
        }

        /// <summary>
        ///  Creates a label to display the string and add it to the appropriate container.
        /// </summary>
        /// <param name="location">Location to add this label to in the container.</param>
        /// <param name="txt">Text to be displayed.</param>
        /// <param name="container">Container for the label to be added in.</param>
        private static void CreateLabel(Point location, string txt, Control container)
        {
            Label newLabel = new Label();
            newLabel = new Label();
            newLabel.BackColor = System.Drawing.Color.Black;
            newLabel.Size = new System.Drawing.Size(320, 30);
            newLabel.Location = location;
            newLabel.Text = txt;
            newLabel.ForeColor = System.Drawing.Color.White;
            container.Controls.Add(newLabel);
        }
        #endregion

        #region Sync Result Pages
        /// <summary>
        /// Creates a panel to display synchronization results dynamically from the list of syncActions passed.
        /// Within each syncAction will contain another detailed list of synchronization errors.
        /// 
        /// Pre-condition : There are syncActions in the list passed in.
        /// Post-condition: Errors/Success displayed to user dynamically.
        /// </summary>
        /// <param name="syncActionListDisplay">List of syncActions that represent the results.</param>
        private void DisplaySyncResult(List<SyncAction> syncActionListDisplay)
        {
            this.Focus();
            Debug.Assert(syncActionListDisplay.Count != 0);
            yAxisLocation = 50;

            bool containsError = false;
            // Check if there are any errors first.
            foreach (SyncAction syncAction in syncActionListDisplay)
            {
                if (syncAction.UnsuccessfulSyncFiles.Count > 0)
                    containsError = true;
            }
            // If there are no errors at all
            if (!containsError)
            {
                // No need to display individual sync results.
                parent.SetErrorLabel("Successfully Synchronized.", System.Drawing.Color.DeepSkyBlue);
                this.Close();
                return;
            }

            this.SuspendLayout();
            // Creates a set of GUI elements to display for each syncAction in the list of syncActions
            foreach (SyncAction syncAction in syncActionListDisplay)
            {
                Game resultGame = syncAction.MyGame;

                // Create label to display game name.
                CreateLabel(resultGame, resultPanel);

                // Create picture box to display game icon.
                CreateGameIconPictureBox(resultGame, resultPanel);

                // No errors for this syncAction: create a label to to let user know it was a success for this game.
                if (syncAction.UnsuccessfulSyncFiles.Count > 0)
                {
                    if (controller.direction == OfflineSync.ExternalToCom || controller.direction == OnlineSync.ComToWeb 
                        || controller.direction == OnlineSync.WebToCom)
                    {
                        //ShowSyncErrors(syncAction.UnsuccessfulSyncFiles, ref yAxisControl);
                        CreateLabel(new System.Drawing.Point(300, yAxisLocation), "Synchronization Failed.\n- Please re-synchronize.", resultPanel);
                    }
                    else if (controller.direction == OfflineSync.ComToExternal)
                        CreateLabel(new System.Drawing.Point(300, yAxisLocation), "Synchronization Failed.\n- Please re-synchronize or manually copy error files.", resultPanel);
                        
                    yAxisLocation += 20;
                }
                else
                {
                    CreateLabel(new System.Drawing.Point(300, yAxisLocation), "Successfully Synchronized.", resultPanel);
                    yAxisLocation += 20; 
                }
                yAxisLocation += 40;
            }//end foreach syncAction

            // Creates the done button in this panel (result display) for user to finish up 
            // this synchronization procedure and go back to the main screen.
            SetupButtonsResultPanel();

            // Change banner's image to "Sync Results";
            SetBackgroundImage(banner, "GameAnywhere.Resources.bannerSyncResults.gif", ImageLayout.Zoom);

            // Hides/Shows appropriate panels to show the Sync result panel.
            SetVisibilityAndUsability(resultPanel, true, true);
            SetVisibilityAndUsability(showGamePanel, false, false);

            this.PerformLayout();
        }

        /// <summary>
        /// Finds and matches a checkBox in the list and sets the action in the syncActions with the appropriate values
        /// to be sent to the controller later.
        /// </summary>
        /// <param name="syncAction">SyncAction to be editted</param>
        /// <param name="g">The game to be found a match (with its corresponding 2 checkBoxes)</param>
        private void ReadCheckBoxInformation(ref SyncAction syncAction, Game g)
        {
            foreach (CheckBox cb in checkBoxList)
            {
                string matchConfigCheckBox = "configCheckBox" + g.Name;
                string matchSavedGameCheckBox = "savedGameCheckBox" + g.Name;

                // If it matches, it is the corresponding checkBox for this game's config files.
                // Checkbox ticked -> user has selected to sync config files for this game.
                if (cb.Name.Equals(matchConfigCheckBox) && cb.Checked)
                    SetSyncAction(syncAction, "Config");

                // If it matches, it is the corresponding checkBox for this game's saved games.
                // Checkbox ticked -> user has selected to sync saved game files for this game.
                if (cb.Name.Equals(matchSavedGameCheckBox) && cb.Checked)
                    SetSyncAction(syncAction, "SavedGame");

            }// end (foreach checkbox)
        }

        /// <summary>
        /// Creates and edit the synchronization files to be sent to controller.
        /// Pre-condition: gameList is not empty
        /// Post-condition: SyncActions are properly created according to the user's actions and stored into the list (data member of ChooseGames)
        /// </summary>
        private void SetupSynchronizationFiles()
        {
            foreach (Game g in gameList)
            {
                SyncAction syncAction = new SyncAction();
                // Finds matching configCheckBox and savedGameCheckBox from checkBoxList to see user's choices.
                ReadCheckBoxInformation(ref syncAction, g);

                // If there are no files available. ( but game is installed! )
                if ((g.ConfigPathList.Count == 0 && g.SavePathList.Count == 0)
                    || (syncAction.Action == SyncAction.DoNothing))
                    continue;

                // User has chosen to sync some files for this game -> add it to the list of syncActions
                if (syncAction.Action != 0)
                    FinalizeSyncAction(syncAction, new Game(g.ConfigPathList, g.SavePathList, g.Name, g.InstallPath, g.ConfigParentPath, g.SaveParentPath));

            }// end foreach (Game g in gameList)
        }

        /// <summary>
        /// Disables and enables the appropriate buttons for the sync result panel.
        /// </summary>
        private void SetupButtonsResultPanel()
        {
            SetVisibilityAndUsability(doneResultPanelButton, true, true);
            SetVisibilityAndUsability(errorButton, true, true);
            SetVisibilityAndUsability(confirmButton, false, false);
            SetVisibilityAndUsability(cancelButton, false, false);
        }

        #endregion

        #region Mouse Event Handlers

        // Confirm button in the Game display panel.
        #region confirmButton
        private void confirmButton_MouseClick(object sender, MouseEventArgs e)
        {
            syncActionListResult = new List<SyncAction>();
            SetupSynchronizationFiles();

            try
            {
                OpenWaitDialog("Please wait while your files are being synchronized.");
                syncActionListResult = controller.SynchronizeGames(syncActionList);
                CloseWaitDialog();
            }
            catch (ConnectionFailureException)
            {
                parent.SetErrorLabel("Connection lost. Please re-sync.", Color.Red);
                CloseWaitDialog();
            }

            if (syncActionListResult.Count > 0)
                DisplaySyncResult(syncActionListResult);
            else
            {
                this.Focus();
                this.Close();
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

        // Cancel button in the Game display panel.
        #region cancelButton
        private void cancelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }
        private void cancelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }
        private void cancelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }
        private void cancelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }
        private void cancelButton_MouseClick(object sender, MouseEventArgs e)
        {
            Close();
        }
        #endregion

        // Done button in the Sync Results panel.
        #region doneResultPanelButton
        private void doneResultPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            parent.Focus();
            this.Close();
        }
        private void doneResultPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(doneResultPanelButton, "GameAnywhere.Resources.doneResultPanelButtonMouseDown.gif", ImageLayout.Zoom);
        }
        private void doneResultPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(doneResultPanelButton, "GameAnywhere.Resources.doneResultPanelButtonMouseOver.gif", ImageLayout.Zoom);
        }
        private void doneResultPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(doneResultPanelButton, "GameAnywhere.Resources.doneResultPanelButton.gif", ImageLayout.Zoom);
        }
        private void doneResultPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(doneResultPanelButton, "GameAnywhere.Resources.doneResultPanelButtonMouseOver.gif", ImageLayout.Zoom);
        }

        #endregion 

        // Error button in the Sync Results panel.
        #region errorButton
        private void errorButton_MouseClick(object sender, MouseEventArgs e)
        {
            SyncErrorDisplay syncErrorDisplay = new SyncErrorDisplay(syncActionListResult, this);
            syncErrorDisplay.ShowDialog();
        }

        private void errorButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(errorButton, "GameAnywhere.Resources.errorsButtonMouseDown.gif", ImageLayout.Center);
        }

        private void errorButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(errorButton, "GameAnywhere.Resources.errorsButtonMouseOver.gif", ImageLayout.Center);
        }

        private void errorButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(errorButton, "GameAnywhere.Resources.errorsButton.gif", ImageLayout.Center);
        }

        private void errorButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(errorButton, "GameAnywhere.Resources.errorsButton.gif", ImageLayout.Center);
        }
        #endregion

        /// <summary>
        /// Sets the fonts of the checkBox according to their checked state.
        /// </summary>
        /// <param name="checkBox">Checkbox to be analyzed and editted.</param>
        private static void SetFonts (CheckBox checkBox)
        {
            if (checkBox.Checked)
            {
                checkBox.ForeColor = System.Drawing.Color.DeepSkyBlue;
                checkBox.Font = new System.Drawing.Font("Verdana", 9F); 
                checkBox.Size = new System.Drawing.Size(170, 30);
            }
            else
            {
                checkBox.ForeColor = System.Drawing.Color.White;
                checkBox.Font = new System.Drawing.Font("Verdana", 8F);
                checkBox.Size = new System.Drawing.Size(170, 30);
            }
        }

        /// <summary>
        /// Region handles the event that ANY checkBox is ticked or unticked.
        /// </summary>
        private void groupCheckBox_CheckedChanged(object sender, EventArgs e)
        {  
            bool anyCheckBoxTicked = false;
            foreach (CheckBox cb in checkBoxList)
            {
                SetFonts(cb);
                // if checkBox is ticked
                if (cb.Checked)
                    anyCheckBoxTicked = true;
            }

            if (anyCheckBoxTicked)
            {
                SetVisibilityAndUsability(confirmButton, true, true);
                SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButton.gif", ImageLayout.Zoom);
            }
            else // No checkBox ticked
            {
                SetBackgroundImage(confirmButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonDulled.gif", ImageLayout.Zoom);
                SetVisibilityAndUsability(confirmButton, true, false);
            }

        }

        #endregion

        #region Helper Functions

        private void OpenWaitDialog(string text)
        {
            waitDialog = new WaitingDialog(text);
            waitThread = new Thread(new ThreadStart(waitDialog.startUp));
            waitThread.Start();
        }

        private void CloseWaitDialog()
        {
            try
            {
                waitDialog.ContinueRun = false;
            }
            catch (Exception) { };

            this.Focus();
            this.Enabled = true;

        }

        public void SetBackgroundImage(System.Windows.Forms.Control control, string resourcePath, ImageLayout imageLayout)
        {
            System.IO.Stream imageStream = this.GetType().Assembly.GetManifestResourceStream(resourcePath);

            control.BackgroundImage = Image.FromStream(imageStream);

            control.BackgroundImageLayout = imageLayout;
            imageStream.Close();
            
        }

        private static void SetVisibilityAndUsability(System.Windows.Forms.Control c, bool makeVisible, bool makeEnabled)
        {
            c.Visible = makeVisible;
            c.Enabled = makeEnabled;
        }



        /// <summary>
        /// Assigns the appropriate icon image to the icon box sent in as argument.
        /// </summary>
        /// <param name="gameIconPictureBox">Icon box to be set the background image.</param>
        /// <param name="g">Game that is to be displayed.</param>
        private void SetIcon(PictureBox gameIconPictureBox, Game g)
        {
            string gameName = g.Name;
            string gifIconPath = "GameAnywhere.Resources.GameIcons." + gameName + ".gif";
            string pngIconPath = "GameAnywhere.Resources.GameIcons." + gameName + ".png";
            string jpgIconPath = "GameAnywhere.Resources.GameIcons." + gameName + ".jpg";
            string defaultIconPath = "GameAnywhere.Resources.GameIcons.defaultIcon.gif";

            System.IO.Stream imageStream = this.GetType().Assembly.GetManifestResourceStream(gifIconPath);

            if (imageStream == null)
                imageStream = this.GetType().Assembly.GetManifestResourceStream(pngIconPath);

            if (imageStream == null)
                imageStream = this.GetType().Assembly.GetManifestResourceStream(jpgIconPath);

            if (imageStream == null)
                imageStream = this.GetType().Assembly.GetManifestResourceStream(defaultIconPath);

            gameIconPictureBox.BackgroundImage = Image.FromStream(imageStream);
            gameIconPictureBox.BackgroundImageLayout = ImageLayout.Zoom;
            imageStream.Close();
   
        }
        #endregion

    }
}
