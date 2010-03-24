using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GameAnywhere
{
    class WebAndThumbSync
    {
        public Dictionary<string, int> NoConflict, Conflicts;
        private MetaData localHash, localMeta, webHash, webMeta;

        public static readonly string LocalMetaDataFileName = "metadata.ga";
        public static readonly string WebMetaDataFileName = "metadata.web";

        
        private const int UPLOAD = 1;
        private const int DOWNLOAD = 2;
        private const int DELETELOCAL = 3;
        private const int DELETEWEB = 4;
        private const int DELETEMETA = 5;
        private const int UPDOWNCONFLICT = 12;
        private const int DELETELOCALCONFLICT = 13;
        private const int DELETEWEBCONFLICT = 24;


        private static string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");
        private string LocalMetaDataPath = Path.Combine(syncFolderPath, LocalMetaDataFileName);
        private string WebMetaDataPath = Path.Combine(syncFolderPath, WebMetaDataFileName);

        private string email;

        internal MetaData WebMeta
        {
            get { return webMeta; }
            set { webMeta = value; }
        }
        internal MetaData WebHash
        {
            get { return webHash; }
            set { webHash = value; }
        }
        internal MetaData LocalMeta
        {
            get { return localMeta; }
            set { localMeta = value; }
        }
        internal MetaData LocalHash
        {
            get { return localHash; }
            set { localHash = value; }
        }

        Storage s3 = new Storage();

        public WebAndThumbSync()
        {
            NoConflict = new Dictionary<string,int>();
            Conflicts = new Dictionary<string,int>();
        }
        
        public WebAndThumbSync(User u)
        {
            email = u.Email;
            CreateMetaData(email);
            NoConflict = new Dictionary<string,int>();
            Conflicts = new Dictionary<string, int>();
        }
       
        private string GenerateHash(string path)
        {
            FileStream fs = File.Open(path, FileMode.Open);
            MD5 md5 = MD5.Create();
            string hash = BitConverter.ToString(md5.ComputeHash(fs)).Replace(@"-", @"").ToLower();
            fs.Close();
            return hash;
        }
        public void GenerateHashDictionary(string dir, Dictionary<string, string> dict)
        {
            foreach (string file in Directory.GetFiles(dir))
            {
                if (Path.GetFileName(file).Equals(WebMetaDataFileName) || Path.GetFileName(file).Equals(LocalMetaDataFileName)) continue;
                dict[file.Replace(syncFolderPath + @"\", "").Replace(@"\", "/")] = GenerateHash(file);
            }
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                GenerateHashDictionary(subdir, dict);
            }
        }

        private void CreateMetaData(string email)
        {
            localMeta = new MetaData();
            webMeta = new MetaData();
            if (File.Exists(LocalMetaDataPath))
            {
                //Create Local Metadata
                localMeta.DeSerialize(LocalMetaDataPath);
            }
            
            if (s3.ListFiles(email + '/' + WebMetaDataFileName).Count == 1)
            {
                //Create Web Metadata
                s3.DownloadFile(WebMetaDataPath, email + '/' + WebMetaDataFileName);
                webMeta.DeSerialize(WebMetaDataPath);
            }
            
            //Generate hash from web and create Metadata object
            webHash = new MetaData(s3.GetHashDictionary(email));

            //Generate hash from local and create Metadata object
            Dictionary<string, string> localDict = new Dictionary<string, string>();
            GenerateHashDictionary(syncFolderPath, localDict);
            localHash = new MetaData(localDict);
        }

        private bool HashIsDifferent(MetaData data1, MetaData data2, string key)
        {
            if (data1.GetEntryValue(key).Equals(data2.GetEntryValue(key))) return false;
            else return true;
        }
        private bool FileIsDeleted(MetaData hash, MetaData meta, string key)
        {
            if (hash.GetEntryValue(key).Equals("") && !meta.GetEntryValue(key).Equals(""))
                return true;
            else return false;
        }
        public Dictionary<string,int> CheckConflicts()
        {
            CheckConflictsHelper(localHash, localMeta, webHash, webMeta, UPLOAD);
            CheckConflictsHelper(webHash, webMeta, localHash, localMeta, DOWNLOAD);
            return FilterConflicts();
        }
        private void CheckConflictsHelper(MetaData hash1, MetaData meta1, MetaData hash2, MetaData meta2, int direction)
        {
            foreach (KeyValuePair<string, string> entry in hash1.FileTable)
            {
                if (meta1.EntryExist(entry.Key))
                {
                    //If hash1 and meta1 are different (i.e. changed)
                    if (HashIsDifferent(hash1, meta1, entry.Key))
                    {
                        //If hash2 and meta2 have a deleted file
                        if (FileIsDeleted(hash2,meta2, entry.Key))
                        {
                            if (direction == UPLOAD)
                            {
                                Conflicts[entry.Key] = DELETELOCALCONFLICT;
                            }
                            else if (direction == DOWNLOAD)
                            {
                                Conflicts[entry.Key] = DELETEWEBCONFLICT;
                            }
                        }
                        //If hash2 and meta2 are same (i.e. not changed)
                        else if (!HashIsDifferent(hash2, meta2, entry.Key))
                        {
                            if (direction == UPLOAD)
                            {
                                NoConflict[entry.Key] = UPLOAD;
                            }
                            else if (direction == DOWNLOAD)
                            {
                                NoConflict[entry.Key] = DOWNLOAD;
                            }
                        }
                        //If hash2 and meta2 are different (i.e. conflict)
                        else
                        {
                            Conflicts[entry.Key] = UPDOWNCONFLICT;
                        }
                    }
                }
                else //New file
                {
                    if (hash2.EntryExist(entry.Key)) //both new files, conflict
                        Conflicts[entry.Key] = UPDOWNCONFLICT;
                    else
                    {
                        if (direction == UPLOAD)
                            NoConflict[entry.Key] = UPLOAD;
                        else if (direction == DOWNLOAD)
                            NoConflict[entry.Key] = DOWNLOAD;
                    }
                }
            }

            foreach (KeyValuePair<string, string> entry in meta1.FileTable)
            {
                if (!hash1.EntryExist(entry.Key)) //Deleted file
                {
                    if (!hash2.EntryExist(entry.Key)) //both deleted, delete metadata
                        NoConflict[entry.Key] = DELETEMETA;
                    else if (Conflicts.ContainsKey(entry.Key))
                        continue;
                    else if (HashIsDifferent(hash2, meta2, entry.Key))
                    {
                        if (direction == UPLOAD)
                        {
                            Conflicts[entry.Key] = DELETEWEBCONFLICT;
                        }
                        if (direction == DOWNLOAD)
                        {
                            Conflicts[entry.Key] = DOWNLOAD;
                        }
                    }
                    else
                    {
                        if (direction == UPLOAD) //Delete from web
                        {
                            NoConflict[entry.Key] = DELETEWEB;
                        }
                        if (direction == DOWNLOAD)//Delete from thumb
                        {
                            NoConflict[entry.Key] = DELETELOCAL;
                        }
                    }
                }

            }
        }

        public List<SyncError> SynchronizeGames(Dictionary<string, int> resolvedConflicts)
        {
            string processName = "Sync files from Web and Thumb";
            List<SyncError> errorList = new List<SyncError>();

            //Merge the resolved conflicts
            MergeConflicts(resolvedConflicts);

            //Web operations and metadata object update
            foreach (string key in NoConflict.Keys)
            {
                string localPath = Path.Combine(syncFolderPath, key);
                string webPath = email + "/" + key;

                try
                {
                    switch (NoConflict[key])
                    {
                        case UPLOAD:
                            s3.UploadFile(localPath, webPath);
                            UpdateMetaData(key, GenerateHash(localPath));
                            break;
                        case DOWNLOAD:
                            //Create the target directory if needed
                            //Console.WriteLine(Path.GetDirectoryName(localDirName));
                            if (!Directory.Exists(Path.GetDirectoryName(localDirName)))
                                    CreateDirectory(Path.GetDirectoryName(Path.Combine(targetPath, webDirName)));

                            s3.DownloadFile(localPath, webPath);
                            UpdateMetaData(key, GenerateHash(localPath));
                            break;
                        case DELETELOCAL:
                            File.Delete(localPath);
                            DeleteMetaData(key);
                            break;
                        case DELETEWEB:
                            s3.DeleteDirectory(webPath);
                            DeleteMetaData(key);
                            break;
                        case DELETEMETA:
                            DeleteMetaData(key);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string gameName = key.Substring(0, key.IndexOf('/'));
                    errorList.AddRange(GetSyncError(gameName,processName,ex.Message));
                }
            }
            //Serialize and Upload Metadata
            localHash.Serialize(LocalMetaDataPath);
            webHash.Serialize(WebMetaDataPath);
            s3.UploadFile(WebMetaDataPath,email + "/" + WebMetaDataFileName);
            File.Delete(WebMetaDataPath);
            return errorList;
        }

        public Dictionary<string,int> FilterConflicts()
        {
            Dictionary<string, int> filteredConflicts = new Dictionary<string, int>();
            foreach (string key in Conflicts.Keys)
            {
                filteredConflicts[GetGamesAndTypes(key)] = 0;
            }
            return filteredConflicts;
        }
        public void MergeConflicts(Dictionary<string, int> resolvedConflicts)
        {
            foreach (string key in Conflicts.Keys)
            {
                int side = resolvedConflicts[GetGamesAndTypes(key)];
                switch (Conflicts[key])
                {
                    case 13:
                        if (side == 1) NoConflict[key] = 1;
                        else if (side == 2) NoConflict[key] = 3;
                        break;
                    case 14:
                        if (side == 1) NoConflict[key] = 1;
                        else if (side == 2) NoConflict[key] = 4;
                        break;
                    case 24:
                        if (side == 1) NoConflict[key] = 2;
                        else if (side == 2) NoConflict[key] = 4;
                        break;
                }
            }
        }

        private string GetGamesAndTypes(string key)
        {
            string game = key.Substring(0, key.IndexOf('/'));
            string type = key.Substring(key.IndexOf('/') + 1);
            return game + "/" + type.Substring(0, type.IndexOf('/'));
        }

        private void UpdateMetaData(string key, string value)
        {
            localMeta.UpdateEntryValue(key,value);
            localHash.UpdateEntryValue(key, value);
            webMeta.UpdateEntryValue(key, value);
            webHash.UpdateEntryValue(key, value);

        }
        private void AddMetaData(string key, string value)
        {
            localMeta.AddEntry(key, value);
            localHash.AddEntry(key, value);
            webMeta.AddEntry(key, value);
            webHash.AddEntry(key, value);
        }
        private void DeleteMetaData(string key)
        {
            localMeta.DeleteEntry(key);
            localHash.DeleteEntry(key);
            webMeta.DeleteEntry(key);
            webHash.DeleteEntry(key);
        }

        private List<SyncError> GetSyncError(string errorPath, string processName, string errorMessage)
        {
            List<SyncError> errorList = new List<SyncError>();
            //Debug.WriteLine(errorMessage);

            if (File.Exists(errorPath)) //Path is a file
                errorList.Add(new SyncError(errorPath, processName, errorMessage));
            else //Path is a folder
            {
                if (Directory.Exists(errorPath))
                {
                    /*//Add all files in the folder
                    foreach (string errorFile in Directory.GetFiles(errorPath))
                        errorList.Add(new SyncError(errorPath, processName, errorMessage));
                    //Add all subfolders in the folder
                    foreach (string subFolder in Directory.GetDirectories(errorPath))
                        errorList.AddRange(GetSyncError(subFolder, processName, errorMessage));*/

                    errorList.Add(new SyncError(errorPath, processName, errorMessage));
                }
                else
                    errorList.Add(new SyncError(errorPath, processName, errorMessage));

            }
            return errorList;
        }
    }
}
