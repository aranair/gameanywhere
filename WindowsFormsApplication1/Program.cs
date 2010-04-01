﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;


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
            //GameLibrary gl = new GameLibrary();
            //gl.initGamesFromFile();

            Controller c = new Controller();
 
            /*
            Dictionary<string, int> cList = new Dictionary<string, int>();
            cList.Add("Warcraft 3/savedGame", 0);
            cList.Add("Warcraft 3/config", 0);
            cList.Add("World of Warcraft/config", 0);
            cList.Add("Football Manager 2010/config", 0);
            cList.Add("Football Manager 2010/savedGame", 0);
            cList.Add("FIFA 10/savedGame", 0);
            ConflictResolve cr = new ConflictResolve(c, cList);
            cr.ShowDialog();*/
        }
    }
}