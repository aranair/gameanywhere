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
    class GameLibraryPreCondition : PreCondition
    {

        internal static void SetGameLibraryPreCondition(int index, string methodName, ref object[] input, ref object testClass)
        {
            if (input.Length == 1) //method for GetGameList(direction)
            {
                try
                {
                    GameLibrary library = new GameLibrary();
                    switch (index)
                    {
                        case 1:
                            break;
                        case 2: //input 2
                            break;
                        case 3: //input 2, test if it return the list which exist in external and computer
                            //renamed Wc3 and FIFA 10,  expect return 3 other game
                            string[] games = Directory.GetDirectories(externalPath);

                            foreach (string g in games)
                            {
                                if (supportedGames.Contains(Path.GetFileName(g)))
                                {
                                    Directory.Move(g, g + "-test"); //rename the folder
                                }
                            }
                            break;
                        case 4: //intput 2: empty SyncFolder
                            //rename to simulate empty
                            string[] extGame = Directory.GetDirectories(externalPath);
                            foreach (string s in extGame)
                                Directory.Move(s, s + "-test"); //rename the folder
                            break;

                        case 5:
                            break;
                        case 6: //test for lock game files
                            {
                                string username = WindowsIdentity.GetCurrent().Name;
                                FolderOperation.AddFileSecurity(externalPath + @"\" + FIFA10GameName, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                Game fifa = getGame(FIFA10GameName, OfflineSync.Uninitialize);
                                fifaSavePath = fifa.SaveParentPath;
                                FolderOperation.AddFileSecurity(fifaSavePath, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                break;
                            }
                        case 7:
                            fifaSavePath = getGame(FIFA10GameName, OfflineSync.Uninitialize).SaveParentPath;
                            Directory.Move(fifaSavePath, fifaSavePath + "-test"); //rename the folder
                            break;
                        case 8:
                            wc3InstallPath = getGame(Warcraft3GameName, OfflineSync.Uninitialize).InstallPath;
                            Directory.Move(wc3InstallPath, wc3InstallPath.Substring(0, wc3InstallPath.Length - 1) + "-test");
                            break;
                        //Initialization test case
                        case 9:
                            {
                                Game fifa = getGame(FIFA10GameName, OfflineSync.Uninitialize);
                                Game wc3 = getGame(Warcraft3GameName, OfflineSync.Uninitialize);
                                Game fm = getGame(footballManager, OfflineSync.Uninitialize);
                                Game wow = getGame(worldOfWarcraft, OfflineSync.Uninitialize);
                                Game abuse = getGame(abuseGameName, OfflineSync.Uninitialize);
                                //Copy FIFA Game files To Com
                                FolderOperation.CopyDirectory(externalPath + @"\FIFA 10\savedGame", fifa.SaveParentPath);
                                FolderOperation.CopyDirectory(externalPath + @"\FIFA 10\config", fifa.ConfigParentPath);

                                //copy Warcraft Game files to com
                                FolderOperation.CopyDirectory(externalPath + @"\Warcraft 3\savedGame", wc3.SaveParentPath);
                                FolderOperation.CopyDirectory(externalPath + @"\Warcraft 3\config", wc3.ConfigParentPath);

                                //copy FM
                                FolderOperation.CopyDirectory(externalPath + @"\Football Manager 2010\savedGame", fm.SaveParentPath);

                                //copy WoW
                                FolderOperation.CopyDirectory(externalPath + @"\World of Warcraft\savedGame", wow.SaveParentPath);

                                //copy Abuse
                                FolderOperation.CopyDirectory(externalPath + @"\Abuse\savedGame", abuse.SaveParentPath);
                                break;
                            }
                        case 10:

                            break;

                        default: break;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.StackTrace.ToString());
                }
            }
            else //method for Web To Com
            {
                try
                {
                    switch (index)
                    {
                        case 11:
                            {
                                break;
                            }
                        default: break;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.StackTrace.ToString());
                }
            }
        }

    }
}
