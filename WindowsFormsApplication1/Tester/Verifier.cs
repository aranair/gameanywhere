using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;


namespace GameAnywhere
{
    /// <summary>
    /// Verifier class contains all the methods required to verified 
    /// for all the test cases for each of the classes
    /// 
    /// 
    /// </summary>
    class Verifier
    {

        private static string externalPath = @".\SyncFolder";
        #region OnlineSync
        private static List<string> supportedGames = new List<string>(new string[] {"Warcraft 3","FIFA 10","Football Manager 2010","World of Warcraft"});

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

            switch (index)
            {
                case 1: //verify Wow all files
                    {
                        listOfGamesOnWeb = store.ListFiles("TestComToWeb@gmails.com");
                        foreach (string s in listOfGamesOnWeb)
                        {
                            if(s.Contains('/'))
                                listOfWebPath.Add(s.Substring(s.IndexOf('/')));
                        }

                        Game wow = PreCondition.getGame("World of Warcraft", OnlineSync.ComToWeb);

                        CheckWithGamesOnWeb(ref result, listOfWebPath, wow);
                        break;
                    }
                case 2: //verifies Wow config, FM 2010 save game
                    {
                        listOfGamesOnWeb = store.ListFiles("TestComToWeb@gmails.com");
                        foreach (string s in listOfGamesOnWeb)
                            listOfWebPath.Add(s.Substring(s.IndexOf('/')));

                        Game wow = PreCondition.getGame("World Of Warcraft", OnlineSync.ComToWeb);
                        Game fm2010 = PreCondition.getGame("Football Manager 2010",OnlineSync.ComToWeb);
                        //check wow
                        CheckWithGamesOnWeb(ref result, listOfWebPath, wow);
                        //check fm
                        CheckWithGamesOnWeb(ref result, listOfWebPath, fm2010);
                        break;
                    }
                case 3: // check syncAction list for the list of error
                    {
                        Game fifa = PreCondition.getGame("FIFA 10",OnlineSync.ComToWeb);
                        //check for all the paths to be sync-ed, they are in the sync error list in the sync Action.
                        List<SyncAction> ret = (List<SyncAction>)returnType;
                        SyncAction fifaSyncAction = null;
                        foreach(SyncAction sa in ret)
                        {
                            if(sa.MyGame.Name.Equals("FIFA 10"))
                                fifaSyncAction = sa;
                        }
                        List<string> errorPath = new List<string>();
                        foreach(SyncError se in fifaSyncAction.UnsuccessfulSyncFiles)
                        {
                            errorPath.Add(se.FilePath);
                        }
                        //check config path
                        foreach (string path in fifa.ConfigPathList)
                        {
                            //if error path do not contain the config file
                            if (!errorPath.Contains(path))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! "+path+" is not in Sync Error list.");
                            }
                        }

                        //
                        foreach (string path in fifa.SavePathList)
                        {
                            //if error path do not contain the config file
                            if (!errorPath.Contains(path))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! " + path + " is not in Sync Error list.");
                            }
                        }
                        break;
                    }
                        default : break;
            }
            return result;
        }

        private static void CheckWithGamesOnWeb(ref TestResult result, List<string> listOfWebPath, Game game)
        {
            //Check save for Wow
            foreach (string com in game.SavePathList)
            {
                string check = com.Substring(game.SaveParentPath.Length);
                MessageBox.Show("check : " + check + " com : " + com);
                if (!listOfWebPath.Contains(check))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! " + com + " is not uploaded to the web");
                }
            }

            //Check config for Wow
            foreach (string com in game.ConfigPathList)
            {
                string check = com.Substring(game.ConfigParentPath.Length);
                MessageBox.Show("check : " + check + " com : " + com);
                if (!listOfWebPath.Contains(check))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! " + com + " is not uploaded to the web");
                }
            }
        }
        #endregion

        # region WebAndThumbSync
        /// <summary>
        /// This method verifies outcome for the check conflict method in the WebAndThumbSync
        /// </summary>
        /// <param name="index"></param>
        /// <param name="returnType"></param>
        /// <param name="testClass"></param>
        /// <param name="exceptionThrown"></param>
        /// <param name="expectedOutput"></param>
        /// <returns></returns>
        public static TestResult VerifyCheckConflicts(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            WebAndThumbSync webThumb = (WebAndThumbSync)testClass;
            Dictionary<string, int> conflicts = new Dictionary<string, int>();
            if(returnType != null)
                conflicts = (Dictionary<string,int>) returnType;

            switch (index)
            {
                
                case 1:
                    {
                        
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : "+expectedOutput+" entries in return, But "+conflicts.Count+" is return.");
                        }
                        
                        if(webThumb.Conflicts.Count != 0 && webThumb.NoConflict.Count!= 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts and noConflicts is not 0!");
                        }
                        break;
                    }
                case 2:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts is not 0!");
                        }
                        if (webThumb.NoConflict.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 2);
                        if(!webThumb.NoConflict.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contains Game1/savedGame/File 1 and direction 2");
                        }
                        break;
                    }
                case 3:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts is not 0!");
                        }
                        if (webThumb.NoConflict.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 1);
                        if (!webThumb.NoConflict.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game1/savedGame/File 1 and direction 1");
                        }
                        break;
                    }
                case 4:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts is not 0!");
                        }
                        if (webThumb.NoConflict.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 1);
                        if (!webThumb.NoConflict.Contains(f2))                 
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contains Game2/savedGame/File 2 and direction 1");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 2);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction 2");
                        }
                        break;
                    }
                case 5:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        if (webThumb.NoConflict.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 1);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction 1");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 2);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction 2");
                        }
                        break;
                    }
                case 6:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        if (webThumb.NoConflict.Count != 3)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 3 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 1);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction 1");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 2);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains File 3 and direction 2");
                        }
                        KeyValuePair<string, int> f4 = new KeyValuePair<string, int>("Game4/savedGame/File 4", 3);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction 3");
                        }
                        break;
                    }
                case 7:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        if (webThumb.NoConflict.Count != 3)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 3 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 1);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction 1");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 2);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction 2");
                        }
                        KeyValuePair<string, int> f4 = new KeyValuePair<string, int>("Game4/savedGame/File 4", 4);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction 4");
                        }
                        break;
                    }
                case 8:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        if (webThumb.NoConflict.Count != 5)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 5 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 1);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction 1");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 2);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction 2");
                        }
                        KeyValuePair<string, int> f4 = new KeyValuePair<string, int>("Game4/savedGame/File 4", 3);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction 3");
                        }
                        KeyValuePair<string, int> f5 = new KeyValuePair<string, int>("Game5/savedGame/File 5", 4);
                        if (!webThumb.NoConflict.Contains(f5))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game5/savedGame/File 5 and direction 4");
                        }
                        KeyValuePair<string, int> f6 = new KeyValuePair<string, int>("Game6/savedGame/File 6", 1);
                        if (!webThumb.NoConflict.Contains(f6))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game6/savedGame/File 6 and direction 1");
                        }
                        break;
                    }
                case 9:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        break;
                    }
                case 10:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 12!");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 2);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game2/savedGame/File 2 and 2!");
                        }
                        break;
                    }
                case 11:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }
                        if(webThumb.Conflicts.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 0!");
                        }
                        KeyValuePair<string, int> f4 = new KeyValuePair<string, int>("Game4/savedGame/File 4", 12);
                        if (!webThumb.Conflicts.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game4/savedGame/File 4 and 0!");
                        }
                        if (webThumb.NoConflict.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game2/savedGame/File 2", 2);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts do not contain Game2/savedGame/File 2 and 2!");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game3/savedGame/File 3", 1);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts do not contain Game3/savedGame/File 3 and 1!");
                        }
                        break;
                    }
                case 12:
                    {
                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }

                        if (webThumb.Conflicts.Count != 0 || webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts and noConflicts is not 0!");
                        }
                        if (webThumb.LocalHash.DeleteEntry("Game1/savedGame/File 1") || webThumb.LocalMeta.DeleteEntry("Game1/savedGame/File 1") ||
                            webThumb.WebHash.DeleteEntry("Game1/savedGame/File 1") || webThumb.WebMeta.DeleteEntry("Game1/savedGame/File 1"))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Game1/savedGame/File 1 should not exist in Meta-Data ");
                        }
                        break;
                    }

                case 13:
                    {
                        if (webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict should contains 0 entry.");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflict should contain only 1 entry.");
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 13);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 1 and 13");
                        }
                        break;
                    }
                case 14:
                    {
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 24);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflict did not contain Game1/savedGame/File 1 and 24");
                        }
                        if (webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict should contain 0 entry, current: " + webThumb.NoConflict.Count);
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts should contain 1 entry, current : " + webThumb.Conflicts.Count);
                        }
                        if (returnType != null)
                        {
                            Dictionary<string, int> conflictReturn = (Dictionary<string, int>)returnType;
                            KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game1/savedGame", 0);
                            if (!conflictReturn.Contains(f2))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! return did not contain Game1/savedGame");
                            }
                        }
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Return is null. Unable to verify return value.");
                        }
                        break;
                    }
                case 15:
                    {
                        if (webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict expected " + 0 + " current :" + webThumb.NoConflict.Count);
                        }
                        if (webThumb.Conflicts.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflict expect " + 2 + " current :" + webThumb.Conflicts.Count);
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 1 and 12");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game1/savedGame/File 2", 24);
                        if (!webThumb.Conflicts.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 2 and 24");
                        }
                        //check for return
                        if (returnType != null)
                        {
                            Dictionary<string, int> conflictReturn = (Dictionary<string, int>)returnType;
                            if (conflictReturn.Count != 1)
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! return expects 1, returned :" + conflictReturn.Count);
                            }
                            KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game1/savedGame", 0);
                            if (!conflictReturn.Contains(f3))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! return did not contain Game1/savedGame");
                            }
                        }
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Return Type null, unable to verify return value");
                        }
                    }
                    break;
                case 16:
                    {
                        if (webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict expect 0, current :" + webThumb.NoConflict.Count);
                        }
                        if (webThumb.Conflicts.Count != 4)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect 4, current:" + webThumb.Conflicts.Count);
                        }
                        KeyValuePair<string, int> f1 = new KeyValuePair<string, int>("Game1/savedGame/File 1", 12);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/savedGame/File 1 and 12");
                        }
                        KeyValuePair<string, int> f2 = new KeyValuePair<string, int>("Game1/savedGame/File 2", 24);
                        if (!webThumb.Conflicts.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/savedGame/File 2 and 24");
                        }
                        KeyValuePair<string, int> f3 = new KeyValuePair<string, int>("Game1/config/File 3", 13);
                        if (!webThumb.Conflicts.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/config/File 3 and 13");
                        }
                        KeyValuePair<string, int> f4 = new KeyValuePair<string, int>("Game2/config/File 4", 12);
                        if (!webThumb.Conflicts.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game2/config/File 4 and 12");
                        }
                        //check return
                        if(returnType != null)
                        {
                            Dictionary<string, int> conflictReturn = (Dictionary<string, int>)returnType;
                            KeyValuePair<string, int> f5 = new KeyValuePair<string, int>("Game1/savedGame", 0);
                            if (!conflictReturn.Contains(f5))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! conflict expect Game1/savedGame and 0");
                            }
                            KeyValuePair<string, int> f6 = new KeyValuePair<string, int>("Game1/config", 0);
                            if (!conflictReturn.Contains(f6))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! conflict expect Game1/config and 0");
                            }
                            KeyValuePair<string, int> f7 = new KeyValuePair<string, int>("Game2/config", 0);
                            if (!conflictReturn.Contains(f7))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! conflict expect Game2/config and 0");
                            }
                        }
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! return is null. Unable to verify return value");
                        }
                        break;
                    }
                default: break;
                
            }
            webThumb.Conflicts.Clear();
            webThumb.NoConflict.Clear();
            return result;
        }
        #endregion 

        #region User class
        #region ResendActivation method
        public static TestResult VerifyResendActivation(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                case 3: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }

            return result;
        }
        #endregion

        #region ChangePassword method

        public static TestResult VerifyChangePassword(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                case 3:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }

                case 4: //check exception ArugmentException
                    {
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 5: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }

            return result;
        }
        #endregion

        # region Verify Register methods

        public static TestResult VerifyRegisterRetrievePassword(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                case 3:
                    {
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 4:
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }
            return result;
        }

        #endregion

        # region Verify Login methods

        public static TestResult VerifyLogin(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                case 3:
                    //check expeceted return type
                    {
                        bool outcome = (bool)returnType;
                        bool expected = (bool)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                
                case 4: //check exception ArugmentException
                    {   
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 5: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }

            return result;
        }

        #endregion
        #endregion

        #region GameLibrary Class
        #region Verify GetGameList method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="testClass"></param>
        /// <param name="returnType"></param>
        /// <param name="passed"></param>
        /// <param name="result"></param>
        public static TestResult VerifyGetGameList(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {  
            TestResult result = new TestResult(true);
            GameLibrary library = (GameLibrary)testClass;
            List<Game> libraryList = library.InstalledGameList;
            // choose test case index
            switch (index)
            {
                case 1:
                    
                    List<Game> glist = library.InstalledGameList;
                    //ensure FIFA 2010 and warcraft 3 are in the list
                    if (glist.Count != (int)expectedOutput)
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! Game Library game list is not properly initialized");
                    }
                    else
                        result.AddRemarks("Passed!");
                    Game warcraft = null;
                    foreach (Game g in libraryList)
                    {
                        if (g.Name.Equals("Warcraft 3"))
                        {
                            warcraft = g;
                        }
                    }
                    break;
                case 2: //return list must be same as the gamelibrary list
                    {
                        List<Game> gameList = (List<Game>)returnType;
                        if (gameList.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Returned: " + gameList.Count + ". Expecting only " + (int)expectedOutput + " games in the return list");
                        }
                        List<string> gamesInLib = new List<string>();
                        List<string> gamesInReturn = new List<string>();
                        foreach (Game g in libraryList)
                            gamesInLib.Add(g.Name);
                        foreach (Game g in gameList)
                            gamesInReturn.Add(g.Name);
                        List<string> notFoundGame = new List<string>();

                        bool passed = true;
                        foreach (string g in gamesInLib) // verify all games in gamelibrary is in the return list
                        {
                            if (!gamesInReturn.Contains(g))
                            {
                                passed = false;
                                result.Result = false;
                                notFoundGame.Add(g);
                            }
                        }
                        result.ReturnType = returnType;
                        if (!passed)
                        {
                            result.AddRemarks("Expected output failed!");
                            foreach (string s in notFoundGame)
                                result.AddRemarks("\tProblem: " + s +" ,is missing in the return list");
                        }
                        else
                            result.AddRemarks("Expected output passed!");
                    }
                    break;
                case 3: //return list must be found in external
                    {
                        List<Game> gameList = (List<Game>)returnType;
                        List<string> gamesInReturn = new List<string>();
                        List<string> gamesInLib = new List<string>();
                        foreach (Game g in libraryList)
                            gamesInLib.Add(g.Name);
                        List<string> gameInReturn3 = new List<string>();
                        foreach (Game g in gameList)
                            gamesInReturn.Add(g.Name);
                        if (gameList.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Returned: " + gameList.Count + ". Expecting only " + (int)expectedOutput + " games in the return list");
                        }

                        List<string> missingGame = new List<string>();
                        List<string> extraGame = new List<string>();
                        foreach (Game g in gameList) //verify all games in return list is in the external
                        {
                            if (!Directory.Exists(Path.Combine(externalPath, g.Name))) // External do not contain the game in return list
                            {
                                result.Result = false;
                                extraGame.Add(g.Name); //extra game in return list
                            }
                        }

                        foreach (string g in gamesInReturn) //verify all games in return list is in the computer
                        {
                            if (!gamesInLib.Contains(g)) //return list do not contain the game in the external
                            {
                                result.Result = false;
                                missingGame.Add(g); //game not installed in computer
                            }
                        }
                        result.ReturnType = returnType;
                        if (!result.Result)
                        {
                            result.AddRemarks("Expected output failed!");
                            foreach (string s in missingGame)
                                result.AddRemarks("\tProblem: " + s +" ,is missing in the Return List");
                            foreach (string s in extraGame)
                                result.AddRemarks("\tProblem: " + s +" ,is not found in the External");
                        }
                        else
                            result.AddRemarks("Passed!");

                        
                    }
                    break;
                case 4:
                    List<Game> gameList4 = (List<Game>)returnType;
                    if (gameList4.Count != 0)
                    {
                        result.AddRemarks("Expected output failed! \n\tExpected: " + expectedOutput + " ,Output: " + gameList4.Count);
                    }
                    else
                        result.AddRemarks("Passed!");


                    break;
                case 5:
                    if (returnType.GetType().Equals(typeof(Exception)))
                    {
                        Exception err = (Exception)returnType;
                        if (!err.GetBaseException().GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                        }
                        if (!result.Result)
                        {
                            result.AddRemarks("Expected output failed!");
                            result.AddRemarks("Problem: " + err.GetBaseException().GetType().Name +
                                " ,Exception caught differs from expected - " + exceptionThrown);
                        }
                        else
                            result.AddRemarks("Expected output passed!");
                    }
                    else
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! No Exception was thrown!");
                    }
                    
                    result.ReturnType = returnType;
                    
                    break;
                case 6:
                case 7:
                case 8:
                     //expect only Wc3 to be returned
                    {
                        List<Game> gameList = (List<Game>)returnType;
                        if (gameList.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Returned: " + gameList.Count + ". Expecting only " + (int)expectedOutput + " games in the return list");
                        }
                        List<string> gamesInLib = new List<string>();
                        List<string> gamesInReturn = new List<string>();
                        foreach (Game g in libraryList)
                            gamesInLib.Add(g.Name);
                        foreach (Game g in gameList)
                            gamesInReturn.Add(g.Name);
                        List<string> notFoundGame = new List<string>();

                        bool passed = true;
                        foreach (string g in gamesInLib) // verify all games in gamelibrary is in the return list
                        {
                            if (g.Equals("FIFA 10") && index == 6) 
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! FIFA 10 not supposed to be in the retun list");
                                continue;
                            }
                            if (!gamesInReturn.Contains(g))
                            {
                                passed = false;
                                result.Result = false;
                                notFoundGame.Add(g);
                            }
                        }
                        result.ReturnType = returnType;
                        if (!passed)
                        {
                            result.AddRemarks("Expected output failed!");
                            foreach (string s in notFoundGame)
                                result.AddRemarks("\tProblem: " + s + " ,is missing in the return list");
                        }
                        else
                            result.AddRemarks("Expected output passed!");
                        break;
                    }               
                default: break;
            } //end of GetGameList Test cases

            return result;
        }


#endregion
        #endregion

        #region OfflineSync
        #region Verify Synchronization method
        /// <summary>
        /// This method does the verification of post-condition for SynchronizaeGame method
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="passed"></param>
        /// <param name="result"></param>
        public static TestResult VerifySynchronizeGames(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            List<SyncAction> syncList = new List<SyncAction>();
            TestResult result = new TestResult(true);
            if (returnType.GetType().Equals(typeof(List<SyncAction>)))
            {
                syncList = (List<SyncAction>)returnType;
            }
            switch (index)
            {
                //case 1 - 3: ext to com
                case 1: //verify config only  FIFA 10 
 
                    CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, true);
                    CheckBackupAndSync(syncList, ref result, true);                 
                    break;
                case 2://verify save only  WC3
                    CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, true);                  
                    CheckBackupAndSync(syncList, ref result, true);
                    break;
                case 3: //verify both game, ALL type
                    CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, true);
                    CheckBackupAndSync(syncList, ref result, true);
                    break;
                    //case 4 - 6: com to ext
                case 4: // FIFA 10 save only
                    {
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, true);
                        CheckBackupAndSync(syncList, ref result, false);
                        
                        break;
                    }
                case 5: //WC3 save, FIFA config only
                    {
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, true);
                        CheckBackupAndSync(syncList, ref result, false);
                        
                        break;
                    }
                case 6: // Verify that only Wc3 is sync
                    {
                        List<SyncAction> wc3Only = new List<SyncAction>();
                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3",OfflineSync.Uninitialize), SyncAction.ConfigFiles));
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, wc3Only,true);
                        
                        //passed in synclist for WC3
                        CheckBackupAndSync(wc3Only, ref result, true);
                        //check FIFA save files are in the syncError List
                      
                        foreach (SyncAction action in syncList)
                        {
                            if (action.MyGame.Name.Equals("FIFA 10"))
                            {
                                //make sure they are in SyncError list
                                List<string> invalid = new List<string>();
                                string[] saveFiles = Directory.GetDirectories(externalPath + @"\FIFA 10\savedGame");
                                foreach (string s in saveFiles)
                                {
                                    invalid.Add(Path.GetFileName(s));
                                }
                                invalid.Add("savedGame");
                                invalid.Add("GA-savedGameBackup");
                                foreach(SyncError error in action.UnsuccessfulSyncFiles)
                                {
                                    if (Path.GetFileName(error.FilePath).Equals("SaveTestBackup")
                                     || Path.GetFileName(error.FilePath).Equals("ConfigTestBackup"))
                                        continue;
                                    if (!invalid.Contains(Path.GetFileName(error.FilePath)))
                                    {
                                        result.Result = false;
                                        result.AddRemarks("Failed! ErrorList: "+error.FilePath + " is not reflected in the SyncError");
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 7: //test for no action 
                    {
                        CheckSimilarities(ref result, syncList);
                        break;
                    }
                case 8:
                    {
                        List<SyncAction> wc3Only = new List<SyncAction>();
                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3",OfflineSync.ExternalToCom), SyncAction.AllFiles));

                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, wc3Only, true);

                        //passed in synclist for WC3
                        CheckBackupAndSync(wc3Only, ref result, true);
                        
                        //check FIFA save files are in the syncError List

                        foreach (SyncAction action in syncList)
                        {
                            if (action.MyGame.Name.Equals("FIFA 10"))
                            {

                                string[] extFiles = Directory.GetDirectories(externalPath + @"\FIFA 10\savedGame");
                                string[] comFiles = Directory.GetDirectories(action.MyGame.SaveParentPath);
                                List<string> comList = new List<string>();
                                foreach (string s in comFiles)
                                {
                                    comList.Add(Path.GetFileName(s));
                                }
                                foreach (string name in extFiles)
                                {
                                    if (comList.Contains(Path.GetFileName(name)))
                                    {
                                        result.Result = false;
                                        result.AddRemarks("Failed! "+name+" is should not be copied over!");
                                    }
                                }
                                if (action.UnsuccessfulSyncFiles.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! denied file not in unsuccessful list");
                                }
                            }
                        }
                        break;
                    }
                case 9:
                    {

                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList,false);

                        //check only sync
                        //CheckBackupAndSync(syncList, ref result, false);

                        //check FIFA save files are in the syncError List

                        foreach (SyncAction action in syncList)
                        {
                            if (action.MyGame.Name.Equals("FIFA 10"))
                            {
                                //make sure they are in SyncError list
                                List<string> invalid = new List<string>();
                                string[] saveFiles = Directory.GetDirectories(externalPath + @"\FIFA 10\savedGame");
                                string[] configFiles = Directory.GetDirectories(externalPath+@"\FIFA 10\config");
                                foreach (string s in saveFiles)
                                    invalid.Add(Path.GetFileName(s));
                                foreach (string s in configFiles)
                                    invalid.Add(Path.GetFileName(s));
                                invalid.Add("GA-configBackup");
                                invalid.Add("GA-savedGameBackup");
                                foreach (SyncError error in action.UnsuccessfulSyncFiles)
                                {
                                    if (!invalid.Contains(Path.GetFileName(error.FilePath)))
                                    {
                                        result.Result = false;
                                        result.AddRemarks(error.FilePath + " is not reflected in the SyncError");
                                    }
                                }

                            }
                        }
                        break;
                    }
                case 10:
                    {
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, false);

                        foreach (SyncAction action in syncList)
                        {
                            if (action.MyGame.Name.Equals("Football Manager 2010"))
                            {
                                string verifyPath = action.MyGame.SaveParentPath + @"\games\config.xml";
                                if (File.Exists(verifyPath))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! save game from external should not copy over.");
                                }
                                verifyPath = action.MyGame.SaveParentPath + @"\games\FM2010Test.txt";
                                if (!File.Exists(verifyPath))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Original game file is deleted!");
                                }
                                if (File.Exists(action.MyGame.SaveParentPath + @"\GA-savedGameBackup"))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Backup should not exist.");
                                }
                            }
                            
                        }

                        break;
                    }
                case 11: //check for normal sync action
                    {
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, false);
                        CheckBackupAndSync(syncList, ref result, true);  
                        break;
                    }
                default: break;
            }
            return result;
        }

        private void RestoreGamesInExt()
        {
            string[] dir = Directory.GetDirectories(externalPath);
            foreach (string folder in dir)
            {
                if (Directory.Exists(folder.Substring(0, folder.Length - 5)))
                    Directory.Delete(folder.Substring(0, folder.Length - 5), true);
                Directory.Move(folder, folder.Substring(0, folder.Length - 5));
            }
        }
#endregion

        #region Verify Restore

        /// <summary>
        /// Helper method to check the Restore process
        /// </summary>
        /// <param name="action"></param>
        /// <param name="result"></param>
        private static void CheckRestore(SyncAction action, ref TestResult result)
        {
            //2. verify backup folder is deleted
            if (Directory.Exists(Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup")))
            {
                result.Result = false;
                result.AddRemarks("Failed: "+ action.MyGame.Name+" Backup folder not deleted!");
            }
            if (action.Action == SyncAction.DoNothing)
                return;
            //3 
            if (action.Action == SyncAction.ConfigFiles || action.Action == SyncAction.AllFiles)
            {
                //check each of those in backup is in the game folder
                List<string> diff = new List<string>();
                try
                {
                    diff.AddRange(FolderOperation.FindDifferences(action.MyGame.ConfigParentPath, Path.Combine(action.MyGame.ConfigParentPath, "ConfigTestBackup"), true));
                }
                catch (Exception err)
                {
                    result.Result = false;
                    result.AddRemarks(action.MyGame.Name+" unable to find difference.");
                    result.AddRemarks(err.Message);
                }
                // for each diff, check they are not in the external
                foreach (string s in diff)
                {
                    string path = "";
                    string extension = "";
                    if (s.StartsWith("File: "))
                    {
                        path = s.Substring(6, s.IndexOf(",is")).Trim();
                        extension = path.Remove(0, action.MyGame.ConfigParentPath.Length + 1);
                    }
                    else if (s.StartsWith("Folder: "))
                    {
                        path = s.Substring(8, s.IndexOf(",is")).Trim();
                        extension = path.Remove(0, action.MyGame.ConfigParentPath.Length + 1);
                    }
                    if (File.Exists(Path.Combine(Path.Combine(externalPath, action.MyGame.Name + @"\config"), extension)) ||
                        Directory.Exists(Path.Combine(Path.Combine(externalPath, action.MyGame.Name + @"\config"), extension)))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed: Files/Folders from external still exist : " + path);
                    }
                }
            }
            if (action.Action == SyncAction.SavedGameFiles || action.Action == SyncAction.AllFiles)
            {
                //check each of those in backup is in the game folder
                List<string> diff = new List<string>();
                try
                {
                    diff.AddRange(FolderOperation.FindDifferences(action.MyGame.SaveParentPath, Path.Combine(action.MyGame.SaveParentPath, "SaveTestBackup"), true));
                }catch(Exception err)
                {
                    result.Result = false;
                    result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                    result.AddRemarks(err.Message);
                }
                //add here

                // for each diff, check they are not in the external
                foreach (string s in diff)
                {
                    string path = "";
                    string extension = "";
                    if (s.StartsWith("File: "))
                    {
                        path = s.Substring(6, s.IndexOf(",is")).Trim();
                        extension = path.Remove(0, action.MyGame.SaveParentPath.Length + 1);
                    }
                    else if (s.StartsWith("Folder: "))
                    {
                        path = s.Substring(8, s.IndexOf(",is")).Trim();
                        extension = path.Remove(0, action.MyGame.SaveParentPath.Length + 1);
                    }
                    if (File.Exists(Path.Combine(Path.Combine(externalPath, action.MyGame.Name + @"\config"), extension)) ||
                        Directory.Exists(Path.Combine(Path.Combine(externalPath, action.MyGame.Name + @"\config"), extension)))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed: Files/Folders from external still exist : " + path);
                    }
                }
            }
        }


        public static TestResult VerifyRestore(int index, object[] input,object returnType, object testClass, string exceptionThrown, object expectedOutput, List<FileInfo> deletedFiles)
        {
            bool passed = true;
            TestResult result = new TestResult(passed);
            if (returnType == null) 
            { 
                result.AddRemarks("Error! Unable to verify Restore!");
                result.Result = false;
                return result;
            }
            List<SyncAction> restoreList = (List<SyncAction>)returnType;
            List<Game> inputGame = (List<Game>)input[0];
            switch (index)
            {
                case 1:
                case 2: //check for missing backup files
                case 3:
                case 4: 
                case 5:
                        //1. check return number of list is correct
                        if (restoreList.Count != (int)expectedOutput)
                            result.AddRemarks("Failed! : Expected List is " + expectedOutput + " returned : " + restoreList.Count);
                        else
                            result.AddRemarks("Passed: Expected output okay!");

                        foreach (SyncAction action in restoreList)
                        {
                            CheckRestore(action, ref result);
                        }  

                    break;
                case 6:
                    //1. check return number of list is correct
                    if (restoreList.Count != (int)expectedOutput)
                        result.AddRemarks("Failed! : Expected List is " + expectedOutput + " returned : " + restoreList.Count);
                    else
                        result.AddRemarks("Passed: Expected output okay!");

                    foreach (SyncAction action in restoreList)
                    {
                        if (action.MyGame.Name.Equals("FIFA 10"))
                        {
                            Game g = action.MyGame;
                            List<string> allPath = new List<string>();
                            List<string> errorPath = new List<string>();

                            foreach (string path in g.ConfigPathList)
                            {
                                allPath.Add(Path.GetFileName(path));
                            }

                            allPath.Add("GA-savedGameBackup");
                            allPath.Add("GA-configBackup");

                            foreach (string path in g.SavePathList)
                                allPath.Add(Path.GetFileName(path));

                            foreach (SyncError errorList in action.UnsuccessfulSyncFiles)
                                errorPath.Add(Path.GetFileName(errorList.FilePath));

                            foreach (string path in allPath)
                            {
                                if (! errorPath.Contains(path))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Sync Error List did not list :"+path);
                                }
                            }
                        }

                    }  
                    break;
                case 7:
                    {
                        //1. check return number of list is correct
                        if (restoreList.Count != (int)expectedOutput)
                            result.AddRemarks("Failed! : Expected List is " + expectedOutput + " returned : " + restoreList.Count);
                        else
                            result.AddRemarks("Passed: Expected output okay!");

                        
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
        
        private static void CheckSimilarities(ref TestResult result, List<SyncAction> syncList)
        {
            List<string> diff = new List<string>();
            int count = 0;
            foreach (SyncAction action in syncList)
            {
                count += FolderOperation.GetDirectoryFileCount(Path.Combine(externalPath, Path.Combine(action.MyGame.Name, "config")));
                count += FolderOperation.GetDirectoryFileCount(Path.Combine(externalPath, Path.Combine(action.MyGame.Name, "config")));
                try
                {
                    diff.AddRange(FolderOperation.FindDifferences(Path.Combine(externalPath, Path.Combine(action.MyGame.Name, "config")), action.MyGame.ConfigParentPath, false));
                    diff.AddRange(FolderOperation.FindDifferences(Path.Combine(externalPath, Path.Combine(action.MyGame.Name, "savedGame")), action.MyGame.SaveParentPath, false));
                }
                catch (Exception err)
                {
                    result.Result = false;
                    result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                    result.AddRemarks(err.Message);
                }

            }
            //count should be same as diff list count.
            if (count == diff.Count)
                result.AddRemarks("Passed! Expected result is returned.");
            else
                result.AddRemarks("Failed! External and Game folder should be different!");
        }

        private static void CheckSyncGamesExpectedOutput(object expectedOutput, ref TestResult result, List<SyncAction> syncList, bool checkErrorList)
        {
            bool passed = true;
            if (syncList == null)
            {
                result.Result = false;
                result.AddRemarks("Expected output failed! \n\tNULL return type identififed!");
            }
            else if (syncList.Count == (int)expectedOutput)
                result.AddRemarks("Expected output passed!");
            else
            {
                result.AddRemarks("Expected output failed! \n\tReturned list Count is " + syncList.Count);
                result.Result = false;
            }

            if (checkErrorList)
            {
                foreach (SyncAction action in syncList)
                {
                    if (action.UnsuccessfulSyncFiles.Count != 0)
                    {
                        result.Result = false;
                        passed = false;
                        result.AddRemarks("Expected return type failed!");
                        foreach (SyncError s in action.UnsuccessfulSyncFiles)
                            result.AddRemarks("\tProblem: " + action.MyGame.Name +
                                " file : " + s.FilePath + " is not synchronized to Com!");
                    }
                }
            }
            if (passed)
                result.AddRemarks("Expected return type passed!");
        }
#endregion
        #endregion

        #region Check Backup and SyncAction helper method
        /// <summary>
        /// private helper method to test backup folder and synchronization result
        /// </summary>
        /// <param name="syncList"></param>
        /// <param name="result"></param>
        private static void CheckBackupAndSync(List<SyncAction> syncList, ref TestResult result, bool backup)//,int direction)
        {
            //Debug.Assert(direction == OfflineSync.ExternalToCom || direction == OfflineSync.ComToExternal);
            if (backup)
            {
                //public const String BACKUP_CONFIG_FOLDER_NAME = "GA-configBackup";
                //public const String BACKUP_SAVED_GAME_FOLDER_NAME = "GA-savedGameBackup";
                //verify the GA-[backup] with test backup
                List<string> errorList = new List<string>();
                foreach (SyncAction action in syncList)
                {
                    switch (action.Action)
                    {
                        case 1: //save game file
                            {
                                string testbackup = Path.Combine(action.MyGame.SaveParentPath, "SaveTestBackup");
                                //ensure no back up is created for the config
                                if (Directory.Exists(Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup")))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! " + action.MyGame.Name + " config back up should not exist!");
                                }

                                if (Directory.Exists(Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup")))
                                {
                                    try
                                    {
                                        errorList.AddRange(FolderOperation.FindDifferences(testbackup, Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup"), false));
                                    }
                                    catch (Exception err)
                                    {
                                        result.Result = false;
                                        result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                                        result.AddRemarks(err.Message);
                                    }
                                }
                                else
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Back up not created for : " + "\n\t" + Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup"));
                                }
                                
                                break;
                            }
                        case 2: //config files                          
                            {
                                string testbackup = Path.Combine(action.MyGame.ConfigParentPath, "ConfigTestBackup");
                                //ensure no back up is created for the save
                                if (Directory.Exists(Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup")))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! " + action.MyGame.Name + " Save Game Backup should not exist!");
                                }

                                if (Directory.Exists(Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup")))
                                {
                                    try
                                    {
                                        errorList.AddRange(FolderOperation.FindDifferences(testbackup, Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup"), false));
                                    }
                                    catch (Exception err)
                                    {
                                        result.Result = false;
                                        result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                                        result.AddRemarks(err.Message);
                                    }
                                }
                                else
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! Back up not created for : " + "\n\t" + Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup"));
                                }
                                
                                break;
                            }
                        case 3:
                            {
                                string saveBackup = Path.Combine(action.MyGame.SaveParentPath, "SaveTestBackup");
                                string configBackup = Path.Combine(action.MyGame.ConfigParentPath, "ConfigTestBackup");
                                if (Directory.Exists(Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup")))
                                {
                                    try
                                    {
                                        errorList.AddRange(FolderOperation.FindDifferences(configBackup, Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup"), false));
                                    }
                                    catch (Exception err)
                                    {
                                        result.Result = false;
                                        result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                                        result.AddRemarks(err.Message);
                                    }
                                }
                                else
                                    result.AddRemarks("Failed! Back up not created for : " + "\n\t" + Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup"));
                                if (Directory.Exists(Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup")))
                                {
                                    try
                                    {
                                        errorList.AddRange(FolderOperation.FindDifferences(saveBackup, Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup"), false));
                                    }
                                    catch (Exception err)
                                    {
                                        result.Result = false;
                                        result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                                        result.AddRemarks(err.Message);
                                    }
                                }
                                else
                                    result.AddRemarks("Failed! Back up not created for : " + "\n\t" + Path.Combine(action.MyGame.SaveParentPath, "GA-savedGameBackup"));
                                break;
                            }
                        default:
                            break;
                    }
                }
                if (errorList.Count == 0 && result.Result == true)
                    result.AddRemarks("Passed! Backup Folder is copied in order.");
                else
                {
                    result.Result = false;
                    result.AddRemarks("Failed! Backup Folder is not copied in order!");
                    foreach (string s in errorList)
                        result.AddRemarks("\t" + s);
                }
            }
            List<string> unSyncList = new List<string>();
            //check synchronizing
            foreach (SyncAction action in syncList)
            {
                string external = @".\SyncFolder\"+action.MyGame.Name;
                int type = action.Action;
                if (type == SyncAction.DoNothing) continue;
                switch (type)
                {
                    case 1:
                        external += (@"\savedGame");
                        try
                        {
                            unSyncList.AddRange(FolderOperation.FindDifferences(external, action.MyGame.SaveParentPath, false));
                        }
                        catch (Exception err)
                        {
                            result.Result = false;
                            result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                            result.AddRemarks(err.Message);
                        }
                            break;
                    case 2:
                        external += (@"\config");
                        
                        try
                        {
                            unSyncList.AddRange(FolderOperation.FindDifferences(external, action.MyGame.ConfigParentPath, false));
                        }
                        catch (Exception err)
                        {
                            result.Result = false;
                            result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                            result.AddRemarks(err.Message);
                        } 
                        break;
                    case 3:
                        string configExt = external + @"\config";
                        try
                        {
                            unSyncList.AddRange(FolderOperation.FindDifferences(configExt, action.MyGame.ConfigParentPath, false));
                        }
                        catch (Exception err)
                        {
                            result.Result = false;
                            result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                            result.AddRemarks(err.Message);
                        }
                        string saveExt = external + @"\savedGame";
                        try
                        {
                            unSyncList.AddRange(FolderOperation.FindDifferences(saveExt, action.MyGame.SaveParentPath, false));
                        }
                        catch (Exception err)
                        {
                            result.Result = false;
                            result.AddRemarks(action.MyGame.Name + " unable to find difference.");
                            result.AddRemarks(err.Message);
                        }
                        break;
                    default:
                        break;
                }             
            }
            if (unSyncList.Count == 0)
                result.AddRemarks("Passed! Synchronization okay!");
            else
            {
                result.Result = false;
                result.AddRemarks("Failed! Not Synchronized!!");
                foreach (string s in unSyncList)
                    result.AddRemarks("\t" + s);
            }
           
        }
        #endregion
    }
}
