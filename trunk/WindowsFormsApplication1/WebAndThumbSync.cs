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

        private const int CONFLICT = 0;
        private const int UPLOAD = 1;
        private const int DOWNLOAD = 2;
        private const int DELETELOCAL = 3;
        private const int DELETEWEB = 4;

        private string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");

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
        /*
        WebAndThumbSync(string email)
        {
            CreateMetaData(email);
            NoConflict = new Dictionary<string,int>();
            Conflicts = new Dictionary<string, int>();
        }
        */
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
            string LocalMetaDataPath = Path.Combine(syncFolderPath, LocalMetaDataFileName);
            string WebMetaDataPath = Path.Combine(syncFolderPath, WebMetaDataFileName);

            //Create Local Metadata
            localMeta.DeSerialize(LocalMetaDataFileName);

            //Create Web Metadata
            s3.DownloadFile(WebMetaDataFileName, email + '/' + WebMetaDataFileName);
            webMeta.DeSerialize(WebMetaDataFileName);

            //Generate hash from web and create Metadata object
            MetaData webHash = new MetaData(s3.GetHashDictionary(email));

            //Generate hash from local and create Metadata object
            Dictionary<string, string> localDict = new Dictionary<string, string>();
            GenerateHashDictionary(syncFolderPath, localDict);
            MetaData localHash = new MetaData(localDict);


        }

        private bool HashIsDifferent(MetaData data1, MetaData data2, string key)
        {
            if (data1.GetEntryValue(key).Equals(data2.GetEntryValue(key))) return false;
            else return true;
        }

        public Dictionary<string,int> CheckConflicts()
        {
            CheckConflictsHelper(localHash, localMeta, webHash, webMeta, UPLOAD);
            CheckConflictsHelper(webHash, webMeta, localHash, localMeta, DOWNLOAD);
            return Conflicts;
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
                        //If hash2 and meta2 are same (i.e. not changed)
                        if (!HashIsDifferent(hash2, meta2, entry.Key))
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
                            Conflicts[entry.Key] = CONFLICT;
                        }
                    }
                }
                else //New file
                {
                    if (hash2.EntryExist(entry.Key)) //both new files, conflict
                        Conflicts[entry.Key] = CONFLICT;
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
                        DeleteMetaData(entry.Key);
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


    }
}
