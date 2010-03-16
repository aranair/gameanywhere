using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;

namespace GameAnywhere
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*un-comment this to run test driver
            if (MessageBox.Show("Run Test Driver","", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Tester());
            }
            else
            {
                Controller c = new Controller();
            }
            */
            
            Controller c = new Controller();
        }
    }
}
