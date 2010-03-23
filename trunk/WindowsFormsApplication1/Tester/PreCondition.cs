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

namespace GameAnywhere
{
    /// <summary>
    /// PreCondition Class is used to set the preconditions for each of the test cases if neccessary
    /// It also contains Helper methods to clean up the extra files/folders created during the testing process
    /// 
    /// For each test case decription, please refer to the text file decription.
    /// </summary>
    class PreCondition
    {
        
        private static string externalPath = @".\SyncFolder";
        private static List<string> supportedGames = new List<string>(new string[] { "Warcraft 3", "FIFA 10" });
        public static readonly string Warcraft3GameName = "Warcraft 3";
        public static readonly string FIFA10GameName = "FIFA 10";
        private static GameLibrary library = new GameLibrary();
        private static string fifaSavePath;
        private static string wc3InstallPath;

        /// <summary>
        /// Store the list of games which are sync on the web.
        /// </summary>
        private static List<string> syncGamesOnWeb;

        /// <summary>
        /// Remove the test folders created for verifying the outcome of action
        /// </summary>
        private static void DeleteTestBackup()
        {
            foreach (Game game in library.InstalledGameList)
            {
                string saveBackup = Path.Combine(game.SaveParentPath, "SaveTestBackup");
                string configBackup = Path.Combine(game.ConfigParentPath, "ConfigTestBackup");

                if (Directory.Exists(saveBackup))
                    Directory.Delete(saveBackup, true);
                if (Directory.Exists(configBackup))
                    Directory.Delete(configBackup, true);
                
            }
            
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        /// <summary>
        /// Check the status of internet connection
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        /// <summary>
        /// Remove any files/folders created in the process of testing
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="index"></param>
        public static void CleanUp(object cleanUpClass, string methodName, int index)
        {
            string user = WindowsIdentity.GetCurrent().Name;
            switch (methodName)
            {
                case "CheckConflicts":
                    
                    break;
                case "GetGameList":
                    {
                        switch(index)
                        {
                            case 1: break;
                            case 2: 
                                //restore
                                FolderOperation.CopyDirectory(Path.Combine(externalPath, "Restore Folder"), externalPath);
                                break;
                            case 3:
                            case 4:
                                //restore all games folder back to original
                                RestoreTestFolderName();
                                break;
                            case 5:
                                break;
                            case 6:
                                string username = WindowsIdentity.GetCurrent().Name;
                                //resume game folder
                                FolderOperation.RemoveFileSecurity(externalPath+@"\FIFA 10", username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10", username,
                                    FileSystemRights.FullControl, AccessControlType.Allow);

                                FolderOperation.RemoveFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                FolderOperation.AddFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Allow);
                                break;
                            case 7:
                                Directory.Move(fifaSavePath + "-test", fifaSavePath); //rename the folder
                                break;
                            case 8:
                                Directory.Move(wc3InstallPath.Substring(0, wc3InstallPath.Length - 1) + "-test", wc3InstallPath);
                                break;
                        }
                        break;
                    }
                case "SynchronizeGames":
                    {


                        if (cleanUpClass.GetType().Equals(typeof(OfflineSync)))
                        {
                            CleanUpOffLineSync(index, user);
                        }
                        else
                        {
                            //Online sync Test cases
                            CleanUpOnlineSync(index, user);
                            break;
                        }

                        
                        break;
                    }
                case "Restore":
                    {
                        switch(index)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                DeleteTestBackup();
                                //delete the save folder, test case 2
                                if (index == 2)
                                {
                                    string path = getGame("Warcraft 3",OfflineSync.Uninitialize).SaveParentPath;
                                    if (Directory.Exists(path + @"\Save"))
                                        Directory.Delete(path + @"\Save", true);
                                }
                                //delete config files in wc3 and save files in fifa 10
                                if (index == 3)
                                {
                                    Game wc3 = getGame("Warcraft 3",OfflineSync.Uninitialize);
                                    Game fifa = getGame("FIFA 10",OfflineSync.Uninitialize);
                                    if (File.Exists(wc3.ConfigParentPath + @"\CustomKeys.txt"))
                                        File.Delete(wc3.ConfigParentPath + @"\CustomKeys.txt");
                                    if (Directory.Exists(fifa.SaveParentPath + @"\A. Profile"))
                                        Directory.Delete(fifa.SaveParentPath + @"\A. Profile", true);
                                }
                                
                            break;
                            case 6: // resume file access control
                            {
                                string username = WindowsIdentity.GetCurrent().Name;
                                FolderOperation.RemoveFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                FolderOperation.AddFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Allow);
                                DeleteTestBackup();
                                break;
                            }
                            case 7:
                            {
                                Game fm = PreCondition.getGame("Football Manager 2010", OfflineSync.ExternalToCom);

                                //resume external
                                FolderOperation.RemoveFileSecurity(fm.SaveParentPath, user,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                FolderOperation.AddFileSecurity(fm.SaveParentPath, user,
                                    FileSystemRights.FullControl, AccessControlType.Allow);

                                //delete the temporary file created
                                if (File.Exists(fm.SaveParentPath + @"\games\FM2010Test.txt"))
                                    File.Delete(fm.SaveParentPath + @"\games\FM2010Test.txt");

                                Directory.CreateDirectory(fm.SaveParentPath + @"\games");

                                if (Directory.Exists(fm.SaveParentPath + @"\GA-savedGameBackup"))
                                    Directory.Delete(fm.SaveParentPath + @"\GA-savedGameBackup", true);

                                if (Directory.Exists(fm.SaveParentPath + @"\SaveTestBackup"))
                                    Directory.Delete(fm.SaveParentPath + @"\SaveTestBackup", true);
                                break;
                            }
                            default: break;
                        }
                        break;
                    }
                case "ChangePassword":
                case "Login":
                case "Register": 
                case "RetrievePassword":
                    break;
                case "ResendActivation":
                    {
                        switch (index)
                        {
                                //network test are being tested together for efficiency
                            case 3:
                                //resume online
                                int i = 0;
                                while (!ToggleNetworkAdapter(true) && i < 100) ++i;
                                if (i == 100) MessageBox.Show("Network cannot be resumed");
                                else
                                    while (!IsConnectedToInternet()) ;
                                break;
                        }
                    break;
                    }
                default: break;
            }

        }

        /// <summary>
        /// This method does the "cleaning" up of the Online Sync action executed for each test case
        /// </summary>
        /// <param name="index"></param>
        /// <param name="user"></param>
        private static void CleanUpOnlineSync(int index, string user)
        {
            Storage store = new Storage();
            string comToWeb = "TestComToWeb@gmails.com";
            string webToCom = "TestWebToCom@gmails.com";
            GameLibrary library = new GameLibrary();
            OfflineSync restore = new OfflineSync(0,library.GetGameList(OfflineSync.Uninitialize));
            switch (index)
            {
                case 1: //create dummy folder back on the web
                case 2: 
                    store.DeleteDirectory(comToWeb);
                    store.UploadFile(@".\readme.txt",comToWeb);
                    restore.Restore();
                    break;
                case 3:
                    //resume online
                    int i = 0;
                    while (!ToggleNetworkAdapter(true) && i < 100) ++i;
                    if (i == 100) MessageBox.Show("Network cannot be resumed");
                    else
                        while (!IsConnectedToInternet()) ;
                    break;

                default: break;
            }

        }

        /// <summary>
        /// This method does the "cleaning" up of the Offline Sync action executed for each test case
        /// </summary>
        /// <param name="index"></param>
        /// <param name="user"></param>
        private static void CleanUpOffLineSync(int index, string user)
        {
            switch (index)
            {
                case 1:
                    DeleteTestBackup();
                    break;
                case 2:
                    DeleteTestBackup();

                    break;
                case 3:
                    {
                        DeleteTestBackup();
                        Game wc3 = PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom);
                        File.Delete(wc3.ConfigParentPath + @"\CustomKeys.txt");
                        Game fifa = PreCondition.getGame("FIFA 10", OfflineSync.ExternalToCom);
                        Directory.Delete(fifa.SaveParentPath + @"\A. Profile", true);
                        break;
                    }
                case 4:
                    {
                        DeleteTestBackup();
                        //rename back the games file
                        RestoreTestFolderName();
                        //delete the copied save files
                        Game fifa = PreCondition.getGame("FIFA 10", OfflineSync.ComToExternal);
                        Directory.Delete(fifa.SaveParentPath + @".\A. Profiles", true);
                        Directory.Delete(fifa.SaveParentPath + @".\I. Be A Pro - Bastard", true);
                        Directory.Delete(fifa.SaveParentPath + @".\I. Be A Pro - Gerald", true);
                        break;
                    }
                case 5:
                    {
                        DeleteTestBackup();
                        //rename back the games file
                        RestoreTestFolderName();
                        //delete the copied save files
                        Game wc3 = PreCondition.getGame("Warcraft 3", OfflineSync.ComToExternal);
                        Game fifa = PreCondition.getGame("FIFA 10", OfflineSync.ComToExternal);
                        Directory.Delete(wc3.SaveParentPath + @"\Save", true);
                        Directory.Delete(fifa.ConfigParentPath + @"\User", true);
                        break;
                    }
                case 6:
                    {
                        DeleteTestBackup();
                        //resume the access of the foler
                        FolderOperation.RemoveFileSecurity(PreCondition.getGame("FIFA 10", OfflineSync.ExternalToCom).ConfigParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(PreCondition.getGame("FIFA 10", OfflineSync.ExternalToCom).ConfigParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Allow);
                        break;
                    }
                case 7: DeleteTestBackup(); break;
                case 8:
                    {
                        DeleteTestBackup();
                        string username = WindowsIdentity.GetCurrent().Name;

                        FolderOperation.RemoveFileSecurity(externalPath + @"\FIFA 10\savedGame\A. Profiles", username,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10\savedGame\A. Profiles", username,
                            FileSystemRights.FullControl, AccessControlType.Allow);

                        break;
                    }
                case 9:
                    {
                        //resume external
                        FolderOperation.RemoveFileSecurity(externalPath + @"\FIFA 10", user,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10", user,
                            FileSystemRights.FullControl, AccessControlType.Allow);
                        //resume game folder
                        FolderOperation.RemoveFileSecurity(fifaSavePath, user,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(fifaSavePath, user,
                            FileSystemRights.FullControl, AccessControlType.Allow);

                        DeleteTestBackup();
                        break;
                    }
                case 10:
                    {
                        Game fm = PreCondition.getGame("Football Manager 2010", OfflineSync.ExternalToCom);
                        //delete the temporary file created
                        if (File.Exists(fm.SaveParentPath + @"\games\FM2010Test.txt"))
                            File.Delete(fm.SaveParentPath + @"\games\FM2010Test.txt");
                        //resume external
                        FolderOperation.RemoveFileSecurity(fm.SaveParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(fm.SaveParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Allow);
                        break;
                    }
                case 11:
                    break;
                default: break;
            }
        }

        private static void RestoreTestFolderName()
        {
            //restore
            string[] games = Directory.GetDirectories(externalPath);
            foreach (string s in games)
            {
                if (s.EndsWith("-test") && Directory.Exists(s))
                {
                    if (Directory.Exists(s.Substring(0, s.Length - 5)))
                        Directory.Delete(s.Substring(0, s.Length - 5), true);
                    try
                    {
                        Directory.Move(s, s.Substring(0, s.Length - 5));
                    }
                    catch (Exception err)
                    {
                        //result.Result = false;
                        MessageBox.Show("Unable to restore : " + s);
                    }
                }
            }

        }

        /// <summary>
        /// Set the pre-condition of each test case.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="methodName"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        public static void SetPreCondition(int index,string methodName,ref object[] input, ref object testClass)
        {
            string user = WindowsIdentity.GetCurrent().Name; //get user account
            switch (methodName)
            {
                case "GetGameList":
                    try
                    {
                        GameLibrary library = new GameLibrary();
                        switch (index)
                        {
                            case 1:
                            case 2: //input 1
                                break;
                            case 3: //input 2, test if it return the list which exist in external and computer
                                string[] games = Directory.GetDirectories(externalPath);
 
                                foreach (string g in games)
                                {
                                    if (supportedGames.Contains(Path.GetFileName(g)))
                                    {
                                        Directory.Move(g, g + "-test"); //rename the folder
                                        break;
                                    }
                                }
                                break;
                            case 4: //intput 2: empty SyncFolder
                                //rename to simulate empty
                                string[] extGame = Directory.GetDirectories(externalPath);
                                foreach(string s in extGame)
                                    Directory.Move(s, s + "-test"); //rename the folder
                                break;
                                
                            case 5:
                                break;
                            case 6: //test for lock game files
                                string username = WindowsIdentity.GetCurrent().Name;
                                FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10", username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                fifaSavePath = getGame("FIFA 10",OfflineSync.Uninitialize).SaveParentPath;
                                FolderOperation.AddFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                break;
                            case 7:
                                fifaSavePath = getGame("FIFA 10",OfflineSync.Uninitialize).SaveParentPath;
                                Directory.Move(fifaSavePath, fifaSavePath + "-test"); //rename the folder
                                break;
                            case 8:
                                wc3InstallPath = getGame("Warcraft 3",OfflineSync.Uninitialize).InstallPath;
                                Directory.Move(wc3InstallPath, wc3InstallPath.Substring(0,wc3InstallPath.Length-1)+"-test");
                                break;
                            //Initialization test case
                            default: break;
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.StackTrace.ToString());
                    }
                    break;
                case "SynchronizeGames":
                    if (testClass.GetType().Equals(typeof(OfflineSync)))
                    {
                        SetOfflineSyncPreCondition(index, ref input, ref testClass, user);
                        break;
                    }
                    else
                    {
                        //Online sync Test cases
                        SetOnlineSyncPreCondition(index, ref input, ref testClass, user);
                        break;
                    }

                    //TODO: modify the file path name , filtering of same parent path in return type
                    //Increase test case to check for filter.
                case "CheckConflicts":
                    {
                        MetaData localHash = new MetaData();
                        MetaData localMeta = new MetaData();
                        MetaData webHash = new MetaData();
                        MetaData webMeta = new MetaData();

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
                                localHash.AddEntry("Game1/savedGame/File 1","X");
                                localMeta.AddEntry("Game1/savedGame/File 1", "A");
                                //webhash - none
                                webMeta.AddEntry("Game1/savedGame/File 1","A");
                                break;

                            case 14: //DeleteWebConflict
                                //localHash - none
                                localMeta.AddEntry("Game1/savedGame/File 1","A");
                                webHash.AddEntry("Game1/savedGame/File 1", "X");
                                webMeta.AddEntry("Game1/savedGame/File 1","A");
                                break;

                            case 15: //check only one return
                                localHash.AddEntry("Game1/savedGame/File 1", "X");
                                //Game1/savedGame/File 2 - none

                                localMeta.AddEntry("Game1/savedGame/File 1","A");
                                localMeta.AddEntry("Game1/savedGame/File 2","B");

                                webHash.AddEntry("Game1/savedGame/File 1","Y");
                                webHash.AddEntry("Game1/savedGame/File 2","X");

                                webMeta.AddEntry("Game1/savedGame/File 1","A");
                                webMeta.AddEntry("Game1/savedGame/File 2","B");
                                break;
                            case 16: //check multiple return
                                localHash.AddEntry("Game1/savedGame/File 1", "X");
                                //Game1/savedGame/File 2 none
                                localHash.AddEntry("Game1/config/File 3", "X");
                                localHash.AddEntry("Game2/config/File 4","X");

                                localMeta.AddEntry("Game1/savedGame/File 1", "A");
                                localMeta.AddEntry("Game1/savedGame/File 2", "B");
                                localMeta.AddEntry("Game1/config/File 3", "C");
                                localMeta.AddEntry("Game2/config/File 4", "D");

                                webHash.AddEntry("Game1/savedGame/File 1", "Y");
                                webHash.AddEntry("Game1/savedGame/File 2", "X");
                                //Game1/config - none
                                webHash.AddEntry("Game2/config/File 4", "Y");

                                webMeta.AddEntry("Game1/savedGame/File 1", "A");
                                webMeta.AddEntry("Game1/savedGame/File 2","B");
                                webMeta.AddEntry("Game1/config/File 3", "C");
                                webMeta.AddEntry("Game2/config/File 4", "D");
                                break;

                            default: break;
                        }

                        WebAndThumbSync webThumb = (WebAndThumbSync)testClass;
                        webThumb.LocalHash = localHash;
                        webThumb.LocalMeta = localMeta;
                        webThumb.WebHash = webHash;
                        webThumb.WebMeta = webMeta;
                        webThumb.NoConflict.Clear();
                        webThumb.Conflicts.Clear();
                        testClass = webThumb;
                        break;
                    }
                case "Restore":
                    GameLibrary lib = new GameLibrary();
                    OfflineSync testRestoreClass = (OfflineSync)testClass;
                    OfflineSync restoreSync = new OfflineSync(OfflineSync.Uninitialize, lib.GetGameList(OfflineSync.Uninitialize));
                    List<Game> restoreGame = (List<Game>)input[0];
                    
                    switch (index)
                    {
                        case 1: //Test direction Ext to Com restore empty folder in fifa 10
                                //check what is in the external is not in the com
                            
                        case 2: //Test direction Ext to com restore save file in wc3
                                                     
                        case 3: //Test direction ext to com, restore config file in wc3, save file in fifa 10
                            
                        case 4:
                        case 5:
                            //restoreSync.SyncDirection = OfflineSync.ExternalToCom;
                            CreateGATestBackup(restoreGame);
                            testClass = restoreSync;
                            
                            break;
                        case 6:
                            //restoreSync.SyncDirection = OfflineSync.ExternalToCom;
                            CreateGATestBackup(restoreGame);
                            testClass = restoreSync;

                            // lock the file access of FIFA, which is to be restored.
                            string username = WindowsIdentity.GetCurrent().Name;
                            fifaSavePath = getGame("FIFA 10",OfflineSync.ExternalToCom).SaveParentPath;
                            FolderOperation.AddFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                            testClass = restoreSync;
                            break;
                        case 7: //disable the option to delete a folder/file in FM 2010 save game
                            
                            Game fm = PreCondition.getGame("Football Manager 2010", OfflineSync.ExternalToCom);
                            FolderOperation.AddFileSecurity(fm.SaveParentPath, user,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                            testClass = restoreSync;
                            break;
                        default: break;
                    }
                    break;
                case "ChangePassword": break;
                case "Login":
                    {
                        switch (index)
                        {
                            case 5:
                                //go to offline mode
                                int i = 0;
                                while (!ToggleNetworkAdapter(false) && i < 100) ++i;
                                if (i == 100) MessageBox.Show("Network cannot be off");
                                Thread.Sleep(1000);
                                break;
                            default: break;
                        }
                        break;
                    }
                case "Register":
                case "RetrievePassword":
                case "ResendActivation":
                        break;

                default: break;
            } //end switch method
            
        }

        /// <summary>
        /// This method will get the Game which are sync on the web
        /// </summary>
        /// <param name="user"></param>
        private static void GetSyncGameFromWeb(string user)
        {
            Storage store = new Storage();
            //store.
        }

        /// <summary>
        /// Used to set online sync pre-condition
        /// Copies all the games over to the com with offline sync
        /// </summary>
        private static void CopyGameFromThumbToWeb()
        {
            string[] gameName = {"FIFA 10","Warcraft 3","World of Warcraft","Football Manager 2010"};
            Game[] games = new Game[4];
            SyncAction[] syncList = new SyncAction[4];
            GameLibrary lib = new GameLibrary();
            for(int i=0; i<4; ++i)
            {
                games[i] = PreCondition.getGame(gameName[i],OfflineSync.ExternalToCom);
                syncList[i] = new SyncAction(games[i], OfflineSync.ExternalToCom);
            }
            OfflineSync off = new OfflineSync(OfflineSync.ExternalToCom, lib.GetGameList(OfflineSync.Uninitialize));
            off.SynchronizeGames(new List<SyncAction>(syncList));
        }

        /// <summary>
        /// Sets the pre-condition for the onine synchronization methods
        /// a helper method for SetPreCondition
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        /// <param name="user"></param>
        private static void SetOnlineSyncPreCondition(int index, ref object[] input, ref object testClass, string user)
        {
            OnlineSync online = (OnlineSync)testClass;
            GameLibrary gameLibrary = new GameLibrary();
            List<SyncAction> synclist = (List<SyncAction>)input[0];
            User newUser = new User();
            
            //copy all sample files of save/config to game folders
            CopyGameFromThumbToWeb();
            
            switch (index)
            {
                case 1: //test normal sync with empty on web, Game: WoW.
                    newUser.Email = "TestComToWeb@gmails.com";
                    testClass = new OnlineSync(OnlineSync.ComToWeb, 
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize),newUser);

                    break;

                case 2: //test sync 2 games WoW interface and Football Manager save
                    newUser.Email = "TestComToWeb@gmails.com";
                    testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize),newUser);
                    break;

                case 3: //test for no connection

                    //go to offline mode
                    int i = 0;
                    while (!ToggleNetworkAdapter(false) && i < 100) ++i;
                    if (i == 100) MessageBox.Show("Network cannot be off");
                    Thread.Sleep(1000);
                    break;

                    testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                    
                    break;
                    //TODO : Check for internet connection breakdown during file transfer.
                default: break;
            }
        }

        /// <summary>
        /// Sets the precondition for the offline synchronization methods
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        /// <param name="user"></param>
        private static void SetOfflineSyncPreCondition(int index, ref object[] input, ref object testClass, string user)
        {
            OfflineSync offSync = (OfflineSync)testClass;
            GameLibrary gameLibrary = new GameLibrary();
            List<SyncAction> synclist = (List<SyncAction>)input[0];

            switch (index)
            {
                case 1: //test only config in ext to com direction
                    {

                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.CONFIG, FolderOperation.ExtToCom);
                        break;
                    }
                case 2: //test only save in ext to com direction
                    {
                        Game wc3 = PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom);
                        Directory.CreateDirectory(wc3.SaveParentPath + @"\Save");
                        FileStream fs = File.Create(wc3.SaveParentPath + @"\Save\newsave.txt");
                        fs.Close();
                        foreach (SyncAction action in synclist)
                            if (action.MyGame.Name.Equals("Warcraft 3"))
                                action.MyGame = getGame("Warcraft 3", OfflineSync.ExternalToCom);

                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.SAVE, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        input[0] = synclist;
                        //testClass = offlineSync;
                        break;
                    }
                case 3: //test save & config in ext to com, with 2 games
                    {

                        offSync.SyncDirection = OfflineSync.ExternalToCom;

                        //create a config file in wc3
                        Game wc3 = PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom);
                        FileStream wc3File = File.Create(wc3.ConfigParentPath + @"\CustomKeys.txt");
                        wc3File.Close();
                        Game fifa = PreCondition.getGame("FIFA 10", OfflineSync.ExternalToCom);

                        Directory.CreateDirectory(fifa.SaveParentPath + @"\A. Profile");
                        FileStream fifaFile = File.Create(fifa.SaveParentPath + @"\A. Profile\SampleSave.save");
                        fifaFile.Close();

                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals("Warcraft 3"))
                                action.MyGame = getGame("Warcraft 3", OfflineSync.ExternalToCom);
                            else if (action.MyGame.Name.Equals("FIFA 10"))
                                action.MyGame = getGame("FIFA 10", OfflineSync.ExternalToCom);
                        }

                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        input[0] = synclist;
                        break;
                    }
                case 4: //test save FIFA in com to ext direction, ext is empty
                    {
                        offSync.SyncDirection = OfflineSync.ComToExternal;
                        //copy a FIFA save game to com
                        Game fifa = getGame("FIFA 10", OfflineSync.ComToExternal);

                        FolderOperation.CopyDirectory(externalPath + @"\FIFA 10\savedGame", fifa.SaveParentPath);
                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals("FIFA 10"))
                                action.MyGame = getGame("FIFA 10", OfflineSync.ComToExternal);
                        }

                        //simulate empty thumb
                        RemoveGamesInExt();
                        testClass = new OfflineSync(OfflineSync.ComToExternal, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        input[0] = synclist;
                        break;
                    }
                case 5: //test config FIFA 10, Warcraft save in com to ext direction
                    {
                        offSync.SyncDirection = OfflineSync.ComToExternal;
                        //copy a FIFA config and wc3 save game
                        Game wc3 = getGame("Warcraft 3", OfflineSync.ComToExternal);
                        Game fifa = getGame("FIFA 10", OfflineSync.ComToExternal);
                        FolderOperation.CopyDirectory(externalPath + @".\Warcraft 3\savedGame\", wc3.SaveParentPath);
                        FolderOperation.CopyDirectory(externalPath + @".\FIFA 10\config", fifa.ConfigParentPath);
                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals("Warcraft 3"))
                                action.MyGame = getGame("Warcraft 3", OfflineSync.ComToExternal);
                            else if (action.MyGame.Name.Equals("FIFA 10"))
                                action.MyGame = getGame("FIFA 10", OfflineSync.ComToExternal);
                        }
                        //simulate empty thumb
                        RemoveGamesInExt();
                        testClass = new OfflineSync(OfflineSync.ComToExternal, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        input[0] = synclist;
                        break;
                    }
                case 6: //test unable to create Backup dir with FIFA 10 config
                    {
                        offSync.SyncDirection = OfflineSync.ExternalToCom;

                        FolderOperation.AddFileSecurity(getGame("FIFA 10", OfflineSync.ExternalToCom).ConfigParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        List<SyncAction> wc3Only = new List<SyncAction>();
                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom), SyncAction.ConfigFiles));
                        FolderOperation.CopyOriginalSettings(wc3Only, FolderOperation.CONFIG, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 7: //test no action
                    {
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 8: //test locking one of the the file in external (FIFA 2010 save in external : A. Profiles)
                    {
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10\savedGame\A. Profiles", user,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        List<SyncAction> wc3Only = new List<SyncAction>();

                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom), SyncAction.AllFiles));
                        FolderOperation.CopyOriginalSettings(wc3Only, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 9: //test for both side 
                    {
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        string username = WindowsIdentity.GetCurrent().Name;
                        FolderOperation.AddFileSecurity(externalPath + @"\FIFA 10", username,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        fifaSavePath = getGame("FIFA 10", OfflineSync.ExternalToCom).SaveParentPath;
                        FolderOperation.AddFileSecurity(fifaSavePath, username,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        //FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 10: //lock FM 2010 save path to unable to create directory
                    {
                        Game fm = PreCondition.getGame("Football Manager 2010", OfflineSync.ExternalToCom);

                        //create a test file in game save folder 
                        FileStream file = File.Create(fm.SaveParentPath + @"\games\FM2010Test.txt");
                        file.Close();

                        FolderOperation.AddFileSecurity(fm.SaveParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));

                        break;
                    }
                case 11:
                    {
                        Game fm = PreCondition.getGame("Football Manager 2010", OfflineSync.ExternalToCom);

                        //create a test file in game save folder 
                        FileStream file = File.Create(fm.SaveParentPath + @"\games\FM2010Test.txt");
                        file.Close();
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.SAVE, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                default: break;
            }
        }

        private static void RemoveGamesInExt()
        {
            string[] dir = Directory.GetDirectories(externalPath);
            foreach (string folder in dir)
                Directory.Move(folder, folder + "-test");
        }

        private static void CreateGATestBackup(List<Game> restoreGame)
        {
            foreach (Game g in restoreGame)
            {
                //if config backup exist, copy the backup for later verifying
                if (Directory.Exists(Path.Combine(g.ConfigParentPath, "GA-configBackup")))
                {
                    Directory.CreateDirectory(Path.Combine(g.ConfigParentPath, "ConfigTestBackup"));
                    FolderOperation.CopyDirectory(Path.Combine(g.ConfigParentPath, "GA-configBackup"),
                                                    Path.Combine(g.ConfigParentPath, "ConfigTestBackup"));
                }
                //if save backup exist, copy the backup for later verifying
                if (Directory.Exists(Path.Combine(g.SaveParentPath, "GA-savedGameBackup")))
                {
                    Directory.CreateDirectory(Path.Combine(g.SaveParentPath, "SaveTestBackup"));
                    FolderOperation.CopyDirectory(Path.Combine(g.SaveParentPath, "GA-savedGameBackup"), Path.Combine(g.SaveParentPath, "SaveTestBackup"));
                }
            }

        }

        /// <summary>
        /// Decode the string and return Array
        /// Create the array object from the param in the string given
        /// format : Array<Type>:{obj1,obj2}
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object[] GetArray(string param, int direction)
        {
            //get the type
            int s, e;
            s = param.IndexOf("<");
            e = param.IndexOf(">");
            string type = param.Substring(s + 1, e - (s + 1));

            //get the list of objects to be added
            string temp = param.Substring(9 + type.Length, param.Length - (9 + type.Length + 1));
            string[] list = temp.Split(new Char[] { '+' });

            switch (type)
            {
                case "String":
                    string[] returnString = (string[])list;
                    return returnString;
                case "Integer":
                    object[] returnObject = new object[list.Length];
                    for (int j = 0; j < list.Length; ++j)
                        returnObject[j] = (int)Convert.ToInt32(list[j]);
                    return returnObject;
                case "Game":
                    //format follow constructor parameters
                    //List<Game>:{game1,game2}
                    Game[] gameArr = new Game[list.Length];
                    int i = 0;
                    foreach (string game in list)
                    {
                        gameArr[i] = PreCondition.getGame(game,direction);
                        ++i;
                    }
                    return gameArr;
                //format = Array<Game>:{(constructor info),(L4D2,c:\prog\L4D2)}
                case "FileInfo":
                    FileInfo[] returnFileInfo = new FileInfo[list.Length];
                    for (int j = 0; j < list.Length; ++j)
                        returnFileInfo[j] = new FileInfo(list[j].Substring(1, list[j].Length - 2));
                    return returnFileInfo;
                default: return null;
            }
        }

        /// <summary>
        /// Decode the string and return Array
        /// Create the ArrayList object from the param in string given
        /// parameter List:{obj1,obj2}
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ArrayList GetArrayList(string param, int direction)
        {
            ArrayList arrlist = null;
            //get the type
            int start, end;
            start = param.IndexOf("<");
            end = param.IndexOf(">");
            string type = param.Substring(start + 1, end - (start + 1));
            string temp = param.Substring(end + 3, param.Length - (end + 4));
            string[] list = temp.Split(new Char[] { '+' });
            switch (type)
            {
                case "String":
                    arrlist = new ArrayList(list);
                    break;
                case "Integer":
                    int[] arr = new int[list.Length];
                    for (int j = 0; j < list.Length; ++j)
                        arr[j] = Convert.ToInt32(list[j]);
                    arrlist = new ArrayList(arr);
                    break;
                case "Game":
                    //format follow constructor parameters
                    //List<Game>:{constructor info,L4D2;c:\prog\L4D2}
                    Game[] gameArr = new Game[list.Length];
                    int i = 0;
                    foreach (string s in list)
                    {
                        string[] game = s.Split(new Char[] {'#'});
                        gameArr[i] = PreCondition.getGame(game[0],Convert.ToInt32(game[1]));
                        ++i;
                    }
                    arrlist = new ArrayList(gameArr);
                    break;
                case "FileInfo":
                    FileInfo[] fileArr = new FileInfo[list.Length];
                    for (int j = 0; j < list.Length; ++j)
                        fileArr[j] = new FileInfo(list[j].Substring(1, list[j].Length - 2));
                    arrlist = new ArrayList(fileArr);
                    break;

                case "SyncAction":
                    if (list[0].Equals("void")) //empty list
                    {
                        arrlist = new ArrayList();
                    }
                    else
                    {
                        Game tempGame = null;
                        SyncAction[] syncList = new SyncAction[list.Length];
                        int j = 0;
                        foreach (string gameInfo in list)
                        {
                            string[] game = gameInfo.Split(new Char[] {';'});
                            string[] newGame = game[0].Split(new Char[] {'#'});
                            tempGame = getGame(newGame[0],Convert.ToInt32(newGame[1]));
                            syncList[j] = new SyncAction(tempGame, Convert.ToInt32(game[1])/*the sync action*/);
                            
                            ++j;
                        }
                        arrlist = new ArrayList(syncList);
                    }
                    break;
            }
            return arrlist;
        }//end of getArrayList 

        /// <summary>
        /// Helper methods to call private methods of other class and get its return object
        /// </summary>
        /// <param name="className"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static object InvokePrivateMethod(object testClass, string methodName, object[] input)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type classType = null;
            object returnType = null;
            foreach (Type type in assembly.GetTypes())
            {
                // Pick up a class
                if (type.IsClass == true && type.Name.Equals(testClass.GetType().Name))
                    classType = type;
            }
            if (classType != null)
            {
                try
                {
                    returnType = classType.InvokeMember(methodName,
                                                 BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |BindingFlags.InvokeMethod,
                                                 null,
                                                 testClass,
                                                 input);
                }
                catch (Exception err)
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Class Not Found");
            }
            return returnType;
        }

        /// <summary>
        /// Gets the Game with the name specified
        /// return null if game specified is not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Game getGame(string name,int direction)
        {
            //if (!supportedGames.Contains(name)) throw new InvalidDataException("Game specified do not exist.");
            Game game = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type foundType = null;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass && type.Name.Equals("GameLibrary"))
                    foundType = type;
            }
            object testClass = Activator.CreateInstance(foundType, new object[0]);

            GameLibrary library = (GameLibrary)testClass;
            List<Game> games = library.GetGameList(direction);

            foreach (Game g in games)
                if (g.Name.Equals(name))
                    return g;
            return game;
        }

        /// <summary>
        /// Switch on / off nettwork adapter
        /// </summary>
        /// <param name="onOff"></param>
        public static bool ToggleNetworkAdapter(bool onOff)
        {
            Shell32.ShellClass sc = new Shell32.ShellClass();
            Shell32.Folder RootFolder =
            sc.NameSpace(Shell32.ShellSpecialFolderConstants.ssfCONTROLS);
            Shell32.Folder SrcFlder = null;
            string adapter1 = "Local Area Connection";
            string adapter2 = "Wireless Network Connection";//Add in Adapter here
            ShellFolderItem fItem = null;

            foreach (Shell32.FolderItem2 fi in RootFolder.Items())
            {
                if (fi.Name == "Network Connections")
                {
                    SrcFlder = (Shell32.Folder)fi.GetFolder;
                    break;
                }
            }

            if (SrcFlder == null)
                return false;

            foreach (Shell32.FolderItem fi in SrcFlder.Items())
            {
                if (fi.Name == adapter1 || fi.Name == adapter2)
                {
                    fItem = (ShellFolderItem)fi;
                    break;
                }
            }

            if (fItem == null)
                return false; 

            foreach (Shell32.FolderItemVerb fi in fItem.Verbs())
            {
                string tempStat = string.Empty;
                //0 - to disable adapter
                //1 - to enable adapter
                int newState = 1;
                if (onOff)
                    newState = 1;
                else
                    newState = 0;
                switch (newState)
                {
                    case 0:
                        tempStat = "disa&ble";
                        newState = 22;
                        break;
                    case 1:
                        tempStat = "en&able";
                        newState = 0;
                        break;
                }
                
                if (string.Compare(fi.Name, tempStat, true) == 0)
                {
                    //set adapter's state
                    fi.DoIt();
                    
                    return true;
                }
            }
            return false;
        }

    }
}