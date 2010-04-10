using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using GameAnywhere.Data;

namespace GameAnywhere.Process
{
    class OnlineSyncVerifier : Verifier
    {

        /// <summary>
        /// Check the sync and backup for online sync
        /// </summary>
        internal static List<string> CheckOnlineSync(ref TestResult result, SyncAction sa)
        {
            List<string> errList = new List<string>();

            Game game = PreCondition.getGame(sa.MyGame.Name, OfflineSync.ComToExternal);
            //check sync
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                string gameName = sa.MyGame.Name;
                string[] checkFiles = Directory.GetFiles(@".\ConfigSyncTest-" + gameName);
                List<string> gamefiles = new List<string>();

                foreach (string path in game.ConfigPathList)
                {
                    if (Directory.Exists(path))
                    {
                        List<string> subPath = FolderOperation.GetAllFilesName(path);
                        foreach (string s in subPath)
                            gamefiles.Add(Path.GetFileName(s));
                    }
                    else
                        gamefiles.Add(Path.GetFileName(path));
                }
                foreach (string file in checkFiles)
                {
                    if (!gamefiles.Contains(Path.GetFileName(file)))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! " + file + " is not found in the Game folder!");
                    }
                }
            }
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                //check sync
                string gameName = sa.MyGame.Name;
                string[] checkFiles = Directory.GetFiles(@".\SaveSyncTest-" + gameName);
                List<string> gameFiles = new List<string>();

                foreach (string path in game.SavePathList)
                {
                    if (Directory.Exists(path))
                    {
                        List<string> subPath = FolderOperation.GetAllFilesName(path);
                        foreach (string s in subPath)
                            gameFiles.Add(Path.GetFileName(s));
                    }
                    else
                        gameFiles.Add(Path.GetFileName(path));
                }
                foreach (string file in checkFiles)
                {
                    if (!gameFiles.Contains(Path.GetFileName(file)))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! " + file + " is not found in the Game folder!");
                    }
                }
            }
            //check backup
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                if (!Directory.Exists(game.ConfigParentPath + @"\GA-configBackup"))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! Backup for config not created!");
                }
                else
                {
                    errList.AddRange(FolderOperation.FindDifferences(game.ConfigParentPath + @"\ConfigTestBackup", game.ConfigParentPath + @"\GA-configBackup", false));
                }
            }
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                if (!Directory.Exists(game.SaveParentPath + @"\GA-savedGameBackup"))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! Backup for Save not created!");
                }
                else
                {
                    errList.AddRange(FolderOperation.FindDifferences(game.SaveParentPath + @"\SaveTestBackup", game.SaveParentPath + @"\GA-savedGameBackup", false));
                }
            }
            return errList;
        }

        /// <summary>
        /// This method verifies the outcome of each test case for OnlineSync class SynchornizeGames method.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="returnType"></param>
        /// <param name="testClass"></param>
        /// <param name="exceptionThrown"></param>
        /// <param name="expectedOutput"></param>
        /// <returns></returns>
        public static TestResult VerifyOnlineSync(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            OnlineSync onlineClass = (OnlineSync)testClass;

            Storage store = new Storage();
            List<string> listOfGamesOnWeb = new List<string>();
            List<string> listOfWebPath = new List<string>();
            string comToWeb = "TestComToWeb@gmails.com";
            string webToCom = "TestWebToCom@gmails.com";
            switch (index)
            {
                case 1: //verify Wow all files
                    {
                        listOfGamesOnWeb = store.ListFiles(comToWeb);
                        foreach (string s in listOfGamesOnWeb)
                        {
                            if (s.Contains('/'))
                                listOfWebPath.Add(s.Substring(s.IndexOf('/')));
                        }

                        Game wow = PreCondition.getGame(worldOfWarcraft, OnlineSync.ComToWeb);

                        CheckWithGamesOnWeb(ref result, listOfWebPath, wow, SyncAction.AllFiles);
                        break;
                    }
                case 2: //verifies Wow config, FM 2010 save game
                    {
                        listOfGamesOnWeb = store.ListFiles(comToWeb);
                        foreach (string s in listOfGamesOnWeb)
                        {
                            if (s.Contains('/'))
                                listOfWebPath.Add(s.Substring(s.IndexOf('/')));
                        }

                        Game wow = PreCondition.getGame(worldOfWarcraft, OnlineSync.ComToWeb);
                        Game fm2010 = PreCondition.getGame(footballManager, OnlineSync.ComToWeb);
                        //check wow
                        CheckWithGamesOnWeb(ref result, listOfWebPath, wow, SyncAction.ConfigFiles);
                        //check fm
                        CheckWithGamesOnWeb(ref result, listOfWebPath, fm2010, SyncAction.SavedGameFiles);
                        break;
                    }

                case 3: //check able to overwrite
                    {
                        store.DownloadFile(@".\verify.txt", comToWeb + "/Warcraft 3/config/CustomKeys.txt");
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OnlineSync.ComToWeb);
                        bool same = FolderOperation.findFileDifferences(@".\verify.txt", wc3.ConfigParentPath + @"\CustomKeys.txt");
                        if (!same)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! files Sync to Web, content is not the same.");
                        }
                        break;
                    }

                case 4: //verify that save is not uploaded and config is uploaded
                    {
                        try
                        {
                            store.DownloadFile(@".\verify.txt", comToWeb + "/Warcraft 3/config/CustomKeys.txt");
                        }
                        catch (Exception err)
                        {
                            //MessageBox.Show(err.Message+"\n"+err.GetType().Name);
                        }
                        List<SyncAction> returnAction = (List<SyncAction>)returnType;
                        foreach (SyncAction sa in returnAction)
                        {
                            bool found = false;
                            if (sa.MyGame.Name.Equals("Warcraft 3"))
                            {
                                foreach (SyncError syncError in sa.UnsuccessfulSyncFiles)
                                {
                                    if (syncError.FilePath.Equals(@"C:\Warcraft III\Warcraft III\Save"))
                                        found = true;
                                }
                            }
                            if (!found)
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! Error path is not found in unsuccessful file list");
                            }
                        }
                        break;
                    }

                case 5: //verify all files are unsuccessful
                    {
                        if (!returnType.GetType().Equals(typeof(ConnectionFailureException)))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! return is not ConnectionFailureException!");
                        }

                        break;
                    }
                case 6:
                    {
                        listOfGamesOnWeb = store.ListFiles(comToWeb);
                        foreach (string s in listOfGamesOnWeb)
                        {
                            if (s.Contains('/'))
                                listOfWebPath.Add(s.Substring(s.IndexOf('/')));
                        }

                        Game wow = PreCondition.getGame(worldOfWarcraft, OnlineSync.ComToWeb);
                        Game fm2010 = PreCondition.getGame(footballManager, OnlineSync.ComToWeb);
                        Game fifa = PreCondition.getGame(FIFA10GameName, OnlineSync.ComToWeb);
                        Game wc3 = PreCondition.getGame(Warcraft3GameName, OnlineSync.ComToWeb);
                        Game abuseGame = PreCondition.getGame(abuse, OnlineSync.ComToWeb);
                        //check wow
                        CheckWithGamesOnWeb(ref result, listOfWebPath, wow, SyncAction.AllFiles);
                        //check fm
                        CheckWithGamesOnWeb(ref result, listOfWebPath, fm2010, SyncAction.SavedGameFiles);
                        //check fifa 10
                        CheckWithGamesOnWeb(ref result, listOfWebPath, fifa, SyncAction.AllFiles);
                        //check wc3 
                        CheckWithGamesOnWeb(ref result, listOfWebPath, wc3, SyncAction.AllFiles);
                        //check abuse
                        CheckWithGamesOnWeb(ref result, listOfWebPath, abuseGame, SyncAction.SavedGameFiles);


                        break;
                    }
                case 7:
                case 8:
                    {
                        //Verify Wc3 is copied to the game folders and backup is created.
                        List<SyncAction> syncList = (List<SyncAction>)returnType;
                        List<string> errList = new List<string>();
                        foreach (SyncAction sa in syncList)
                        {
                            errList = CheckOnlineSync(ref result, sa);
                        }
                        if (errList.Count != 0)
                        {
                            result.Result = false;
                            foreach (string s in errList)
                                result.AddRemarks(s);
                        }
                        break;
                    }
                case 9:
                    {
                        List<SyncAction> syncList = (List<SyncAction>)returnType;
                        foreach (SyncAction sa in syncList)
                        {
                            //Check Wc3 sync
                            List<string> errList = new List<string>();
                            if (sa.MyGame.Name.Equals("Warcraft 3"))
                            {
                                errList = CheckOnlineSync(ref result, sa);
                            }
                            if (errList.Count != 0)
                            {
                                result.Result = false;
                                foreach (string s in errList)
                                    result.AddRemarks(s);
                            }

                            //check FIFA error list
                            if (sa.MyGame.Name.Equals("FIFA 10"))
                            {
                                if (sa.UnsuccessfulSyncFiles.Count == 0)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Unsuccessful file is not in the unsuccessful sync-list.");
                                    break;
                                }
                                string[] comVerifyFiles = Directory.GetFiles(@".\AllFifaFiles");
                                List<string> unsuccessfulList = new List<string>();
                                foreach (SyncError syncErr in sa.UnsuccessfulSyncFiles)
                                {
                                    unsuccessfulList.Add(Path.GetFileName(syncErr.FilePath));
                                }
                                foreach (string file in comVerifyFiles)
                                {
                                    if (!unsuccessfulList.Contains(Path.GetFileName(file)))
                                    {
                                        result.Result = false;
                                        result.AddRemarks("Failed! " + Path.GetFileName(file) + " not added in the unsuccessful sync-list.");
                                    }
                                }
                            }
                        }
                        break;
                    }

                case 10:
                    {
                        List<SyncAction> syncList = (List<SyncAction>)returnType;
                        foreach (SyncAction sa in syncList)
                        {
                            //Check Wc3 sync
                            List<string> errList = new List<string>();
                            errList = CheckOnlineSync(ref result, sa);
                            if (errList.Count != 0)
                            {
                                result.Result = false;
                                foreach (string s in errList)
                                    result.AddRemarks(s);
                            }
                        }
                        break;
                    }
                default: break;
            }
            return result;
        }
    }
}
