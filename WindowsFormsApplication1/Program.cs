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
            Dictionary<string,int> d = new Dictionary<string,int>();
            d.Add("Warcraft 3/config", 0);
            d.Add("Warcraft 3/savedGame", 0);
            d.Add("World of Warcraft/savedGame", 0);
            ConflictResolve cr = new ConflictResolve(c, d);
            cr.ShowDialog();

        }
    }
}
