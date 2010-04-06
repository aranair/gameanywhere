using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameAnywhere.Process;
using GameAnywhere.Data;

namespace GameAnywhere.Interface
{
    /// <summary>
    /// This form handles the display of the first restore warning when user tries to exit program.
    /// </summary>
    public partial class restoreWarning : Form
    {
        private Form formParent;
        private Controller controller;
        public bool cancelClicked = false;

        public restoreWarning(Controller controller)
        {
            this.controller = controller;
            InitializeComponent();
        }
        public restoreWarning(Form formParent, Controller controller)
        {
            this.controller = controller;
            InitializeComponent();
            this.formParent = formParent;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            cancelClicked = true;
            this.Close();
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            // User has clicked not to restore. Therefore a warning dialog should be displayed.
            reconfirmationWarning formReconfirmationWarning = new reconfirmationWarning(controller,this,this.formParent);
            formReconfirmationWarning.ShowDialog();
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            // User has chosen to restore.
            List<SyncAction> syncActionList = new List<SyncAction>();
            List<string> gameListNotRestored = new List<string>();
            syncActionList = controller.Restore();

            foreach (SyncAction sa in syncActionList)
            {
                if (sa.UnsuccessfulSyncFiles.Count > 0)
                {
                    gameListNotRestored.Add(sa.MyGame.Name);
                }
            }

            if (gameListNotRestored.Count > 0)
            {
                string gameList = "";

                foreach (string s in gameListNotRestored)
                {
                    gameList = gameList + "- " + s + "\n";
                }

                MessageBox.Show("The following games were not restored successfully. It is advised that you manually restore these games by "
                    + "going to the game directory, copying out the original files in the backup folders and deleting the backup folders before the next usage of GameAnywhere.\n\n" + gameList, 
                    "Warning"); 
            }

            
            
            formParent.Close();
            this.Close();
        }

    }
}
