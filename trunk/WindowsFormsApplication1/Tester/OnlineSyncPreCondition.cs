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
    class OnlineSyncPreCondition : PreCondition
    {

        /// <summary>
        /// Sets the pre-condition for the onine synchronization methods
        /// a helper method for SetPreCondition
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        /// <param name="user"></param>
        internal static void SetOnlineSyncPreCondition(int index, ref object[] input, ref object testClass, string user)
        {
            OnlineSync online = (OnlineSync)testClass;
            GameLibrary gameLibrary = new GameLibrary();
            List<SyncAction> synclist = (List<SyncAction>)input[0];
            User newUser = new User();
            string comToWeb = "TestComToWeb@gmails.com";
            string webToCom = "TestWebToCom@gmails.com";
            Storage store = new Storage();


            switch (index)
            {
                case 1: //test normal sync with empty on web, Game: WoW.
                    //copy all sample files of save/config to game folders

                    newUser.Email = comToWeb;
                    testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);

                    break;

                case 2: //test sync 2 games WoW interface and Football Manager save
                    //copy all sample files of save/config to game folders

                    newUser.Email = comToWeb;
                    testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                    break;

                case 3: //put a file on net and overwrite
                    {
                        //copy all sample files of save/config to game folders

                        newUser.Email = comToWeb;
                        testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                        //add a sample save file in the net
                        Game wc3 = getGame(Warcraft3GameName, OnlineSync.ComToWeb);
                        store.UploadFile(@".\readme.txt", comToWeb + "/" + Warcraft3GameName + "/config/" + "CustomKeys.txt");

                    }
                    break;

                case 4: //test a file to be locked on the com, lock wc3\save
                    {
                        //copy all sample files of save/config to game folders

                        newUser.Email = comToWeb;
                        testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                        Game game = getGame("Warcraft 3", OfflineSync.Uninitialize);
                        FolderOperation.AddFileSecurity(game.ConfigParentPath + @"\Save", user, FileSystemRights.FullControl, AccessControlType.Deny);

                        break;
                    }
                case 5:
                    //go to offline mode
                    int i = 0;
                    while (!ToggleNetworkAdapter(false) && i < 100) ++i;
                    if (i == 100) MessageBox.Show("Network cannot be off");
                    Thread.Sleep(1000);

                    newUser.Email = comToWeb;
                    testClass = new OnlineSync(OnlineSync.ComToWeb, gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);

                    break;
                case 6: //Upload all game to Web
                    newUser.Email = comToWeb;
                    testClass = new OnlineSync(OnlineSync.ComToWeb,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                    break;

                //Test Web To Com
                case 7: //empty initial Wc3 game folder
                    {
                        newUser.Email = webToCom;
                        testClass = new OnlineSync(OnlineSync.WebToCom,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                        //copy original game settings
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.WebToCom);
                        //download the games files to check correct games files are sync
                        Game game = getGame("Warcraft 3", OfflineSync.Uninitialize);
                        Directory.CreateDirectory(@".\SaveSyncTest-Warcraft 3");
                        Directory.CreateDirectory(@".\ConfigSyncTest-Warcraft 3");
                        store.DownloadFile(@".\ConfigSyncTest-Warcraft 3\CustomKeys.txt", webToCom +
                                            "/Warcraft 3/config/CustomKeys.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\com.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/com.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\commm.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/commm.txt");

                        break;
                    }
                case 8: //check backup folder store existing files
                    {

                        Game game = PreCondition.getGame("Warcraft 3", OfflineSync.Uninitialize);
                        Directory.CreateDirectory(game.SaveParentPath + @"\Save");
                        File.Copy(@".\SyncFolder\Warcraft 3\savedGame\Save\com.txt", game.SaveParentPath + @"\Save\com.txt", true);
                        File.Copy(@".\SyncFolder\Warcraft 3\config\CustomKeys.txt", game.ConfigParentPath + @"\CustomKeys.txt", true);

                        //set the test class
                        newUser.Email = webToCom;
                        testClass = new OnlineSync(OnlineSync.WebToCom,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);

                        //copy original game settings
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.WebToCom);
                        Directory.CreateDirectory(@".\SaveSyncTest-Warcraft 3");
                        Directory.CreateDirectory(@".\ConfigSyncTest-Warcraft 3");
                        store.DownloadFile(@".\ConfigSyncTest-Warcraft 3\CustomKeys.txt", webToCom +
                                            "/Warcraft 3/config/CustomKeys.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\com.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/com.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\commm.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/commm.txt");
                        break;
                    }
                case 9:
                    {
                        //download warcraft 3 to verify later
                        Directory.CreateDirectory(@".\SaveSyncTest-Warcraft 3");
                        Directory.CreateDirectory(@".\ConfigSyncTest-Warcraft 3");
                        store.DownloadFile(@".\ConfigSyncTest-Warcraft 3\CustomKeys.txt", webToCom +
                                            "/Warcraft 3/config/CustomKeys.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\com.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/com.txt");
                        store.DownloadFile(@".\SaveSyncTest-Warcraft 3\commm.txt", webToCom +
                                            "/Warcraft 3/savedGame/Save/commm.txt");

                        Game fifa = getGame("FIFA 10", OfflineSync.Uninitialize);

                        Storage s3 = new Storage();
                        List<string> fileList = s3.ListFiles("TestWebToCom@gmails.com");
                        Directory.CreateDirectory(@".\AllFifaFiles");
                        //copy all fifa files to com for verification later
                        foreach (string file in fileList)
                        {
                            if (file.Contains("FIFA 10"))
                            {
                                s3.DownloadFile(@".\AllFifaFiles\" + Path.GetFileName(file), file);
                            }
                        }

                        //set the test class
                        newUser.Email = webToCom;
                        testClass = new OnlineSync(OnlineSync.WebToCom,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);
                        //copy original game settings
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.WebToCom);

                        //lock a folder
                        FolderOperation.AddFileSecurity(fifa.SaveParentPath, user, FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        break;
                    }
                case 10: //download all to com, com is empty
                    {
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.WebToCom);

                        List<string> allfilesOnWeb = store.ListFiles(webToCom);

                        List<Game> games = library.GetGameList(0);
                        foreach (Game g in games)
                        {
                            if (!g.ConfigParentPath.Equals(""))
                                Directory.CreateDirectory(@".\ConfigSyncTest-" + g.Name);
                            if (!g.SaveParentPath.Equals(""))
                                Directory.CreateDirectory(@".\SaveSyncTest-" + g.Name);
                        }

                        foreach (string file in allfilesOnWeb)
                        {
                            if (file.Contains(FIFA10GameName))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + FIFA10GameName + file.Substring(file.LastIndexOf('/')), file);
                                if (file.Contains("config"))
                                    store.DownloadFile(@".\ConfigSyncTest-" + FIFA10GameName + file.Substring(file.LastIndexOf('/')), file);
                            }
                            if (file.Contains(Warcraft3GameName))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + Warcraft3GameName + file.Substring(file.LastIndexOf('/')), file);
                                if (file.Contains("config"))
                                    store.DownloadFile(@".\ConfigSyncTest-" + Warcraft3GameName + file.Substring(file.LastIndexOf('/')), file);
                            }
                            if (file.Contains(footballManager))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + footballManager + file.Substring(file.LastIndexOf('/')), file);
                            }
                            if (file.Contains(worldOfWarcraft))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + worldOfWarcraft + file.Substring(file.LastIndexOf('/')), file);
                            }
                            if (file.Contains(abuseGameName))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + abuseGameName + file.Substring(file.LastIndexOf('/')), file);
                            }
                            if (file.Contains(theSims3))
                            {
                                if (file.Contains("savedGame"))
                                    store.DownloadFile(@".\SaveSyncTest-" + theSims3 + file.Substring(file.LastIndexOf('/')), file);
                            }
                        }

                        newUser.Email = webToCom;
                        testClass = new OnlineSync(OnlineSync.WebToCom,
                                               gameLibrary.GetGameList(OfflineSync.Uninitialize), newUser);

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
        internal static void CleanUpOnlineSync(int index, string user)
        {
            Storage store = new Storage();
            string comToWeb = "TestComToWeb@gmails.com";
            string webToCom = "TestWebToCom@gmails.com";
            GameLibrary library = new GameLibrary();
            OfflineSync restore = new OfflineSync(0, library.GetGameList(OfflineSync.Uninitialize));
            switch (index)
            {
                case 1: //create dummy folder back on the web
                case 2:
                case 3:
                    if (File.Exists(@".\verify.txt"))
                        File.Delete(@".\verify.txt");
                    store.DeleteDirectory(comToWeb);
                    restore.Restore();
                    break;
                case 4: //Lock the save folder for wc3
                    FolderOperation.RemoveFileSecurity(@"C:\Warcraft III\Warcraft III\Save", user, FileSystemRights.FullControl, AccessControlType.Deny);
                    store.DeleteDirectory(comToWeb);
                    restore.Restore();
                    break;

                case 5:

                    //resume online
                    int i = 0;
                    while (!ToggleNetworkAdapter(true) && i < 100) ++i;
                    if (i == 100) MessageBox.Show("Network cannot be resumed");
                    else
                        while (!IsConnectedToInternet()) ;
                    break;

                case 7:
                    {
                        DeleteTestBackup();
                        DeleteVerifySyncFolder();
                        OfflineSync off = new OfflineSync(OfflineSync.ComToExternal, library.GetGameList(OfflineSync.Uninitialize));
                        off.Restore();
                        break;
                    }
                case 8:
                    {
                        DeleteTestBackup();
                        DeleteVerifySyncFolder();
                        OfflineSync off = new OfflineSync(OfflineSync.ComToExternal, library.GetGameList(OfflineSync.Uninitialize));
                        off.Restore();
                        Game game = PreCondition.getGame("Warcraft 3", OfflineSync.Uninitialize);
                        if (File.Exists(game.ConfigParentPath + @"\CustomKeys.txt"))
                            File.Delete(game.ConfigParentPath + @"\CustomKeys.txt");
                        if (Directory.Exists(game.SaveParentPath + @"\Save"))
                            Directory.Delete(game.SaveParentPath + @"\Save", true);
                        break;
                    }
                case 9:
                    {
                        Game fifa = getGame("FIFA 10", OfflineSync.Uninitialize);
                        FolderOperation.RemoveFileSecurity(fifa.SaveParentPath, user, FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        DeleteTestBackup();
                        DeleteVerifySyncFolder();
                        Directory.Delete(@".\AllFifaFiles", true);
                        OfflineSync off = new OfflineSync(OfflineSync.ComToExternal, library.GetGameList(OfflineSync.Uninitialize));
                        off.Restore();
                        break;
                    }
                case 10:
                    {
                        DeleteTestBackup();
                        DeleteVerifySyncFolder();
                        OfflineSync off = new OfflineSync(OfflineSync.ComToExternal, library.GetGameList(OfflineSync.Uninitialize));
                        off.Restore();
                        break;
                    }
                default: break;
            }

        }
     }
}
