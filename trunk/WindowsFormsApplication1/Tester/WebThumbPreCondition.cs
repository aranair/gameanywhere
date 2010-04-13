//Tan Hong Zhou
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security;
using System.Diagnostics;

using System.Reflection;
using Shell32;
using System.Threading;
using System.Runtime.InteropServices;

using GameAnywhere.Data;

namespace GameAnywhere.Process
{
    /// <summary>
    /// Set the pre-conditions for CheckConflicts and SynchronizeGames method test cases.
    /// </summary>
    class WebThumbPreCondition : PreCondition
    {

        /// <summary>
        /// Set the pre-condition of CheckConflict method
        /// </summary>
        /// <param name="index">Index of the Test Case</param>
        /// <param name="localHash">Reference to the local hash</param>
        /// <param name="localMeta">Reference to the local Meta</param>
        /// <param name="webHash">Reference to the web Meta</param>
        /// <param name="webMeta">Reference to the web Meta</param>
        internal static void SetCheckConflictPreCondition(int index, ref MetaData localHash, ref MetaData localMeta, ref MetaData webHash, ref MetaData webMeta)
        {
            switch (index)
            {
                case 1:
                    localHash.AddEntry("Game1/savedGame/File 1", "A");
                    localHash.AddEntry("Game2/savedGame/File 2", "B");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");

                    webHash.AddEntry("Game1/savedGame/File 1", "A");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    break;
                case 2: //different file at web
                    localHash.AddEntry("Game1/savedGame/File 1", "A");
                    localHash.AddEntry("Game2/savedGame/File 2", "B");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");

                    webHash.AddEntry("Game1/savedGame/File 1", "X");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    break;
                case 3: //different file at thumb
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localHash.AddEntry("Game2/savedGame/File 2", "B");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");

                    webHash.AddEntry("Game1/savedGame/File 1", "A");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    break;
                case 4: //conflicts on both side
                    localHash.AddEntry("Game1/savedGame/File 1", "A");
                    localHash.AddEntry("Game2/savedGame/File 2", "X");
                    localHash.AddEntry("Game3/savedGame/File 3", "C");
                    localHash.AddEntry("Game4/savedGame/File 4", "D");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");
                    localMeta.AddEntry("Game3/savedGame/File 3", "C");
                    localMeta.AddEntry("Game4/savedGame/File 4", "D");

                    webHash.AddEntry("Game1/savedGame/File 1", "A");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");
                    webHash.AddEntry("Game3/savedGame/File 3", "X");
                    webHash.AddEntry("Game4/savedGame/File 4", "D");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    webMeta.AddEntry("Game3/savedGame/File 3", "C");
                    webMeta.AddEntry("Game4/savedGame/File 4", "D");
                    break;
                case 5:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localHash.AddEntry("Game2/savedGame/File 2", "X");
                    localHash.AddEntry("Game3/savedGame/File 3", "C");
                    localHash.AddEntry("Game4/savedGame/File 4", "D");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");
                    localMeta.AddEntry("Game3/savedGame/File 3", "C");
                    localMeta.AddEntry("Game4/savedGame/File 4", "D");

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");
                    webHash.AddEntry("Game3/savedGame/File 3", "X");
                    webHash.AddEntry("Game4/savedGame/File 4", "D");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    webMeta.AddEntry("Game3/savedGame/File 3", "C");
                    webMeta.AddEntry("Game4/savedGame/File 4", "D");
                    break;
                case 6:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localHash.AddEntry("Game2/savedGame/File 2", "X");
                    localHash.AddEntry("Game3/savedGame/File 3", "C");
                    localHash.AddEntry("Game4/savedGame/File 4", "D");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");
                    localMeta.AddEntry("Game3/savedGame/File 3", "C");
                    localMeta.AddEntry("Game4/savedGame/File 4", "D");

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");
                    webHash.AddEntry("Game3/savedGame/File 3", "X");
                    //webHash.AddEntry("File 4", "D");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    webMeta.AddEntry("Game3/savedGame/File 3", "C");
                    webMeta.AddEntry("Game4/savedGame/File 4", "D");
                    break;
                case 7:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localHash.AddEntry("Game2/savedGame/File 2", "X");
                    localHash.AddEntry("Game3/savedGame/File 3", "C");
                    //localHash.AddEntry("File 4", "D");
                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");
                    localMeta.AddEntry("Game3/savedGame/File 3", "C");
                    localMeta.AddEntry("Game4/savedGame/File 4", "D");

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");
                    webHash.AddEntry("Game3/savedGame/File 3", "X");
                    webHash.AddEntry("Game4/savedGame/File 4", "D");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    webMeta.AddEntry("Game3/savedGame/File 3", "C");
                    webMeta.AddEntry("Game4/savedGame/File 4", "D");
                    break;
                case 8:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localHash.AddEntry("Game2/savedGame/File 2", "X");
                    localHash.AddEntry("Game3/savedGame/File 3", "C");
                    localHash.AddEntry("Game4/savedGame/File 4", "D");
                    //none
                    localHash.AddEntry("Game6/savedGame/File 6", "F");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game2/savedGame/File 2", "B");
                    localMeta.AddEntry("Game3/savedGame/File 3", "C");
                    localMeta.AddEntry("Game4/savedGame/File 4", "D");
                    localMeta.AddEntry("Game5/savedGame/File 5", "E");
                    //none

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "B");
                    webHash.AddEntry("Game3/savedGame/File 3", "X");
                    //none
                    webHash.AddEntry("Game5/savedGame/File 5", "E");
                    //none
                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game2/savedGame/File 2", "B");
                    webMeta.AddEntry("Game3/savedGame/File 3", "C");
                    webMeta.AddEntry("Game4/savedGame/File 4", "D");
                    webMeta.AddEntry("Game5/savedGame/File 5", "E");
                    //none
                    break;
                case 9:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    break;
                case 10:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "A");
                    break;
                case 11:
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    //none
                    localHash.AddEntry("Game3/savedGame/File 3", "A");
                    localHash.AddEntry("Game4/savedGame/File 4", "W");
                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game2/savedGame/File 2", "A");
                    //none
                    webHash.AddEntry("Game4/savedGame/File 4", "Z");
                    break;
                case 12: //special case
                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    break;

                case 13: //DeleteLocalConflict
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    //webhash - none
                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    break;

                case 14: //DeleteWebConflict
                    //localHash - none
                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webHash.AddEntry("Game1/savedGame/File 1", "X");
                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    break;

                case 15: //check only one return
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    //Game1/savedGame/File 2 - none

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game1/savedGame/File 2", "B");

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game1/savedGame/File 2", "X");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game1/savedGame/File 2", "B");
                    break;
                case 16: //check multiple return
                    localHash.AddEntry("Game1/savedGame/File 1", "X");
                    //Game1/savedGame/File 2 none
                    localHash.AddEntry("Game1/config/File 3", "X");
                    localHash.AddEntry("Game2/config/File 4", "X");

                    localMeta.AddEntry("Game1/savedGame/File 1", "A");
                    localMeta.AddEntry("Game1/savedGame/File 2", "B");
                    localMeta.AddEntry("Game1/config/File 3", "C");
                    localMeta.AddEntry("Game2/config/File 4", "D");

                    webHash.AddEntry("Game1/savedGame/File 1", "Y");
                    webHash.AddEntry("Game1/savedGame/File 2", "X");
                    //Game1/config - none
                    webHash.AddEntry("Game2/config/File 4", "Y");

                    webMeta.AddEntry("Game1/savedGame/File 1", "A");
                    webMeta.AddEntry("Game1/savedGame/File 2", "B");
                    webMeta.AddEntry("Game1/config/File 3", "C");
                    webMeta.AddEntry("Game2/config/File 4", "D");
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Clean up the Web and Thumb test cases pre-conditions
        /// </summary>
        /// <param name="index"></param>
        /// <param name="user"></param>
        internal static void CleanUpWebAndThumb(int index, string user)
        {
            string key = "WebAndThumb@gmails.com";
            Storage store = new Storage();
            switch (index)
            {
                //add test cases for future expansion

                case 17:
                case 18:
                    {
                        FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game1\savedGame\File 1.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game2\config\File 4.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);
                        break;
                    }
                case 19:
                    {
                        //resume online
                        int i = 0;
                        while (!ToggleNetworkAdapter(true) && i < 100) ++i;
                        if (i == 100) MessageBox.Show("Network cannot be resumed");
                        while (!IsConnectedToInternet()) ;
                        break;
                    }

                default: break;
            }
            try
            {
                //if key exist delete it
                store.DeleteDirectory(key);
            }
            catch { }
            Directory.Move(@".\SyncFolder", @".\localTest" + index);
            if (Directory.Exists(@".\SyncFolder-test2"))
                Directory.Move(@".\SyncFolder-test2", @".\SyncFolder");

            for (int i = 1; i <= 21; i++)
            {
                if (Directory.Exists(@".\webTest" + i))
                    Directory.Delete(@".\webTest" + i, true);
                if (Directory.Exists(@".\localTest" + i))
                    Directory.Delete(@".\localTest" + i, true);
                if (Directory.Exists(@".\metaTest" + i))
                    Directory.Delete(@".\metaTest" + i, true);
                if (Directory.Exists(@".\webTest" + i + "-test"))
                    Directory.Delete(@".\webTest" + i + "-test", true);
                if (Directory.Exists(@".\localTest" + i + "-test"))
                    Directory.Delete(@".\localTest" + i + "-test", true);
            }
        }

        /// <summary>
        /// Set the precondition for the Web and Thumb sync
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        /// <param name="user"></param>
        internal static void SetWebAndThumbPreCondition(int index, ref object[] input, ref object testClass, string user)
        {
            MetaData localHash = new MetaData();
            MetaData localMeta = new MetaData();
            MetaData webHash = new MetaData();
            MetaData webMeta = new MetaData();
            WebAndThumbSync webThumb;

            switch (index)
            {
                //case 17 locked then setup files
                case 18:
                    {
                        //set up files before lock
                        webThumb = WebAndThumbFileSetUp(index);
                        //Lock Game1\savedGame Folder
                        FolderOperation.AddFileSecurity(@".\SyncFolder\Game1\savedGame\File 1.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(@".\SyncFolder\Game2\config\File 4.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);

                        break;
                    }
                case 19:
                    {
                        webThumb = WebAndThumbFileSetUp(index);
                        //go to offline mode
                        int i = 0;
                        while (!ToggleNetworkAdapter(false) && i < 100) ++i;
                        if (i == 100) MessageBox.Show("Network cannot be off");
                        Thread.Sleep(1000);
                        break;
                    }
                default:
                    webThumb = WebAndThumbFileSetUp(index);
                    break;
            }

            testClass = webThumb;
        }

        /// <summary>
        /// SetUp the actual files and folders for testing of WebThumb Synchronization
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static WebAndThumbSync WebAndThumbFileSetUp(int index)
        {
            string id = "WebAndThumb@gmails.com";
            Storage s3 = new Storage();
            User gaUser = new User();
            gaUser.Email = id;
            WebAndThumbSync webThumb = null;

            if (Directory.Exists(@".\SyncFolder"))
                Directory.Move(@".\SyncFolder", @".\SyncFolder-test2");

            //switch is used here to allow future expansion of test case.
            switch (index)
            {
                case 17:
                    {
                        //upload all web files
                        List<string> allFiles = FolderOperation.GetAllFilesName(@".\webTest" + index);

                        //upload the test cases files of web test folder to the web
                        foreach (string file in allFiles)
                        {
                            string path = file.Substring(file.IndexOf('\\') + 1);
                            s3.UploadFile(file, (id + "/" + path.Substring(path.IndexOf('\\') + 1).Replace('\\', '/')));
                        }

                        Directory.Move(@".\localTest" + index, @".\SyncFolder");

                        //Lock Game1\savedGame Folder
                        FolderOperation.AddFileSecurity(@".\SyncFolder\Game1\savedGame\File 1.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);

                        break;
                    }
                default:
                    {
                        //upload all web files
                        List<string> allFiles = FolderOperation.GetAllFilesName(@".\webTest" + index);

                        //upload the test cases files of web test folder to the web
                        foreach (string file in allFiles)
                        {
                            string path = file.Substring(file.IndexOf('\\') + 1);
                            if (Path.GetFileName(file).Equals("metadata.ga"))
                                continue;
                            s3.UploadFile(file, (id + "/" + path.Substring(path.IndexOf('\\') + 1).Replace('\\', '/')));
                        }

                        //rename test
                        Directory.Move(@".\localTest" + index, @".\SyncFolder");

                        break;
                    }
            }
            try
            {
                webThumb = new WebAndThumbSync(gaUser);
                Dictionary<string, int> conflicts = new Dictionary<string, int>();
                webThumb.CheckConflicts();
            }
            catch (Exception err)
            {
                if (err.GetType().Equals(typeof(UnauthorizedAccessException)))
                {
                    FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game1\savedGame\File 1.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);

                    webThumb = new WebAndThumbSync(gaUser);
                    webThumb.LocalHash.AddEntry("UnauthorizedAccessException", "x");
                    return webThumb;
                }
                Directory.Move(@".\SyncFolder", @".\localTest" + index);
                if (Directory.Exists(@".\SyncFolder-test2"))
                    Directory.Move(@".\SyncFolder-test2", @".\SyncFolder");

                for (int i = 1; i <= 16; i++)
                {
                    if (Directory.Exists(@".\webTest" + i))
                        Directory.Delete(@".\webTest" + i, true);
                    if (Directory.Exists(@".\localTest" + i))
                        Directory.Delete(@".\localTest" + i, true);
                    if (Directory.Exists(@".\metaTest" + i))
                        Directory.Delete(@".\metaTest" + i, true);
                    if (Directory.Exists(@".\webTest" + i + "-test"))
                        Directory.Delete(@".\webTest" + i + "-test", true);
                    if (Directory.Exists(@".\localTest" + i + "-test"))
                        Directory.Delete(@".\localTest" + i + "-test", true);
                }
                FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game1\savedGame\File 1.txt", user, FileSystemRights.FullControl, AccessControlType.Deny);
                return null;
            }

            return webThumb;
        }
    }
}
