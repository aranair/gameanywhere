using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using System.Reflection;
using System.Diagnostics;


//Synchronize, restore, game name to be input

namespace GameAnywhere
{
    /// <summary>
    /// 
    /// The purpose of Test Case is to store the method to test, its input, expected output,
    /// expected execption to be thrown, and the result
    /// 
    /// example of test case format which is stored in the file :-
    /// [method name]|[input parameters]|[Expected Output]|[Exceptions which may be thrown]
    /// 
    /// </summary>
    class TestCase
    {

        private object testClass;
        private string methodName;
        private string testCase;
        private int index; //test case index
        private object[] input;
        private object output;
        private object expectedOutput;
        private string exceptionThrown;
        TestResult testOutcome;
        private string externalPath = @".\SyncFolder";
        private List<FileInfo> deletedFile; //used for testing restore function

        #region Properties
        internal TestResult TestOutcome
        {
            get { return testOutcome; }
            set { testOutcome = value; }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public string MethodName
        {
            get { return methodName; }
            set { methodName = value; }

        }
        public string ExceptionThrown
        {
            get { return exceptionThrown; }
            set { exceptionThrown = value; }
        }

        public object[] Input
        {
            get { return input; }
            set { input = value; }
        }

        public object Output
        {
            get { return output; }
            set { output = value; }
        }
        #endregion

        /// <summary>
        /// Constructor takes in _testcase_ : string
        /// Then it will intialize the intput & expectedOutput.
        /// </summary>
        /// <param name="_testcase_"></param>
        public TestCase(string testCaseDescription, object classType)
        {
            
            this.testCase = testCaseDescription; 
            this.testClass = classType;
            deletedFile = new List<FileInfo>();
            InitializeTestCase();
        }

        

        /// <summary>
        /// InitializeTestCase will process the string testcase,
        /// read in the inputs and expected output
        /// </summary>
        /// <returns></returns>
        private void InitializeTestCase()
        {
            string[] temp = testCase.Split(new Char[] { '~' });
            index = Convert.ToInt32(temp[0]); //get the index of the test case
            //format = [method name]|[input parameters]|[Expected Output]|[Exceptions which may be thrown]
            string[] testCaseComponent = temp[1].Split(new Char[] { '|' });

            // 1.   get method name
            this.methodName = testCaseComponent[0];

            // 2.   stores all the parameters to be split
            string parameters = testCaseComponent[1];
            GetParameters(parameters);

            // 3.   get the expected output, the return type
            string expected = testCaseComponent[2];
            if(methodName.Equals("SynchronizeGames"))
            {
                if (testClass.GetType().Equals(typeof(OfflineSync)))
                {
                    OfflineSync offline = (OfflineSync)testClass;
                    GetExpectedOutput(expected, offline.SyncDirection);
                }
                else if(testClass.GetType().Equals(typeof(OnlineSync)))
                {
                    OnlineSync online = (OnlineSync)testClass;
                    GetExpectedOutput(expected, online.SyncDirection);
                }
            }
            else
                GetExpectedOutput(expected, 0);

            // 4.   get the expected exceptions
            // Expception format: [FileNotFoundExpception("some messages"),DirectoryNotFoundException("Some messages")]
            this.exceptionThrown = testCaseComponent[3];

        } //end initTestCase

        /// <summary>
        /// if method invoked returned null (something went wrong with execution)
        /// set the error message
        /// </summary>
        /// <param name="invalidReturn"></param>
        /// <returns></returns>
        private TestResult getInvalidError(Exception err)
        {
            MessageBox.Show("Type: "+err.GetType().Name+
                               "\nInnerType: "+err.InnerException.GetType().Name+
                               "\nMessage: "+err.Message);
            TestResult result = new TestResult(false);
            result.AddRemarks(err.Message);
            result.AddRemarks(err.GetType().ToString());
            if(err.InnerException != null)
                result.AddRemarks(err.InnerException.ToString());
            return result;
        }
    
        /// <summary>
        /// Try and excute each test cases and get its results
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public TestResult invokeTestMethod(/*ref object testClass*/)
        {
            object returnType = null;
            TestResult result = null;
            bool error = false;
            //set the precondition for the specific test case
            PreCondition.SetPreCondition(index, methodName, ref input, ref testClass);

            //try and run the test case
            Assembly assembly = Assembly.GetExecutingAssembly(); 
            Type classType = null;
            if (testClass != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Pick up a class
                    if (type.IsClass == true && type.Name.Equals(testClass.GetType().Name))
                        classType = type;
                }
            }

            if (classType != null)
            {
                try
                {
                    object[] methodInput;
                    if (methodName.Equals("Restore"))
                        methodInput = new object[0];
                    else
                        methodInput = input;

                    returnType = classType.InvokeMember(methodName,
                                                 BindingFlags.Default | BindingFlags.InvokeMethod,
                                                 null,
                                                 testClass,
                                                 methodInput);
                }
                catch (Exception err)
                {
                    //check for test case number and match if correct exception is thrown
                    switch (methodName)
                    {
                        case "RefreshList":
                            MessageBox.Show(err.StackTrace.ToString());
                            break;
                        case "GetGameList":
                                error = true;
                                result = getInvalidError(err);
                            break;
                        case "SynchronizeGames":
                            if (err.GetBaseException().GetType().Equals(typeof(DeleteDirectoryErrorException)) ||
                                err.GetBaseException().GetType().Equals(typeof(CreateFolderFailedException))||
                                err.GetBaseException().GetType().Equals(typeof(ConnectionFailureException)))
                                returnType = err;
                            else
                            {
                                error = true;
                                result = getInvalidError(err);
                            }
                            break;
                        case "Restore":
                            if (err.GetBaseException().GetType().Equals(typeof(DeleteDirectoryErrorException))||
                                err.GetBaseException().GetType().Equals(typeof(CreateFolderFailedException)))
                                returnType = err;
                            else
                            {
                                error = true;
                                result = getInvalidError(err);
                            }
                            break;
                        case "ResendActivation":
                        case "ChangePassword":
                        case "RetrievePassword":
                        case "Register":
                        case "Login":
                            if (err.GetBaseException().GetType().Equals(typeof(System.Net.WebException)) ||
                                err.GetBaseException().GetType().Equals(typeof(ArgumentException)))
                                returnType = err;
                            else
                            {
                                error = true;
                                result = getInvalidError(err);
                            }
                            break;
                        case "CheckConflict":
                            MessageBox.Show(err.Message);
                            break;
                        //add more methods and exceptions here
                        default: 
                            break;
                    }
                }
                //finally verify and return the result accordingly.
                finally
                {
                    if (!error)
                        result = CheckCondition(methodName, testClass, returnType);
                    else
                    {
                        //null is being returned, faulty
                        MessageBox.Show(methodName+ " "+index+ " is return is null");
                    }

                    //clean up / Restore the test file/folders or orginal files/folders
                    PreCondition.CleanUp(testClass,methodName,index);
                }
                testOutcome = result;
                return result;
            }
            else
            {           
                //throw new Exception("ClassNotFound!");
                result = new TestResult(false);
                result.AddRemarks("Failed! Fail to initialize test class!");
                testOutcome = result;
                return result;
            }
        }

        /// <summary>
        /// checkCondition verify the results for each test cases
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="returnType"></param>
        /// <param name="testCaseIndex"></param>
        /// <returns></returns>
        private TestResult CheckCondition(string methodName, object testClass, object returnType)
        {
            TestResult result = null;
            switch (methodName)
            {
                case "RefreshList":
                case "GetGameList":
                    result = Verifier.VerifyGetGameList(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                    //add more methods here
                case "SynchronizeGames":
                    if(testClass.GetType().Equals(typeof(OfflineSync)))
                        result = OfflineSyncVerifier.VerifySynchronizeGames(index, returnType, testClass, exceptionThrown, expectedOutput);
                    else if (testClass.GetType().Equals(typeof(OnlineSync)))
                        result = Verifier.VerifyOnlineSync(index, returnType, testClass, exceptionThrown, expectedOutput);
                    else if (testClass.GetType().Equals(typeof(WebAndThumbSync)))
                        result = WebThumbVerifier.VerifyWebThumbSync(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                case "Restore":
                    result = OfflineSyncVerifier.VerifyRestore(index,input, returnType, testClass, exceptionThrown, expectedOutput, deletedFile);
                    break;
                case "Login":
                    result = Verifier.VerifyLogin(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                case "RetrievePassword":
                case "Register":
                    result = Verifier.VerifyRegisterRetrievePassword(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                case "ChangePassword":
                    result = Verifier.VerifyChangePassword(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                case "ResendActivation":
                    result = Verifier.VerifyResendActivation(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
                case "CheckConflicts":
                    result = WebThumbVerifier.VerifyCheckConflicts(index, returnType, testClass, exceptionThrown, expectedOutput);
                    break;
            }
            return result; //return test result
        }
        
        /// <summary>
        /// Get and set the expected output based in the input
        /// </summary>
        /// <param name="expected"></param>
        private void GetExpectedOutput(string expected, int direction)
        {
            if (expected.Equals("void"))
            {
                this.expectedOutput = null ;
                return;
            }
            string[] temp = expected.Split(new Char[] { ':' });
            //format [type]:[value/list of values]
            if (expected.StartsWith("int") || expected.StartsWith("Integer"))
            {
                expectedOutput = Convert.ToInt32(temp[1]);
            }
            else if (expected.StartsWith("string") || expected.StartsWith("String"))
            {
                if (temp[1].Equals("void"))
                    expectedOutput = "";
                else
                    expectedOutput = temp[1];
            }
            else if (expected.StartsWith("bool") || expected.StartsWith("boolean") || expected.StartsWith("Boolean"))
            {
                expectedOutput = Convert.ToBoolean(temp[1]);
            }
            else if (expected.StartsWith("List"))
            {
                int start, end;
                start = expected.IndexOf("<");
                end = expected.IndexOf(">");
                string type = expected.Substring(start + 1, end - (start + 1));
                ArrayList expectedList = PreCondition.GetArrayList(expected,direction);
                switch (type)
                {
                    case "string":
                    case "String":
                        List<string> strObj = new List<string>();
                        foreach (String s in expectedList)
                            strObj.Add(s);
                        expectedOutput = strObj;
                        break;
                    case "int":
                    case "Integer":
                        List<int> intObj = new List<int>();
                        foreach (int num in expectedList)
                            intObj.Add(num);
                        expectedOutput = intObj;
                        break;
                    case "Game":
                        List<Game> gameObj = new List<Game>();
                        foreach (Game g in expectedList)
                            gameObj.Add(g);
                        expectedOutput = gameObj;
                        break;
                    case "FileInfo":
                        List<FileInfo> fileObj = new List<FileInfo>();
                        foreach (FileInfo f in expectedList)
                            fileObj.Add(f);
                        expectedOutput = fileObj;
                        break;
                    default: break;
                }
            }
            else if (expected.StartsWith("array") || expected.StartsWith("Array"))
            {
                expectedOutput = PreCondition.GetArray(expected,direction);
            }
            //to add in more data types eg. Array
        }

        /// <summary>
        /// GetParameters get the paramter of the testcase
        /// </summary>
        /// <param name="split"></param>
        /// <param name="parameters"></param>
        private void GetParameters(string parameters)
        {
            if (parameters.Equals("void"))
            {
                this.input = new object[0]; //create an empty array
                return;
            }
            string[] param_string = parameters.Split(new Char[] { ',' });
            
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
                    ArrayList temp = null;
                    if (testClass.GetType().Equals(typeof(OfflineSync)))
                    {
                        OfflineSync offline = (OfflineSync)testClass;
                        temp = PreCondition.GetArrayList(param_string[i], offline.SyncDirection);
                    }
                    else if (testClass.GetType().Equals(typeof(OnlineSync)))
                    {
                        OnlineSync online = (OnlineSync)testClass;
                        temp = PreCondition.GetArrayList(param_string[i], online.SyncDirection);
                    }
                    else
                        temp = PreCondition.GetArrayList(param_string[i], 0);
                    switch (type)
                    {
                        //convert the arraylist to list
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
                        case "SyncAction":
                            List<SyncAction> syncObj = new List<SyncAction>();
                            foreach (SyncAction s in temp)
                                syncObj.Add(s);
                            param[i] = syncObj;
                            break;
                        default: break;
                    }
                }
                //if is array data structure, create the Array object
                //format : Array<Type>:{obj1,obj2} 
                else if (param_string[i].StartsWith("Array"))
                {
                    OfflineSync offline = (OfflineSync)testClass;
                    param[i] = PreCondition.GetArray(param_string[i],offline.SyncDirection);   
                }
                else //other primitive data type
                {
                    if (param_string[i].StartsWith("Integer")) //eg. Integer:100
                        param[i] = Convert.ToInt32(param_string[i].Substring(8, param_string[i].Length - 8));
                    else if (param_string[i].StartsWith("int")) //int:1
                        param[i] = Convert.ToInt32(param_string[i].Substring(4, param_string[i].Length - 4));
                    else if (param_string[i].StartsWith("String") || param_string[i].StartsWith("string")) //eg. String:"hello world"
                    {
                        if (param_string[i].Substring(7, param_string[i].Length - 7).Equals("void"))
                            param[i] = "";
                        else
                            param[i] = param_string[i].Substring(7, param_string[i].Length - 7);
                    }
                    else if (param_string[i].StartsWith("bool")) //eg. bool:true
                        param[i] = Convert.ToBoolean(param_string[i].Substring(5, param_string[i].Length - 5));
                    else if (param_string[i].StartsWith("Dictionary"))
                    {
                        param[i] = GetDictionaryParam(param_string[i]);
                    }
                    //add in other primitve data here
                    else if (param_string[i].ToLower().StartsWith("comtowebuser"))
                    {
                        User newUser = new User();
                        newUser.Email = "TestComToWeb@gmails.com";
                        param[i] = newUser;
                    }
                    else if (param_string[i].ToLower().StartsWith("webtocomuser"))
                    {
                        if (testClass.GetType().Equals(typeof(GameLibrary)))
                        {
                            param[i] = OnlineSync.GetGamesAndTypesFromWeb("TestWebToCom@gmails.com");
                        }
                        else
                        {
                            User newUser = new User();
                            newUser.Email = "TestWebToCom@gmails.com";
                            param[i] = newUser;
                        }
                    }
                }
                //support for other data structure to be added        
            } //end of for loop, extracting input parameters

            this.input = param; //set the input
        }

        public static Dictionary<string,int> GetDictionaryParam(string param_string)
        {
            Dictionary<string,int> resolvedConflicts = new Dictionary<string,int>();
            int start, end;
            start = param_string.IndexOf("{");
            end = param_string.IndexOf("}");
            string dictionaryKeys = param_string.Substring(start + 1, end - (start + 1));

            //if empty Dictionary
            if (dictionaryKeys.Equals("void"))
                return resolvedConflicts;
            string[] keyEntry = dictionaryKeys.Split(new Char[] {'+'});

            foreach (string key in keyEntry)
            {
                string[] keyValues = key.Split(new Char[] {';'});
                string entryName = keyValues[0];
                string action = keyValues[1];
                resolvedConflicts.Add(entryName, Convert.ToInt32(action));
            }
            return resolvedConflicts;
        }

        // test reading in of input and storing the data
        public string getTestCaseProperties()
        {
            string ret = "";
            //ret += "Class Name : " + testClass.ToString() + "\n";
            ret += "Method Name : " + methodName + "\n";
            ret += "Inputs : \n";
            foreach (object o in input)
                ret += o.ToString() + "\n";
            ret += "Expected Outputs :\n";
            if(expectedOutput != null)
                ret += expectedOutput.ToString() + "\n";
            if(exceptionThrown != null)
            {
                ret += "Expected Exceptions :\n";
                foreach (object o in exceptionThrown)
                    ret += o.ToString() + "\n";
            }
            return ret;
        }

    }
}
