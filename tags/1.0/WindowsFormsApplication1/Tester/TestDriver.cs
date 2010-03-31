using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Security.Cryptography;

using System.Windows.Forms;

namespace GameAnywhere
{

    /// <summary>
    /// Pre-condition for test driver, 
    /// both save and config files 
    /// do not exist in all the games
    /// 
    /// Purpose of TestDriver.cs is to test:
    /// 
    ///     for (each public methods of a class)
    ///         for (every possible inputs)
    ///             ensure that the method returns the expected output
    ///             ensure that each of the exceptions is properly handled
    ///
    /// </summary>
    class TestDriver
    {
        private string className;
        private object[] constructorParameters;
        private object testClass;
        private List<TestCase> testCases;    //stores ArrayList of TestCase
        string[] tests; //stores the test cases read from the test file

        public TestDriver()
        {
            className = null;
            constructorParameters = null; tests = null;
            testCases = null; testClass = null;
        }

        public TestDriver(string testPath)
        {
            if(testPath.EndsWith(".tcf"))
                tests = File.ReadAllLines(testPath);
            testCases = new List<TestCase>();
            InitializeClassInfo();
            InitializeTestCases();
        }

        /// <summary>
        /// This method will create the instance of the class based on the constructor parameter(s) specified
        /// </summary>
        private void InitializeClassInfo()
        {
            Type foundType = null;
            string temp = tests[0];
            string[] classData = temp.Split(new Char[] { '|' });
            this.className = classData[0];
            object[] constructorParameters = readConstructorArguments(classData[1]);
            
            //Load the assembly and create the instance of it
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass && type.Name.Equals(className))
                    foundType = type;
            }
            Debug.Assert(foundType != null);
            this.testClass = Activator.CreateInstance(foundType, constructorParameters);
            if (testClass.GetType().Equals(typeof(GameLibrary)))
            {
                GameLibrary gl = (GameLibrary)testClass;
                gl.RefreshList();
                testClass = gl;
            }
        }

        /// <summary>
        /// Initialize and add in the test cases to the list
        /// </summary>
        private void InitializeTestCases()
        {

            for (int i = 1; i < tests.Length; ++i)
            {
                if(! tests[i].Trim().Equals(""))
                    testCases.Add(new TestCase(tests[i], testClass));
            }
        }


        /// <summary>
        /// This method will create the test folders/files for testing the sync method 
        /// </summary>
        /// <param name="localHash">a string array of the text file</param>
        public void CreateWebThumbSyncTestFile(string[] file)
        {
            int index = 0;
            string front = @".\";
            while (!file[index].Equals("#"))
            {
                if (file[index].Equals("@"))
                {
                    front = @".\"+file[++index];
                    Directory.CreateDirectory(front);
                    ++index;
                }
                else
                {
                    string[] current = file[index].Split(new Char[] { ',' });
                    FolderOperation.CreateFile(current[0],front);
                    if (current.Length == 2)
                    {
                        TextWriter tw = new StreamWriter(front + "\\" + current[0]);
                        tw.WriteLine(current[1]);
                        tw.Close();
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// startTest will invoke the 
        /// </summary>
        public List<TestCase> startTest()
        {
            /*
            //initialize thefiles and folders if the test class is WebAndThumbSync
            if (testClass.GetType().Equals(typeof(WebAndThumbSync)))
            {
                string localPath = @".\GenerateLocalTestFile.txt";
                string webPath = @".\GenerateWebTestFile.txt";
                string metaDataPath = @".\GenerateMetaData.txt";

                //Read the files for each
                string[] localHash = File.ReadAllLines(localPath);
                string[] webHash = File.ReadAllLines(webPath);
                string[] metaHash = File.ReadAllLines(metaDataPath);

                //Create the test files and folders
                CreateWebThumbSyncTestFile(localHash);
                //Create and Upload web copy
                CreateWebThumbSyncTestFile(webHash);

                //Create metadata files
                CreateWebThumbSyncTestFile(metaHash);

                //create the serialize object for local for verification
                FolderOperation.SerializeMetaData(localHash,WebAndThumbSync.LocalMetaDataFileName+"Test",false);

                //create metadata of web copy and upload. for verification later
                FolderOperation.SerializeMetaData(webHash,WebAndThumbSync.WebMetaDataFileName+"Test",false);

                //create the meta data and locally and upload
                FolderOperation.SerializeMetaData(metaHash,WebAndThumbSync.LocalMetaDataFileName,false);
                for (int i = 1; 1 <= 16; ++i)
                {
                    File.Copy(@".\metaTest" + i + @"\metadata.ga", @".\localTest" + i + @"\metadata.ga");
                    File.Copy(@".\metaTest" + i + @"\metadata.ga", @".\webTest" + i + @"\metadata.web");
                }

            }
             * */
            foreach (TestCase t in testCases)
            {
                t.invokeTestMethod(/*ref testClass*/);
            } 
            return testCases;
        }

        /// <summary>
        /// Reads in and initialize the constructor parameter.
        /// </summary>
        /// <param name="arumentList"></param>
        /// <returns></returns>
        private object[] readConstructorArguments(string argumentList)
        {
            if (argumentList.Equals("void"))
                return new object[0];
            string[] param_string = argumentList.Split(new Char[] { ',' });

            //param stores all input parameters object
            object[] param = new object[param_string.Length];
            for (int i = 0; i < param.Length; ++i)
            {

                //if is arraylist data structure, create the ArrayList object
                if (param_string[i].StartsWith("List"))
                {
                    int start, end;
                    start = param_string[i].IndexOf("<");
                    end = param_string[i].IndexOf(">");
                    string type = param_string[i].Substring(start + 1, end - (start + 1));
                    ArrayList temp = PreCondition.GetArrayList(param_string[i],OfflineSync.Uninitialize);
                    switch (type)
                    {
                        case "string":
                        case "String":
                            List<string> strObj = new List<string>();
                            foreach (String s in temp)
                                strObj.Add(s);
                            param[i] = strObj;
                            break;
                        case "int":
                        case "Integer":
                            List<int> intObj = new List<int>();
                            foreach (int num in temp)
                                intObj.Add(num);
                            param[i] = intObj;
                            break;
                        case "Game":
                            List<Game> gameObj = new List<Game>();
                            foreach (Game g in temp)
                                gameObj.Add(g);
                            param[i] = gameObj;
                            break;
                        case "FileInfo":
                            List<FileInfo> fileObj = new List<FileInfo>();
                            foreach (FileInfo f in temp)
                                fileObj.Add(f);
                            param[i] = fileObj;
                            break;
                        default: break;
                    }
                    //param[i] = arraylist;
                }
                //if is array data structure, create the Array object
                //format : Array<Type>:{obj1+obj2} 
                else if (param_string[i].StartsWith("Array"))
                {
                    param[i] = PreCondition.GetArray(param_string[i], OfflineSync.Uninitialize);
                }

                // add in other data type here, queue ... ...

                else //other primitive data type
                {
                    if (param_string[i].StartsWith("Integer")) //eg. integer:100
                        param[i] = Convert.ToInt32(param_string[i].Substring(8, param_string.Length - 8));
                    else if (param_string[i].StartsWith("String")) //eg. String:"hello world"
                        param[i] = param_string[i].Substring(7, param_string.Length - 7);
                    else if (param_string[i].StartsWith("bool")) //eg. bool:true
                        param[i] = Convert.ToBoolean(param_string[i].Substring(5, param_string.Length - 5));
                    //add in other primitve data here
                    else if (param_string[i].ToLower().StartsWith("ComToWebUser"))
                    {
                        User newUser = new User();
                        newUser.Email = "TestComToWeb@gmails.com";
                        param[i] = newUser;
                    }
                    else if (param_string[i].ToLower().StartsWith("WebToComUser"))
                    {
                        User newUser = new User();
                        newUser.Email = "TestWebToCom@gmails.com";
                        param[i] = newUser;
                    }
                }
                //support for other data structure to be added        
            } //end of for loop, extracting input parameters
            return param;
        }

    }
}
