using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameAnywhere
{
    class WebThumbVerifier : Verifier
    {

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
            if (returnType != null)
                conflicts = (Dictionary<string, int>)returnType;

            switch (index)
            {

                case 1:
                    {

                        if (conflicts.Count != (int)expectedOutput)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expected : " + expectedOutput + " entries in return, But " + conflicts.Count + " is return.");
                        }

                        if (webThumb.Conflicts.Count != 0 && webThumb.NoConflict.Count != 0)
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
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contains Game1/savedGame/File 1 and direction WebAndThumbSync.NoConflictDirection.Download");
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
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game1/savedGame/File 1 and direction WebAndThumbSync.NoConflictDirection.Upload");
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
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contains Game2/savedGame/File 2 and direction WebAndThumbSync.NoConflictDirection.Upload");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction WebAndThumbSync.NoConflictDirection.Download");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
                        }
                        if (webThumb.NoConflict.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction WebAndThumbSync.NoConflictDirection.Upload");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction WebAndThumbSync.NoConflictDirection.Download");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
                        }
                        if (webThumb.NoConflict.Count != 3)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 3 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction WebAndThumbSync.NoConflictDirection.Upload");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains File 3 and direction WebAndThumbSync.NoConflictDirection.Download");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f4 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game4/savedGame/File 4", WebAndThumbSync.NoConflictDirection.DeleteLocal);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction WebAndThumbSync.NoConflictDirection.DeleteLocal");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
                        }
                        if (webThumb.NoConflict.Count != 3)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 3 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction WebAndThumbSync.NoConflictDirection.Upload");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction WebAndThumbSync.NoConflictDirection.Download");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f4 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game4/savedGame/File 4", WebAndThumbSync.NoConflictDirection.DeleteWeb);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction WebAndThumbSync.NoConflictDirection.DeleteWeb");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
                        }
                        if (webThumb.NoConflict.Count != 5)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 5 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game2/savedGame/File 2 and direction WebAndThumbSync.NoConflictDirection.Upload");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game3/savedGame/File 3 and direction WebAndThumbSync.NoConflictDirection.Download");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f4 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game4/savedGame/File 4", WebAndThumbSync.NoConflictDirection.DeleteLocal);
                        if (!webThumb.NoConflict.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game4/savedGame/File 4 and direction WebAndThumbSync.NoConflictDirection.DeleteLocal");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f5 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game5/savedGame/File 5", WebAndThumbSync.NoConflictDirection.DeleteWeb);
                        if (!webThumb.NoConflict.Contains(f5))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game5/savedGame/File 5 and direction WebAndThumbSync.NoConflictDirection.DeleteWeb");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f6 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game6/savedGame/File 6", WebAndThumbSync.NoConflictDirection.Upload);
                        if (!webThumb.NoConflict.Contains(f6))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts did not contains Game6/savedGame/File 6 and direction WebAndThumbSync.NoConflictDirection.Upload");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload!");
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 1 + " items, conflicts contains " + webThumb.NoConflict.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Download);
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
                        if (webThumb.Conflicts.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Expect " + 2 + " items, conflicts contains " + webThumb.Conflicts.Count + " item");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 =
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Conflicts do not contain Game1/savedGame/File 1 and 0!");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f4 =
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game4/savedGame/File 4", WebAndThumbSync.ConflictDirection.UploadOrDownload);
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
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f2 =
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game2/savedGame/File 2", WebAndThumbSync.NoConflictDirection.Download);
                        if (!webThumb.NoConflict.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflicts do not contain Game2/savedGame/File 2 and 2!");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game3/savedGame/File 3", WebAndThumbSync.NoConflictDirection.Upload);
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

                        if (webThumb.Conflicts.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts and noConflicts is not 0!");
                        }
                        KeyValuePair<string, WebAndThumbSync.NoConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.NoConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.NoConflictDirection.DeleteMetadata);
                        if (!webThumb.NoConflict.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict did not contain : Game1/savedGame/File 1 with direction :5");
                        }
                        break;
                    }

                case 13:
                    {
                        if (webThumb.NoConflict.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! noConflict should contains 0 entry, current: " + webThumb.NoConflict.Count);
                        }
                        if (webThumb.Conflicts.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflict should contain only 1 entry.");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDeleteLocal);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDeleteLocal");
                        }
                        break;
                    }
                case 14:
                    {
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflict did not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f2 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 2", WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb);
                        if (!webThumb.Conflicts.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts did not contain Game1/savedGame/File 2 and WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb");
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
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f1 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 1", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f1))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/savedGame/File 1 and WebAndThumbSync.ConflictDirection.UploadOrDownload");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f2 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/savedGame/File 2", WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb);
                        if (!webThumb.Conflicts.Contains(f2))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/savedGame/File 2 and WebAndThumbSync.ConflictDirection.DownloadOrDeleteWeb");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f3 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game1/config/File 3", WebAndThumbSync.ConflictDirection.UploadOrDeleteLocal);
                        if (!webThumb.Conflicts.Contains(f3))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game1/config/File 3 and WebAndThumbSync.ConflictDirection.UploadOrDeleteLocal");
                        }
                        KeyValuePair<string, WebAndThumbSync.ConflictDirection> f4 = 
                            new KeyValuePair<string, WebAndThumbSync.ConflictDirection>("Game2/config/File 4", WebAndThumbSync.ConflictDirection.UploadOrDownload);
                        if (!webThumb.Conflicts.Contains(f4))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! conflicts expect Game2/config/File 4 and WebAndThumbSync.ConflictDirection.UploadOrDownload");
                        }
                        //check return
                        if (returnType != null)
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

        /// <summary>
        /// This method verifies the web thumb synchronization
        /// </summary>
        /// <param name="index"></param>
        /// <param name="returnType"></param>
        /// <param name="testClass"></param>
        /// <param name="exceptionThrown"></param>
        /// <param name="expectedOutput"></param>
        /// <returns></returns>
        public static TestResult VerifyWebThumbSync(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {

            
            TestResult result = new TestResult(true);
            Storage store = new Storage();
            //used to compare previous version of verify meta
            MetaData verifyMeta = new MetaData();
            MetaData currentMeta = new MetaData();
            switch (index)
            {
                case 1:
                    {
                        verifyMeta = verifyMeta.DeSerialize(@".\localTest" + index + "-test" + @"\" + WebAndThumbSync.LocalMetaDataFileName);
                        currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);

                        foreach (string key in verifyMeta.FileTable.Keys)
                        {
                            string testValue = verifyMeta.FileTable[key];
                            string currentValue = currentMeta.FileTable[key];
                            if (!testValue.Equals(currentValue))
                            {
                                result.Result = false;
                                result.AddRemarks("Failed! current meta : " + key + " " + currentValue + " is different with meta in initial :" + testValue);
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt","savedGame","Game1");
                        
                        break;
                    }
                case 3:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        break;
                    }
                case 4:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        break;
                    }
                case 5:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        break;
                    }
                case 6:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckDeleteLocal(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "savedGame", "Game4");
                        break;
                    }
                case 7:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");

                        CheckDeleteWeb(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "savedGame", "Game4");
                        break;
                    }
                case 8:
                    {
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        CheckDeleteLocal(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "savedGame", "Game4");
                        CheckDeleteLocal(index, ref result, ref verifyMeta, ref currentMeta, "File 5.txt", "savedGame", "Game5");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 6.txt", "savedGame", "Game6");
                        break;
                    }
                case 9:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        break;
                    }
                case 10:
                    {
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        break;
                    }
                case 11:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game2");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "savedGame", "Game3");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "savedGame", "Game4");
                        break;
                    }
                case 12:
                    {
                        currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);
                        if (currentMeta.FileTable.Count != 0)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Meta data of File 1.txt should be removed!");
                        }
                        break;
                    }
                case 13:
                    {
                        CheckDeleteLocal(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        break;
                    }
                case 14:
                    {
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        break;
                    }
                case 15:
                    {
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckDeleteWeb(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game1");
                        break;
                    }
                case 16:
                    {
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 1.txt", "savedGame", "Game1");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 2.txt", "savedGame", "Game1");
                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "config", "Game1");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "config", "Game2");
                        break;
                    }
                case 17:
                    {
                        List<SyncError> errorList = (List<SyncError>)returnType;
                        if (errorList.Count != 1)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Error List fail to include File 1.txt and File 2.txt of Game 1.");
                        }

                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "config", "Game1");
                        CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "config", "Game2");
                        break;
                    }
                case 18:
                    {
                        List<SyncError> errorList = (List<SyncError>)returnType;
                        if (errorList.Count != 2)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Error List fail to include File 1.txt of Game1/savedGame and File 4.txt of Game2/config.");
                        }

                        CheckUpload(index, ref result, ref verifyMeta, ref currentMeta, "File 3.txt", "config", "Game1");
                        //CheckDownload(index, ref result, ref verifyMeta, ref currentMeta, "File 4.txt", "config", "Game2");
                        break;
                    }

            }

            //All cases conflicts should be 0 after synchronization method.

            return result;
        }

        private static void CheckDeleteWeb(int index, ref TestResult result, ref MetaData verifyMeta, ref MetaData currentMeta
            , string fileName, string fileType, string gameName)
        {
            Storage store = new Storage();
            string webId = "WebAndThumb@gmails.com";
            string key = gameName + "/" + fileType + "/" + fileName;

            //current local meta data
            currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);

            try
            {
                store.DownloadFile(@".\SyncFolder\" + gameName + @"\" + fileType + @"\verify" + fileName, webId + "/" + key);
                result.Result = false;
                result.AddRemarks("Failed! file : " + fileType + "\\" + fileName + ", should be deleted on the Web!");
            }
            catch{
                //pass if exception thrown
            }
            if (currentMeta.EntryExist(key))
            {
                result.Result = false;
                result.AddRemarks("Failed! file : " + fileType + "\\" + fileName + ", should be deleted in meta data!");
            }
        }

        private static void CheckDeleteLocal(int index, ref TestResult result, ref MetaData verifyMeta, ref MetaData currentMeta
            , string fileName, string fileType, string gameName)
        {
            string key = gameName + "/" + fileType + "/" + fileName;

            //current local meta data
            currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);

            if (File.Exists(@".\SyncFolder\" + gameName + @"\" + fileType + @"\" + fileName))
            {
                result.Result = false;
                result.AddRemarks("Failed! file : "+fileType+"\\"+fileName+", should be deleted on com!");
            }
            if (currentMeta.EntryExist(key))
            {
                result.Result = false;
                result.AddRemarks("Failed! file : " + fileType + "\\" + fileName + ", should be deleted in meta data!");
            }
        }

        private static void CheckUpload(int index, ref TestResult result, ref MetaData verifyMeta, ref MetaData currentMeta
            ,string fileName, string fileType, string gameName)
        {
            Storage store = new Storage();
            string webId = "WebAndThumb@gmails.com";
            string key = gameName+"/"+fileType+"/"+fileName;

            //current local metadata
            currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);
            verifyMeta = verifyMeta.DeSerialize(@".\localTest" + index + @"-test\" + WebAndThumbSync.LocalMetaDataFileName);
            //get file from web and it's hash value
            store.DownloadFile(@".\SyncFolder\"+gameName+@"\"+fileType+@"\verify"+fileName, webId + "/"+key);
            string webHash = FolderOperation.GenerateHash(@".\SyncFolder\"+gameName+@"\"+fileType+@"\verify"+fileName);

            //get local hash value
            string localHash = FolderOperation.GenerateHash(@".\SyncFolder\"+gameName+@"\"+fileType+@"\"+fileName);

            //check web hash same as localTest(i)-test, local hash same as web hash, local hash in current MetaData       
            try
            {
                string verifyValue = verifyMeta.FileTable[key];
                string currentValue = currentMeta.FileTable[key];

                //if the computer's copy hash has not changed compared to original
                if (!localHash.Equals(verifyValue))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! file : "+fileType+"\\"+fileName+", Upload operation, computer value should not change!");
                }
                if (!localHash.Equals(webHash))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! file : " + fileType + "\\" + fileName + ", local hash and web hash Not Synchronzied!");
                }
                if (!localHash.Equals(currentValue))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! " + fileType + "\\" + fileName + ",MetaData not updated with current hash!");
                }
            }
            catch
            {
                result.Result = false;
                result.AddRemarks("Failed! unable to find key.");
            }
        }

        /// <summary>
        /// Check the upload direction for 1 file
        /// </summary>
        /// <param name="index"></param>
        /// <param name="result"></param>
        /// <param name="verifyMeta"></param>
        /// <param name="currentMeta"></param>
        /// <param name="FileName">The file name: eg. Gile 1.txt</param>
        /// <param name="FileType">The file type. eg. savedGame</param>
        /// <param name="gameName">The game name</param>
        private static void CheckDownload(int index, ref TestResult result, ref MetaData verifyMeta, ref MetaData currentMeta,
            string fileName, string fileType,string gameName)
        {
            string webId = "WebAndThumb@gmails.com";
            Storage store = new Storage();
            string key = gameName+"/"+fileType+"/"+fileName;

            //check computer copy for file 1 is overwritten by web copy
            verifyMeta = verifyMeta.DeSerialize(@".\webTest" + index + @"\" + WebAndThumbSync.LocalMetaDataFileName);
            currentMeta = currentMeta.DeSerialize(@".\SyncFolder\" + WebAndThumbSync.LocalMetaDataFileName);
            store.DownloadFile(@".\SyncFolder\"+gameName+@"\"+fileType+@"\verify"+fileName, webId+"/"+key);
            string localHash = FolderOperation.GenerateHash(@".\syncFolder\"+gameName+@"\"+fileType+@"\"+fileName);
            string webHash = FolderOperation.GenerateHash(@".\SyncFolder\"+gameName+@"\"+fileType+@"\verify"+fileName);
            MetaData checkOnly = new MetaData();
            
            try
            {
                string webOldValue = FolderOperation.GenerateHash(@".\webTest" + index + @"\" + gameName + @"\" + fileType + @"\" + fileName);
                string currentValue = currentMeta.FileTable[key];

                //check if web values changes 
                if (!webHash.Equals(webOldValue))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! Download operation, web's file : "+fileType+"\\"+fileName+" hash code should not change!");
                }
                //check if web and local value differs
                if (!webHash.Equals(localHash))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! " + fileType + "\\" + fileName + " hash code from web and local differs! Not Synchronized!");
                }
                if (!localHash.Equals(currentMeta.FileTable[key]))
                {
                    result.Result = false;
                    result.AddRemarks("Failed! Local MetaData differs from the current file.");
                }
            }
            catch
            {
                result.Result = false;
                result.AddRemarks("Failed! unable to find key.");
            }
        }
    }
}
