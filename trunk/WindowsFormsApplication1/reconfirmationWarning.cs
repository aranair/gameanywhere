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
    /// <summary>
    /// This form handles the display of the 2nd warning dialog when user chooses not to restore original game files.
    /// </summary>
    public partial class reconfirmationWarning : Form
    {
        private Form formParent;
        private Form parent_formParent; //parent of parent form
        private Controller controller;

        public reconfirmationWarning()
        {
            InitializeComponent();
        }
        public reconfirmationWarning(Controller controller, Form formParent, Form parent_formParent)
        {
            this.controller = controller;
            this.formParent = formParent;
            this.parent_formParent = parent_formParent; 
            InitializeComponent();
        }

        private void buttonOk_MouseClick(object sender, MouseEventArgs e)
        {
            // User has chosen not to back up.
            List<SyncError> syncErrorList = new List<SyncError>();
            List<string> errorFoldersList = new List<string>();

            syncErrorList = controller.RemoveAllBackup();

            foreach (SyncError syncError in syncErrorList)
            {
                errorFoldersList.Add(syncError.FilePath);
            }

            if (errorFoldersList.Count > 0)
            {
                string errors = "";

                foreach (string s in errorFoldersList)
                {
                    errors = errors + "- " + s + "\n";
                }

                MessageBox.Show("The following backup folders cannot be removed. It is advised that you manually remove these folders before the next usage of GameAnywhere.\n\n"
                    + errors, "Warning");
            }
            formParent.Close();
            parent_formParent.Close();        
        }

        private void buttonCancel_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}
