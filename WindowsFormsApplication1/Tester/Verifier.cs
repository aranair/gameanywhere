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

        private static List<string> supportedGames = new List<string>(new string[] {"Warcraft 3","FIFA 10","Football Manager 2010","World of Warcraft"});

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
                                string testbackup = Path.Combine(action.MyGame.ConfigParentPath, "SaveTestBackup");
                                //ensure no back up is created for the config
                                if (Directory.Exists(Path.Combine(action.MyGame.ConfigParentPath, "GA-configBackup")))
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! " + action.MyGame.Name + " config back up should not exist!");
                                }

                                if (Directory.Exists(testbackup))
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

                                if (Directory.Exists(testbackup))
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
