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
    class GameLibraryVerifier : Verifier
    {

        /// <summary>
        /// Verify the method of GetGameList in GameLibrary.
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
                            if ((!g.Name.Equals(footballManager) && g.SavePathList.Count != 0) ||
                                (g.Name.Equals(footballManager) && g.SavePathList.Count != 1))
                            {
                                //Exception: Check FM 2010 has one saved path list
                                result.Result = false;
                                result.AddRemarks("Failed! " + g.Name + " savePathList expected : 0, returned : " + g.SavePathList.Count);
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
                                result.AddRemarks("\tProblem: " + s + " ,is missing in the Return List");
                            foreach (string s in extraGame)
                                result.AddRemarks("\tProblem: " + s + " ,is not found in the External");
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
                                    result.AddRemarks("Failed! ConfigPathList of " + game.Name + " expects : 1, returned : " + game.ConfigPathList.Count);
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

                        foreach (Game game in gameList)
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
                                if (game.SavePathList.Count != 1)
                                {
                                    result.Result = false;
                                    result.AddRemarks("Failed! SavePathList of " + game.Name + " expects : 1, returned : " + game.SavePathList.Count);
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
    }
}
