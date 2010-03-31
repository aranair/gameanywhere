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
        public static readonly string Warcraft3GameName = "Warcraft 3";
        public static readonly string FIFA10GameName = "FIFA 10";
        public static readonly string worldOfWarcraft = "World of Warcraft";
        public static readonly string footballManager = "Football Manager 2010";
        public static readonly string abuse = "Abuse";

        /* Revmoed for v0.9.1
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
                            if(s.Contains('/'))
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
                        Game fm2010 = PreCondition.getGame(footballManager,OnlineSync.ComToWeb);
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

                case 4:
                    {
                        try
                        {
                            store.DownloadFile(@".\verify.txt", comToWeb + "/Warcraft 3/config/CustomKeys.txt");
                        }
                        catch (Exception err)
                        {
                            //MessageBox.Show(err.Message+"\n"+err.GetType().Name);
                        }
                        List<SyncAction> returnAction = (List<SyncAction>) returnType;
                        foreach(SyncAction sa in returnAction)
                        {
                            bool found = false;
                            if (sa.MyGame.Name.Equals("Warcraft 3"))
                            {
                                foreach(SyncError syncError in sa.UnsuccessfulSyncFiles)
                                {
                                    if (syncError.FilePath.Equals(@"C:\Warcraft III\Warcraft III\CustomKeys.txt"))
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
                        List<SyncAction> returnAction = (List<SyncAction>)returnType;
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, returnAction, false);
                        foreach (SyncAction sa in returnAction)
                        {
                            bool configFound = false;
                            bool saveFound = false;
                            if (sa.MyGame.Name.Equals("Warcraft 3"))
                            {
                                foreach (SyncError syncError in sa.UnsuccessfulSyncFiles)
                                {
                                    if (syncError.FilePath.Equals(@"C:\Warcraft III\Warcraft III\CustomKeys.txt"))
                                        configFound = true;
                                }
                                foreach (SyncError syncError in sa.UnsuccessfulSyncFiles)
                                {
                                    if (syncError.FilePath.Equals(@"C:\Warcraft III\Warcraft III\Save"))
                                        saveFound = true;
                                }
                            }
                            if (!configFound)
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! config Error path is not found in unsuccessful file list");
                            }
                            if (!saveFound)
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! save Error path is not added correctly in unsuccessful file list.");
                            }
                        }

                        break;
                    }
                case 6:
                    break;
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
                                        result.AddRemarks("Failed! "+Path.GetFileName(file)+" not added in the unsuccessful sync-list.");
                                    }
                                }
                            }
                        }
                        break;
                    }

                    /*
                     * 
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
        */

        /// <summary>
        /// Check the sync and backup for online sync
        /// </summary>
        private static List<string> CheckOnlineSync(ref TestResult result, SyncAction sa)
        {
            List<string> errList = new List<string>();

            Game game = PreCondition.getGame(sa.MyGame.Name, OfflineSync.ComToExternal);
            //check sync
            if (sa.Action == SyncAction.ConfigFiles || sa.Action == SyncAction.AllFiles)
            {
                string[] checkFiles = Directory.GetFiles(@".\ConfigSyncTest");
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
                        result.AddRemarks("Failed! "+file+ " is not found in the Game folder!");
                    }
                }
            }
            if (sa.Action == SyncAction.SavedGameFiles || sa.Action == SyncAction.AllFiles)
            {
                //check sync
                string[] checkFiles = Directory.GetFiles(@".\SaveSyncTest");
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
                    errList.AddRange(FolderOperation.FindDifferences(game.ConfigParentPath + @"\ConfigTestBackup", game.ConfigParentPath + @"\GA-configBackup",false));
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
                    errList.AddRange(FolderOperation.FindDifferences(game.SaveParentPath + @"\SaveTestBackup", game.ConfigParentPath + @"\GA-savedGameBackup", false));
                }
            }
            return errList;
        }

        private static void CheckWithGamesOnWeb(ref TestResult result, List<string> listOfWebPath, Game game, int type)
        {
            List<string> filesOnWeb = new List<string>();
            foreach (string file in listOfWebPath)
                filesOnWeb.Add(file.Substring(file.LastIndexOf('/')+1));
            //Check save for game
            List<string> saveGameList = new List<string>();

            if (type == SyncAction.SavedGameFiles || type == SyncAction.AllFiles)
            {
                foreach (string s in game.SavePathList)
                {
                    if (File.Exists(s))
                        saveGameList.Add(s);
                    else
                    {
                        List<string> getFiles = FolderOperation.GetAllFilesName(s);
                        saveGameList.AddRange(getFiles);
                    }
                }
                foreach (string com in saveGameList)
                {
                    string comFile = "";
                    if (File.Exists(com))
                        comFile = com.Substring(com.LastIndexOf('\\') + 1);
                    else
                        comFile = com.Substring(com.LastIndexOf('\\') + 1);
                    //check web contains them
                    if (!filesOnWeb.Contains(comFile))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! " + comFile + " not uploaded to web.");
                    }

                }
            }
            List<string> configGameList = new List<string>();
            if (type == SyncAction.ConfigFiles || type == SyncAction.AllFiles)
            {
                foreach (string s in game.ConfigPathList)
                {
                    if (File.Exists(s))
                        configGameList.Add(s);
                    else
                    {
                        List<string> getFiles = FolderOperation.GetAllFilesName(s);
                        configGameList.AddRange(getFiles);
                    }
                }
                //Check config for game
                foreach (string com in configGameList)
                {
                    string comFile = "";
                    if (File.Exists(com))
                        comFile = com.Substring(com.LastIndexOf('\\') + 1);
                    else
                        comFile = com.Substring(com.LastIndexOf('\\') + 1);
                    //check web contains them
                    if (!filesOnWeb.Contains(comFile))
                    {
                        result.Result = false;
                        result.AddRemarks("Failed! " + comFile + " not uploaded to web.");
                    }
                }
            }
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
            List<Game> gameList = (List<Game>)returnType;
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
                        foreach (Game g in gameList)
                        {
                            if (g.ConfigPathList.Count != 0)
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! " + g.Name + " configlist expected : 0 , returned :" + g.ConfigPathList.Count);
                            }
                            if ((! g.Name.Equals(footballManager)  && g.SavePathList.Count != 0) || 
                                (g.Name.Equals(footballManager) && g.SavePathList.Count != 1))
                            {
                                //Exception: Check FM 2010 has one saved path list
                                result.Result = false;
                                result.AddRemarks("Failed! "+g.Name+" savePathList expected : 0, returned : "+g.SavePathList.Count);
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
                    {
                        if (gameList.Count != 0)
                        {
                            result.AddRemarks("Expected output failed! \n\tExpected: " + expectedOutput + " ,Output: " + gameList.Count);
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 5: //Not important: Asserted by caller
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
                                result.AddRemarks("Failed! FIFA 10 not supposed to be in the return list");
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
                case 9:
                case 10:
                    {
                        //check for return count
                        if (gameList.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Returned: " + gameList.Count + ". Expecting only " + (int)expectedOutput + " games in the return list");
                        }
                        //check their repective savePathList and configPathList
                        foreach (Game game in gameList)
                        {
                            //check fifa 
                            if (game.Name.Equals(FIFA10GameName))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of "+game.Name+" expects : 1, returned : "+game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 3)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 3, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check Wc3
                            if (game.Name.Equals(Warcraft3GameName))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check FM
                            if (game.Name.Equals(footballManager))
                            {
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check WoW
                            if (game.Name.Equals(worldOfWarcraft))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check Abuse
                            if (game.Name.Equals(abuse))
                            {
                                if (game.SavePathList.Count != 3)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 3, returned : " + game.SavePathList.Count);
                                }
                            }
                        }
                        break;
                    }
                case 11:
                    {
                        if (gameList.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Returned: " + gameList.Count + ". Expecting only " + (int)expectedOutput + " games in the return list");
                        }

                        foreach(Game game in gameList)
                        {
                            //check fifa 
                            if (game.Name.Equals(FIFA10GameName))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check Wc3
                            if (game.Name.Equals(Warcraft3GameName))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check FM
                            if (game.Name.Equals(footballManager))
                            {
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check WoW
                            if (game.Name.Equals(worldOfWarcraft))
                            {
                                if (game.ConfigPathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
                                }
                                if (game.SavePathList.Count != 0)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 0, returned : " + game.SavePathList.Count);
                                }
                            }

                            //check Abuse
                            if (game.Name.Equals(abuse))
                            {
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
                                }
                            }
                        }
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
                case 5: //
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
                case 12:
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
                case 8:
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="result"></param>
        /// <param name="syncList"></param>
        /// <param name="checkErrorList"></param>
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
