using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameAnywhere
{
    class OfflineSyncVerifier : Verifier
    {

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
            if (returnType != null && returnType.GetType().Equals(typeof(List<SyncAction>)))
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
                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3", OfflineSync.Uninitialize), SyncAction.ConfigFiles));
                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, wc3Only, true);

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
                                foreach (SyncError error in action.UnsuccessfulSyncFiles)
                                {
                                    if (Path.GetFileName(error.FilePath).Equals("SaveTestBackup")
                                     || Path.GetFileName(error.FilePath).Equals("ConfigTestBackup"))
                                        continue;
                                    if (!invalid.Contains(Path.GetFileName(error.FilePath)))
                                    {
                                        result.Result = false;
                                        result.AddRemarks("Failed! ErrorList: " + error.FilePath + " is not reflected in the SyncError");
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
                        wc3Only.Add(new SyncAction(PreCondition.getGame("Warcraft 3", OfflineSync.ExternalToCom), SyncAction.AllFiles));

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
                                        result.AddRemarks("Failed! " + name + " is should not be copied over!");
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

                        CheckSyncGamesExpectedOutput(expectedOutput, ref result, syncList, false);

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
                                string[] configFiles = Directory.GetDirectories(externalPath + @"\FIFA 10\config");
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
                result.AddRemarks("Failed: " + action.MyGame.Name + " Backup folder not deleted!");
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
                    result.AddRemarks(action.MyGame.Name + " unable to find difference.");
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
                }
                catch (Exception err)
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


        public static TestResult VerifyRestore(int index, object[] input, object returnType, object testClass, string exceptionThrown, object expectedOutput, List<FileInfo> deletedFiles)
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

       

        
        #endregion
    }
}
