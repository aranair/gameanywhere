using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using GameAnywhere.Data;


namespace GameAnywhere.Process
{
    /// <summary>
    /// Verifier class contains all the methods required to verified 
    /// for all the test cases for each of the classes
    /// 
    /// 
    /// </summary>
    class Verifier
    {

        internal static string externalPath = @".\SyncFolder";
        internal static List<string> supportedGames = new List<string>(new string[] {"Warcraft 3","FIFA 10","Football Manager 2010","World of Warcraft"});
        public static readonly string Warcraft3GameName = "Warcraft 3";
        public static readonly string FIFA10GameName = "FIFA 10";
        public static readonly string worldOfWarcraft = "World of Warcraft";
        public static readonly string footballManager = "Football Manager 2010";
        public static readonly string abuse = "Abuse";

       

        #region Check Backup and SyncAction helper method

        /// <summary>
        /// This method verifies the games on web with the files on the com
        /// </summary>
        /// <param name="result"></param>
        /// <param name="listOfWebPath"></param>
        /// <param name="game"></param>
        /// <param name="type"></param>
        internal static void CheckWithGamesOnWeb(ref TestResult result, List<string> listOfWebPath, Game game, int type)
        {
            List<string> filesOnWeb = new List<string>();
            foreach (string file in listOfWebPath)
                filesOnWeb.Add(file.Substring(file.LastIndexOf('/') + 1));
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

        /// <summary>
        /// Check for similarities 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="syncList"></param>
        internal static void CheckSimilarities(ref TestResult result, List<SyncAction> syncList)
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
        /// Check the expected output
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="result"></param>
        /// <param name="syncList"></param>
        /// <param name="checkErrorList"></param>
        internal static void CheckSyncGamesExpectedOutput(object expectedOutput, ref TestResult result, List<SyncAction> syncList, bool checkErrorList)
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
        /// <summary>
        /// private helper method to test backup folder and synchronization result
        /// </summary>
        /// <param name="syncList"></param>
        /// <param name="result"></param>
        internal static void CheckBackupAndSync(List<SyncAction> syncList, ref TestResult result, bool backup)//,int direction)
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
                        case 1: //config files                          
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
                        case 2: //save game file
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
                if (errorList.Count == 0)
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
                    case 2:
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
