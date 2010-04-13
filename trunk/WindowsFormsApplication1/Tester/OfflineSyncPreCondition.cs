using System;
using System.Collections.Generic;
using System.Linq;
//Tan Hong Zhou
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
    class OfflineSyncPreCondition : PreCondition
    {

        /// <summary>
        /// Sets the precondition for the offline synchronization methods
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="testClass"></param>
        /// <param name="user"></param>
        internal static void SetOfflineSyncPreCondition(int index, ref object[] input, ref object testClass, string user)
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
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OfflineSync.ExternalToCom);
                        Directory.CreateDirectory(wc3.SaveParentPath + @"\Save");
                        FileStream fs = File.Create(wc3.SaveParentPath + @"\Save\newsave.txt");
                        fs.Close();
                        foreach (SyncAction action in synclist)
                            if (action.MyGame.Name.Equals(Warcraft3GameName))
                                action.MyGame = getGame(Warcraft3GameName, OfflineSync.ExternalToCom);

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
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OfflineSync.ExternalToCom);
                        FileStream wc3File = File.Create(wc3.ConfigParentPath + @"\CustomKeys.txt");
                        wc3File.Close();
                        Game fifa = PreCondition.getGame(FIFA10GameName, OfflineSync.ExternalToCom);

                        Directory.CreateDirectory(fifa.SaveParentPath + @"\A. Profile");
                        FileStream fifaFile = File.Create(fifa.SaveParentPath + @"\A. Profile\SampleSave.save");
                        fifaFile.Close();

                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals(Warcraft3GameName))
                                action.MyGame = getGame(Warcraft3GameName, OfflineSync.ExternalToCom);
                            else if (action.MyGame.Name.Equals(FIFA10GameName))
                                action.MyGame = getGame(FIFA10GameName, OfflineSync.ExternalToCom);
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
                        Game fifa = getGame(FIFA10GameName, OfflineSync.ComToExternal);

                        FolderOperation.CopyDirectory(externalPath + @"\FIFA 10\savedGame", fifa.SaveParentPath);
                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals(FIFA10GameName))
                                action.MyGame = getGame(FIFA10GameName, OfflineSync.ComToExternal);
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
                        Game wc3 = getGame(Warcraft3GameName, OfflineSync.ComToExternal);
                        Game fifa = getGame(FIFA10GameName, OfflineSync.ComToExternal);
                        FolderOperation.CopyDirectory(externalPath + @".\Warcraft 3\savedGame\", wc3.SaveParentPath);
                        FolderOperation.CopyDirectory(externalPath + @".\FIFA 10\config", fifa.ConfigParentPath);
                        foreach (SyncAction action in synclist)
                        {
                            if (action.MyGame.Name.Equals(Warcraft3GameName))
                                action.MyGame = getGame(Warcraft3GameName, OfflineSync.ComToExternal);
                            else if (action.MyGame.Name.Equals(FIFA10GameName))
                                action.MyGame = getGame(FIFA10GameName, OfflineSync.ComToExternal);
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

                        FolderOperation.AddFileSecurity(getGame(FIFA10GameName, OfflineSync.ExternalToCom).ConfigParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        List<SyncAction> wc3Only = new List<SyncAction>();
                        wc3Only.Add(new SyncAction(PreCondition.getGame(Warcraft3GameName, OfflineSync.ExternalToCom), SyncAction.ConfigFiles));
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

                        wc3Only.Add(new SyncAction(PreCondition.getGame(Warcraft3GameName, OfflineSync.ExternalToCom), SyncAction.AllFiles));
                        FolderOperation.CopyOriginalSettings(wc3Only, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 9: //test for both side 
                    {
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        string username = WindowsIdentity.GetCurrent().Name;
                        FolderOperation.AddFileSecurity(externalPath + @"\" + FIFA10GameName, username,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        fifaSavePath = getGame(FIFA10GameName, OfflineSync.ExternalToCom).SaveParentPath;
                        FolderOperation.AddFileSecurity(fifaSavePath, username,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        //FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 10: //lock FM 2010 save path to unable to create directory
                    {
                        Game fm = PreCondition.getGame(footballManager, OfflineSync.ExternalToCom);

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
                        Game fm = PreCondition.getGame(footballManager, OfflineSync.ExternalToCom);

                        //create a test file in game save folder 
                        FileStream file = File.Create(fm.SaveParentPath + @"\games\FM2010Test.txt");
                        file.Close();
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.SAVE, FolderOperation.ExtToCom);
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        break;
                    }
                case 12:
                    {
                        testClass = new OfflineSync(OfflineSync.ExternalToCom, gameLibrary.GetGameList(OfflineSync.Uninitialize));
                        offSync.SyncDirection = OfflineSync.ExternalToCom;
                        FolderOperation.CopyOriginalSettings(synclist, FolderOperation.BOTH, FolderOperation.ExtToCom);
                        break;
                    }

                default: break;
            }
        }

        /// <summary>
        /// This method does the "cleaning" up of the Offline Sync action executed for each test case
        /// </summary>
        /// <param name="index"></param>
        /// <param name="user"></param>
        internal static void CleanUpOffLineSync(int index, string user)
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
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OfflineSync.ExternalToCom);
                        File.Delete(wc3.ConfigParentPath + @"\CustomKeys.txt");
                        Game fifa = PreCondition.getGame(FIFA10GameName, OfflineSync.ExternalToCom);
                        Directory.Delete(fifa.SaveParentPath + @"\A. Profiles", true);
                        break;
                    }
                case 4:
                    {
                        DeleteTestBackup();
                        //rename back the games file
                        RestoreTestFolderName();
                        //delete the copied save files
                        Game fifa = PreCondition.getGame(FIFA10GameName, OfflineSync.ComToExternal);
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
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OfflineSync.ComToExternal);
                        Game fifa = PreCondition.getGame(FIFA10GameName, OfflineSync.ComToExternal);
                        Directory.Delete(wc3.SaveParentPath + @"\Save", true);
                        Directory.Delete(fifa.ConfigParentPath + @"\User", true);
                        break;
                    }
                case 6:
                    {
                        DeleteTestBackup();
                        //resume the access of the foler
                        FolderOperation.RemoveFileSecurity(PreCondition.getGame(FIFA10GameName, OfflineSync.ExternalToCom).ConfigParentPath, user,
                            FileSystemRights.CreateDirectories, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(PreCondition.getGame(FIFA10GameName, OfflineSync.ExternalToCom).ConfigParentPath, user,
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
                        FolderOperation.RemoveFileSecurity(externalPath + @"\" + FIFA10GameName, user,
                            FileSystemRights.FullControl, AccessControlType.Deny);
                        FolderOperation.AddFileSecurity(externalPath + @"\" + FIFA10GameName, user,
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
                        Game fm = PreCondition.getGame(footballManager, OfflineSync.ExternalToCom);
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
                    {
                        Game fm = getGame("Football Manager 2010", OfflineSync.Uninitialize);
                        File.Delete(fm.SaveParentPath + @"\games\FM2010Test.txt");
                        break;
                    }
                case 12:
                    DeleteTestBackup();
                    break;
                default: break;
            }
        }
    }
}
