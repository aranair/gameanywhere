using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Reflection;
using System.Collections;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

using System.Management;

namespace GameAnywhere
{
    public partial class Tester : Form
    {

        public Tester()
        {
            InitializeComponent();
        }

        private object[] readConstructorArguments(string argumentList)
        {
            string[] param_string = argumentList.Split(new Char[] { ',' });

            //param stores all input parameters object
            object[] param = new object[param_string.Length];
            for (int i = 0; i < param.Length; ++i)
            {

                //if is arraylist data structure, create the ArrayList object
                if (param_string[i].StartsWith("ArrayList"))
                {
                    param[i] = GetArrayList(param_string[i]);
                    //param[i] = arraylist;
                }
                //if is array data structure, create the Array object
                //format : Array<Type>:{obj1,obj2} 
                else if (param_string[i].StartsWith("Array"))
                {
                    param[i] = GetArray(param_string[i]);
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
                    //add in other primitve data here
                }
                //support for other data structure to be added        
            } //end of for loop, extracting input parameters
            return param;
        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            string user = WindowsIdentity.GetCurrent().Name;
            //resume external
            string externalPath = @".\SyncFolder";
            FolderOperation.RemoveFileSecurity(externalPath + @"\" + "FIFA 10", user,
                FileSystemRights.FullControl, AccessControlType.Deny);
            FolderOperation.AddFileSecurity(externalPath + @"\" + "FIFA 10", user,
                FileSystemRights.FullControl, AccessControlType.Allow);
            //resume game folder
            FolderOperation.RemoveFileSecurity(@"C:\Documents and Settings\Administrator\My Documents\FIFA 10", user,
                FileSystemRights.FullControl, AccessControlType.Deny);
            FolderOperation.AddFileSecurity(@"C:\Documents and Settings\Administrator\My Documents\FIFA 10", user,
                FileSystemRights.FullControl, AccessControlType.Allow);
            //FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game1\savedGame", user, FileSystemRights.FullControl, AccessControlType.Deny);
            //FolderOperation.RemoveFileSecurity(@".\SyncFolder\Game2\config", user, FileSystemRights.CreateFiles, AccessControlType.Deny);
            Game fifa = PreCondition.getGame("FIFA 10",0);
            FolderOperation.RemoveFileSecurity(fifa.SaveParentPath, user, FileSystemRights.CreateDirectories, AccessControlType.Deny);
            //MessageBox.Show("Stop!");
            //Move any existing SyncFolder
            /*if (Directory.Exists(@".\SyncFolder"))
                Directory.Move(@".\SyncFolder", @".\SyncFolder-backup");
            Directory.Move(@".\Syncfolder-test", @"SyncFolder");*/

            TestDriver test = null;
            if (testCaseSource.Text.EndsWith(".tcf"))
                test = new TestDriver(testCaseSource.Text);
            else
            {
                MessageBox.Show("Please select a test file.");
                return;
            }
            commentsText.Text = "Please wait...testing in progress...";
            List<TestCase> result = test.startTest();
            displayResult.Clear();
            commentsText.Text = "Done!";
            foreach (TestCase cases in result)
            {
                displayResult.AppendText(cases.Index+") "+cases.MethodName + " : " + cases.TestOutcome.Result + " \nRemarks : " + cases.TestOutcome.Remarks);
                displayResult.AppendText("\n");
            }



            //MessageBox.Show("Restore original SyncFolder now?");

            /*Directory.Move(@"SyncFolder", @".\Syncfolder-test");
            //restore the syncfolder if it is backup
            if (Directory.Exists(@".\SyncFolder-backup"))
                Directory.Move(@".\SyncFolder-backup", @".\SyncFolder");*/
            
         }

        /// <summary>
        /// Array is Not in used in the project
        /// Create the array object from the param in the string given
        /// format : Array<Type>:{obj1+obj2}
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private object[] GetArray(string param)
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
                        returnObject[j] = Convert.ToInt32(list[j]);
                    return returnObject;

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
        /// Create the ArrayList object from the param in string given
        /// parameter ArrayList:{obj1+obj2}
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private ArrayList GetArrayList(string param)
        {
            ArrayList arrlist = null;
            //get the type
            int s, e;
            s = param.IndexOf("<");
            e = param.IndexOf(">");
            string type = param.Substring(s + 1, e - (s + 1));
            string temp = param.Substring(11, param.Length - 12);
            string[] list = temp.Split(new Char[] { ',' });
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

                //format = Array<Game>:{(constructor info),(L4D2,c:\prog\L4D2)}
                case "FileInfo":
                    FileInfo[] arr2 = new FileInfo[list.Length];
                    for (int j = 0; j < list.Length; ++j)
                        arr2[j] = new FileInfo(list[j].Substring(1, list[j].Length - 2));
                    arrlist = new ArrayList(arr2);
                    break;
                default: break;
            }
            return arrlist;
        }//end of getArrayList 

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                if (File.Exists(path))
                    this.testCaseSource.Text = path;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
