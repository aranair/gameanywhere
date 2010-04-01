﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Security.AccessControl;

using System.Security.Cryptography;

namespace GameAnywhere
{
    /// <summary>
    /// FolderOperation contains the methods which are required for any folder operations.
    /// They include: File security modification, copydirectory, folder compare
    /// </summary>
    class FolderOperation
    {
        private string sourcePath;

        #region properties
        public string SourcePath
        {
          get { return sourcePath; }
          set { sourcePath = value; }
        }
        private string targetPath;

        public string TargetPath
        {
          get { return targetPath; }
          set { targetPath = value; }
        }
        #endregion

        public FolderOperation(string source, string target)
        {
            sourcePath = source; targetPath = target;
        }
        private static string external = @".\SyncFolder";
        public static readonly int CONFIG=2, SAVE =1,BOTH=3, ExtToCom = 4, ComToExt = 5;
        /// <summary>
        /// copies the original settings to test back up folder to check created backup folder
        /// settings: FolderOperation.CONFIG/SAVE
        /// </summary>
        /// <param name="action"></param>
        /// <param name="testbackup"></param>
        /// <param name="settings"></param>
        public static void CopyOriginalSettings(List<SyncAction> synclist, int settings, int direction)
        {
            GameLibrary library = new GameLibrary();
            List<Game> gameList = library.GetGameList(OfflineSync.ComToExternal);
            //settings 1 - save, 2 - config, 3 - both
            if (settings != 1) // for settings 2 & 3
            {
                foreach (SyncAction action in synclist)
                {
                    string testbackup = "";
                    if (direction == ExtToCom)
                        testbackup = Path.Combine(action.MyGame.ConfigParentPath, "ConfigTestBackup");
                    else
                        testbackup = Path.Combine(external, "ConfigTestBackup");
                    Directory.CreateDirectory(testbackup);
                    List<string> list = null;
                    foreach(Game g in gameList)
                    {
                        if (g.Name.Equals(action.MyGame.Name))
                            list = g.ConfigPathList;
                    }
                    foreach (string s in list)
                    {
                        if (File.Exists(s))
                            File.Copy(s, testbackup+@"\"+Path.GetFileName(s));
                        else
                        {                
                            if(Directory.Exists(s))
                                FolderOperation.CopyDirectory(s, Path.Combine(testbackup, Path.GetFileName(s)));
                        }
                    }
                }
            }
            if(settings  != 2) //for setting 1 & 3
            {
                foreach (SyncAction action in synclist)
                {
                    string testbackup = "";
                    if (direction == ExtToCom)
                        testbackup = Path.Combine(action.MyGame.SaveParentPath, "SaveTestBackup");
                    else
                        testbackup = Path.Combine(external, "SaveTestBackup");
                    Directory.CreateDirectory(testbackup);
                    List<string> list = null;
                    foreach (Game g in gameList)
                    {
                        if (g.Name.Equals(action.MyGame.Name))
                            list = g.SavePathList;
                    }
                    foreach (string s in list)
                    {
                        if (File.Exists(s))
                            File.Copy(s, testbackup + @"\" + Path.GetFileName(s));
                        else
                        {
                            if (Directory.Exists(s))
                                FolderOperation.CopyDirectory(s, Path.Combine(testbackup, Path.GetFileName(s)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// copies the files and folders from source path to destination path
        /// modified from msdn website
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            //string folder = Path.Combine(destDirName,Path.GetFileName(sourceDirName));
            // If the destination directory does not exist, create it.
            //string newFolder = Path.Combine(destDirName, Path.GetFileName(sourceDirName));
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            // Get the file contents of the directory to copy.
            CopyFilesInDirectory(sourceDirName, destDirName);

            //copy the subdirectories.
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);
                // Copy the subdirectories.
                if(!subdir.Name.Equals("SaveTestBackup") || !subdir.Name.Equals("ConfigTestBackup"))
                    CopyDirectory(subdir.FullName, temppath);
            }
        }

        /// <summary>
        /// copy all the files of the directory from source to destination
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private static void CopyFilesInDirectory(string source, string dest)
        {
            string[] files = Directory.GetFiles(source);
            foreach (string s in files)
            {
                File.Copy(s,Path.Combine(dest,Path.GetFileName(s)),true);
            }
        }

        /// <summary>
        /// return the string of the Md5 hash code
        /// Codes from tutorial site: http://sharpertutorials.com/calculate-md5-checksum-file/
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return the list of FileInfo which are not found in the target directory
        /// direction - specifies 1-way(what is in source is in target) or 2-way check(both must be similar)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static List<string> FindDifferences(string source, string target, bool direction)
        {
            List<string> differences = new List<string>();

            if (!Directory.Exists(source) || !Directory.Exists(target))
            {
                throw new Exception("Non-directory specified in the parameter!");
            }

            DirectoryInfo sourceDir = new DirectoryInfo(source);
            DirectoryInfo targetDir = new DirectoryInfo(target);
            DirectoryInfo[] sourceSubDirs = sourceDir.GetDirectories();

            // Get the file contents of the directory to copy.
            FileInfo[] filesInSource = sourceDir.GetFiles();
            FileInfo[] filesInTarget = targetDir.GetFiles();
            string[] fileNameInTarget = new string[filesInTarget.Length];
            for(int i=0; i<filesInTarget.Length; ++i)
                fileNameInTarget[i] = filesInTarget[i].FullName;
            List<string> targetList = new List<string>(fileNameInTarget);

            //Check all the files in current directory
            foreach (FileInfo file in filesInSource)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(target, file.Name);
                
                if (File.Exists(temppath))
                {
                    //get md5 hashcode for both files
                    if(! GetMD5HashFromFile(file.FullName).Equals(GetMD5HashFromFile(temppath)))
                        differences.Add("File: " + temppath + " ,is different.");
                }
                else
                {
                    differences.Add("File: "+temppath + " ,is missing in target: "+target);
                }
            }

            if(direction == true)
                foreach (string path in targetList)
                    differences.Add("File: "+path+" ,is not in source folder: "+source);

            //copy the subdirectories.
            foreach (DirectoryInfo subdir in sourceSubDirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(target, subdir.Name);
                // Check if the sub directory exist in target
                if (Directory.Exists(temppath))
                {
                    List<string> diff = FindDifferences(subdir.FullName, temppath, direction);
                    differences.AddRange(diff);
                }
                else
                {
                    differences.Add("Folder: "+temppath+" ,is missing in target folder.");
                }
                
            }    
            return differences;
        }

        /// <summary>
        /// Get the number of files in the directory given the path of the directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int GetDirectoryFileCount(string path)
        {
            int fileCount = 0;
            //get all the directories and files inside a directory
            String[] allFiles = Directory.GetFileSystemEntries(path);
            //loop through all items
            foreach (string file in allFiles)
            {
                //check to see if the file is a directory if not increment the count
                if (Directory.Exists(file))
                {
                    //recursive call
                    GetDirectoryFileCount(file);
                }
                else
                {
                    //increment file count
                    fileCount++;
                }
            }
            return fileCount;
        }

        /// <summary>
        /// Conatins return true if the file is found in the directory and they are the same.
        /// return false if the file is not found or not the same.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool Contains(string directory, string file /*the full path with filename*/)
        {
            DirectoryInfo folder = new DirectoryInfo(directory);
            FileInfo[] fileInFolder = folder.GetFiles();
            bool found = false;

            foreach (FileInfo info in fileInFolder)
            {
                if (info.FullName.Equals(file))
                {
                    found = true;
                    FileInfo f = new FileInfo(file);
                    if (!info.GetHashCode().Equals(f.GetHashCode()))
                        found = false;
                    break;
                }
            }
            return found;
        }

        
        // Adds an Access Control entry on the specified file for the specified account.
        /// <summary>
        /// Codes from Microsoft MSDN library,
        /// To deny copying of files, 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="account"></param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        public static void AddFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        // Removes an ACL entry on the specified file for the specified account.
        public static void RemoveFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

    }
}