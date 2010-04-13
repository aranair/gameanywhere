using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameAnywhere.Data;

namespace GameAnywhere.Interface
{
    /// <summary>
    /// This class handles the display of detailed errors after synchronization.
    /// </summary>
    public partial class SyncErrorDisplay : Form
    {
        #region Constructors and Data Members
        // The parent who called this form ( to facilitate .close())
        private Form parent;
        private List<SyncAction> syncActionList;

        /// <summary>
        /// A variable to determine the current yAxis location to facilitate the dynamic creation of the displays.
        /// </summary>
        private int yAxisLocation = 30;

       
       

        public SyncErrorDisplay(List<SyncAction> syncActionList, Form parent)
        {
            this.parent = parent;
            parent.Close();

            InitializeComponent();
            this.syncActionList = syncActionList;

            // Function to start displaying the errors from the list of syncActions.
            DisplayErrors();
        }

        public SyncErrorDisplay(List<SyncError> syncErrorList, Form parent)
        {
            this.parent = parent;
            parent.Close();

            InitializeComponent();
            CompileSyncErrors(syncErrorList);
            
            // Function to start displaying the errors from the list of syncActions.
            DisplayErrors();
        }

        private void CompileSyncErrors(List<SyncError> syncErrorList)
        {
            this.syncActionList = new List<SyncAction>();
            foreach (SyncError se in syncErrorList)
            {
                string gameName = se.FilePath;
                Game newGame = new Game(new List<string>(), new List<string>(),gameName, "", "", "");
                bool gameExists = false;
                foreach (SyncAction sa in syncActionList)
                {
                    if (sa.MyGame.Name.Equals(newGame.Name))
                    {
                        sa.UnsuccessfulSyncFiles.Add(se);
                        gameExists = true;
                    }
                }
                if (gameExists == false)
                {
                    SyncAction newSyncAction = new SyncAction(newGame);
                    newSyncAction.UnsuccessfulSyncFiles.Add(se);
                    syncActionList.Add(newSyncAction);
                }

            }
        }

        public SyncErrorDisplay(List<SyncError> syncErrorList)
        {
            InitializeComponent();
            CompileSyncErrors(syncErrorList);

            // Function to start displaying the errors from the list of syncActions.
            DisplayErrors();
        }
        #endregion

        /// <summary>
        /// Dynamically creates a display for the errors.
        /// </summary>
        private void DisplayErrors()
        {
            this.SuspendLayout();
           
            // Creates a set of GUI elements per game, namely: 
            // 1 game name label, 1 checkBox if config files are available, 1 checkBox if saved game files are available.
            foreach (SyncAction syncAction in syncActionList)         
            {
                if (syncAction.UnsuccessfulSyncFiles.Count > 0)
                {
                    Game game = syncAction.MyGame;
                    // Creates and edit a new label to display each game name and add it to the display panel
                    CreateLabel(game, errorDisplayPanel);
                    
                    // Function responsible for creating all the labels to display all the errors for this sync action result.
                    ShowSyncErrors(syncAction.UnsuccessfulSyncFiles);

                    // Adds one extra number of space after each game!
                    yAxisLocation = yAxisLocation + 40;
                }

            }//end foreach (Game g in gameList)

            this.ResumeLayout(true);
            this.PerformLayout();
            
           
        }

        /// <summary>
        /// Creates all the labels to display all the errors for this sync action result.
        /// </summary>
        /// <param name="syncErrorList"></param>
        private void ShowSyncErrors(List<SyncError> syncErrorList)
        {
            bool backupErrorMessageShown = false;
            bool thumbdriveInsufficientSpaceMessageShown = false;
            bool computerInsufficientSpaceMessageShown = false;

            // for each error, this portion shows the file path that was not accessible and the error involved.
            foreach (SyncError syncError in syncErrorList)
            {
                string errorText = syncError.ErrorMessage;

                // Bypass the error message if its any of these 3 and it has been shown for this game.
                if (   (errorText.Equals("Unable to backup original game files.") && backupErrorMessageShown)
                    || (errorText.Equals("Insufficient space in external storage device.") && thumbdriveInsufficientSpaceMessageShown)
                    || (errorText.Equals("Insufficient space in computer.") && computerInsufficientSpaceMessageShown))
                    continue;

                CreateSyncErrorLabel(errorText, errorDisplayPanel);
                MessageBox.Show(errorText);
                if (errorText.Contains("There is not enough space on the disk."))
                    break;
     
                // Edit duplicate flags.
                if (errorText.Equals("Unable to backup original game files."))
                    backupErrorMessageShown = true;
                if (errorText.Equals("Insufficient space in external storage device."))
                    thumbdriveInsufficientSpaceMessageShown = true;
                if (errorText.Equals("Insufficient space in computer."))
                    computerInsufficientSpaceMessageShown = true;
            }
        }

        /// <summary>
        /// Creates a single label to display one error.
        /// </summary>
        /// <param name="txt">The error message to be displayed.</param>
        /// <param name="container">The container for the error label to be put in.</param>
        private void CreateSyncErrorLabel(string txt, Panel container)
        {
            int totalLinesNeeded = 1;
            int totalCharacters = 0;

            Label newLabel = new Label();
            newLabel.Text = txt;
            newLabel.BackColor = System.Drawing.Color.Black;
            newLabel.Size = new System.Drawing.Size(620, 15);
            newLabel.Location = new System.Drawing.Point(35, yAxisLocation);            
            newLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;

            // For lines that are longer than the width of the panel, do calculations and add appropriate height
            // to the label to display the full error.
            totalCharacters = FindLabelLength(ref newLabel);
            
            // Portion edits the label's height and yAxisLocation variable according to the character count.
            if (totalCharacters > 530)
            {
                totalLinesNeeded = (int)Math.Ceiling((totalCharacters / 640.0)) + 1;
                newLabel.Size = new System.Drawing.Size(620, totalLinesNeeded * 15);
                yAxisLocation = yAxisLocation + (totalLinesNeeded * 15);
            }
            else
                yAxisLocation = yAxisLocation + 15;
            container.Controls.Add(newLabel);
        }

        /// <summary>
        /// Finds the length required for the label's text and font
        /// </summary>
        /// <param name="newLabel">Label to be checked.</param>
        private int FindLabelLength(ref Label newLabel)
        {
            int totalCharacters = 0;
            using (System.Drawing.Graphics g = newLabel.CreateGraphics())
            {
                totalCharacters = (int)g.MeasureString(newLabel.Text, newLabel.Font).Width;
            }
            return totalCharacters;
            
        }

        /// <summary>
        /// Creates a single label for the game.
        /// </summary>
        /// <param name="g">Game that is being displayed</param>
        /// <param name="container">Container that the label is being put in.</param>
        private void CreateLabel(Game g, Control container)
        {
            System.Windows.Forms.Label gameLabel;
            gameLabel = new Label();
            gameLabel.BackColor = System.Drawing.Color.Black;
            gameLabel.Location = new System.Drawing.Point(35, yAxisLocation);
            gameLabel.Name = "gameLabel" + g.Name;
            gameLabel.Size = new System.Drawing.Size(200, 20);
            gameLabel.Text = g.Name;
            gameLabel.ForeColor = System.Drawing.Color.LimeGreen;
            gameLabel.Font = new System.Drawing.Font("Verdana", 9.75F);
            container.Controls.Add(gameLabel);
            yAxisLocation = yAxisLocation + 20;
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
        #endregion

        // Region handles mouse events for the done button in the sync result page.
        #region doneErrorDisplayButton
        private void doneErrorDisplayButton_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
        private void doneErrorDisplayButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(doneErrorDisplayButton, "GameAnywhere.Resources.doneResultPanelButtonMouseDown.gif", ImageLayout.Zoom);
        }
        private void doneErrorDisplayButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(doneErrorDisplayButton, "GameAnywhere.Resources.doneResultPanelButtonMouseOver.gif", ImageLayout.Zoom);
        }
        private void doneErrorDisplayButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(doneErrorDisplayButton, "GameAnywhere.Resources.doneResultPanelButton.gif", ImageLayout.Zoom);
        }
        private void doneErrorDisplayButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(doneErrorDisplayButton, "GameAnywhere.Resources.doneResultPanelButtonMouseOver.gif", ImageLayout.Zoom);
        }

        #endregion 
    }
}
