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
    /// <summary>
    /// PreCondition Class is used to set the preconditions for each of the test cases if neccessary
    /// It also contains Helper methods to clean up the extra files/folders created during the testing process
    /// 
    /// For each test case decription, please refer to the text file decription.
    /// </summary>
    class PreCondition
    {
        
        internal static string externalPath = @".\SyncFolder";
        internal static List<string> supportedGames = new List<string>(new string[] { "Warcraft 3", "FIFA 10" });

        //Games name
        public static readonly string Warcraft3GameName = "Warcraft 3";
        public static readonly string FIFA10GameName = "FIFA 10";
        public static readonly string worldOfWarcraft = "World of Warcraft";
        public static readonly string footballManager = "Football Manager 2010";
        public static readonly string abuseGameName = "Abuse";
        public static readonly string theSims3 = "The Sims 3";

        internal static GameLibrary library = new GameLibrary();
        internal static string fifaSavePath;
        internal static string wc3InstallPath;
        internal static string user = WindowsIdentity.GetCurrent().Name;
        internal static string comToWebUser = "TestComToWeb@gmails.com";
        internal static string webToComUser = "TestWebToCom@gmails.com";

        /// <summary>
        /// Store the list of games which are sync on the web.
        /// </summary>
        private static List<string> syncGamesOnWeb;

        /// <summary>
        /// Remove the test folders created for verifying the outcome of action
        /// </summary>
        internal static void DeleteTestBackup()
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

        /// <summary>
        /// Delete the folders created to verify the web to com sync
        /// </summary>
        internal static void DeleteVerifySyncFolder()
        {
            foreach (Game game in library.InstalledGameList)
            {
                string saveBackup = @".\SaveSyncTest-"+game.Name;
                string configBackup = @".\ConfigSyncTest-"+game.Name;

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
        /// Restore the computer back to original condition, eg. restore internet connection
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
                                FolderOperation.RemoveFileSecurity(externalPath+@"\"+FIFA10GameName, username,
                                    FileSystemRights.FullControl, AccessControlType.Deny);
                                FolderOperation.AddFileSecurity(externalPath + @"\"+FIFA10GameName, username,
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
                            case 9: //delete all the files copied over to com
                                {
                                    Game fifa = getGame(FIFA10GameName, OfflineSync.Uninitialize);
                                    Game wc3 = getGame(Warcraft3GameName, OfflineSync.Uninitialize);
                                    Game fm = getGame(footballManager, OfflineSync.Uninitialize);
                                    Game wow = getGame(worldOfWarcraft, OfflineSync.Uninitialize);
                                    Game abuse = getGame(abuseGameName, OfflineSync.Uninitialize);  
                                    
                                    //delete all FIFA files copied
                                    if (Directory.Exists(fifa.SaveParentPath))
                                    {
                                        Directory.Delete(fifa.SaveParentPath,true);
                                        Directory.CreateDirectory(fifa.SaveParentPath);
                                    }

                                    //delete all wc3 files copied
                                    if (Directory.Exists(wc3.SaveParentPath+@"\Save"))
                                    {
                                        Directory.Delete(wc3.SaveParentPath+@"\Save" ,true);
                                    }
                                    if (File.Exists(wc3.ConfigParentPath + @"\CustomKeys.txt"))
                                    {
                                        File.Delete(wc3.ConfigParentPath + @"\CustomKeys.txt");
                                    }

                                    //delete all fm files
                                    if (File.Exists(fm.SaveParentPath + @"\games\config.xml"))
                                    {
                                        File.Delete(fm.SaveParentPath + @"\games\config.xml");
                                    }

                                    //delete all Wow Files
                                    if (Directory.Exists(wow.ConfigParentPath))
                                    {
                                        Directory.Delete(wow.ConfigParentPath,true);
                                        Directory.CreateDirectory(wow.ConfigParentPath);
                                    }
                                    if (Directory.Exists(wow.SaveParentPath + @"\Interface"))
                                    {
                                        Directory.Delete(wow.SaveParentPath + @"\Interface",true);
                                        //Directory.CreateDirectory(wow.SaveParentPath + @"\Interface");
                                    }

                                    //delete all Abuse files
                                    if (File.Exists(abuse.SaveParentPath + @"\save0001.spe"))
                                    {
                                        File.Delete(abuse.SaveParentPath + @"\save0001.spe");
                                    }
                                    if(File.Exists(abuse.SaveParentPath + @"\save1.spe"))
                                    {
                                        File.Delete(abuse.SaveParentPath + @"\save1.spe");
                                    }
                                    if (File.Exists(abuse.SaveParentPath + @"\save2.spe"))
                                    {
                                        File.Delete(abuse.SaveParentPath + @"\save2.spe");
                                    }

                                    break;
                                }
                            case 10:
                                break;
                        }
                        break;
                    }
                case "SynchronizeGames":
                    {


                        if (cleanUpClass.GetType().Equals(typeof(OfflineSync)))
                        {
                            OfflineSyncPreCondition.CleanUpOffLineSync(index, user);
                        }
                        else if (cleanUpClass.GetType().Equals(typeof(OnlineSync)))
                        {
                            //Online sync Test cases
                            OnlineSyncPreCondition.CleanUpOnlineSync(index, user);
                            break;
                        }
                        else if (cleanUpClass.GetType().Equals(typeof(WebAndThumbSync)))
                        {
                            WebThumbPreCondition.CleanUpWebAndThumb(index, user);
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
                                    Game wc3 = getGame(Warcraft3GameName,OfflineSync.Uninitialize);
                                    Game fifa = getGame(FIFA10GameName,OfflineSync.Uninitialize);
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
                                Game fm = PreCondition.getGame(footballManager, OfflineSync.ExternalToCom);

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
                            case 8:
                            {
                                DeleteTestBackup();
                                break;
                            }
                            default: break;
                        }
                        break;
                    }

                case "Register":
                case "RetrievePassword":
                case "ChangePassword":
                case "Login":
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

        internal static void RestoreTestFolderName()
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
                    GameLibraryPreCondition.SetGameLibraryPreCondition(index, methodName, ref input, ref testClass);
                    break;
                case "SynchronizeGames":
                    if (testClass.GetType().Equals(typeof(OfflineSync)))
                    {
                        OfflineSyncPreCondition.SetOfflineSyncPreCondition(index, ref input, ref testClass, user);
                    }
                    else if (testClass.GetType().Equals(typeof(OnlineSync)))
                    {
                        //Online sync Test cases
                        OnlineSyncPreCondition.SetOnlineSyncPreCondition(index, ref input, ref testClass, user);
                    }
                    else if (testClass.GetType().Equals(typeof(WebAndThumbSync)))
                    {
                        WebThumbPreCondition.SetWebAndThumbPreCondition(index, ref input, ref testClass, user);
                    }
                    break;

                case "CheckConflicts":
                    {
                        MetaData localHash = new MetaData();
                        MetaData localMeta = new MetaData();
                        MetaData webHash = new MetaData();
                        MetaData webMeta = new MetaData();

                        WebThumbPreCondition.SetCheckConflictPreCondition(index, ref localHash, ref localMeta, ref webHash, ref webMeta);

                        WebAndThumbSync webThumb = (WebAndThumbSync)testClass;
                        webThumb.LocalHash = localHash;
                        webThumb.LocalMeta = localMeta;
                        webThumb.WebHash = webHash;
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
                        case 8:
                            CreateGATestBackup(restoreGame);
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
        /// Used to set online sync pre-condition
        /// Copies all the games over to the com with offline sync
        /// </summary>
        public static void CopyGameFromThumbToCom()
        {
            string[] gameName = {"FIFA 10","Warcraft 3","World of Warcraft","Football Manager 2010","Abuse"};
            Game[] games = new Game[5];
            SyncAction[] syncList = new SyncAction[5];
            GameLibrary lib = new GameLibrary();
            for(int i=0; i<5; ++i)
            {
                games[i] = PreCondition.getGame(gameName[i],OfflineSync.ExternalToCom);
                syncList[i] = new SyncAction(games[i], OfflineSync.ExternalToCom);
                if (games[i].Name.Equals("Football Manager 2010") ||
                    games[i].Name.Equals("Abuse")||
                    games[i].Name.Equals("World of Warcraft")||
                    games[i].Name.Equals("The Sims 3"))
                    syncList[i].Action = 2;
                else
                    syncList[i].Action = 3;
            }
            OfflineSync off = new OfflineSync(OfflineSync.ExternalToCom, lib.GetGameList(OfflineSync.Uninitialize));
            off.SynchronizeGames(new List<SyncAction>(syncList));
        }

        

        internal static void RemoveGamesInExt()
        {
            string[] dir = Directory.GetDirectories(externalPath);
            foreach (string folder in dir)
                Directory.Move(folder, folder + "-test");
        }

        internal static void CreateGATestBackup(List<Game> restoreGame)
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
        /// format : Array<Type>:{obj1+obj2}
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
                            int dir = Convert.ToInt32(newGame[1]);

                            if (dir == OnlineSync.ComToWeb)
                            {
                                CopyGameFromThumbToCom();
                            }

                            tempGame = getGame(newGame[0],dir);
                            
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
            List<Game> gameslist = null;
            string webToCom = "TestWebToCom@gmails.com", comToWeb = "TestComToWeb@gmails.com";
            User webUser = new User();
            if (direction == 4)
            {
                List<string> listOfWebGames = OnlineSync.GetGamesAndTypesFromWeb(webToCom);
                gameslist = library.GetGameList(direction,listOfWebGames);
            }
            else
                gameslist = library.GetGameList(direction);

            foreach (Game g in gameslist)
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