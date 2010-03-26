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
        /// <summary>
        /// Data Members
        /// </summary>
        public Dictionary<string, int> NoConflict, Conflicts;
        private MetaData localHash, localMeta, webHash, webMeta;
        private string email;
        private Storage s3 = new Storage();

        public static readonly string LocalMetaDataFileName = "metadata.ga";
        public static readonly string WebMetaDataFileName = "metadata.web";
        private static string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");
        private string LocalMetaDataPath = Path.Combine(syncFolderPath, LocalMetaDataFileName);
        private string WebMetaDataPath = Path.Combine(syncFolderPath, WebMetaDataFileName);

        private const int UPLOAD = 1;
        private const int DOWNLOAD = 2;
        private const int DELETELOCAL = 3;
        private const int DELETEWEB = 4;
        private const int DELETEMETA = 5;
        private const int UPDOWNCONFLICT = 12;
        private const int DELETELOCALCONFLICT = 13;
        private const int DELETEWEBCONFLICT = 24;
        
        /// <summary>
        /// Accessors & Mutators
        /// </summary>
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

        /// <summary>
        /// Constructors
        /// </summary>
        public WebAndThumbSync()
        {
            NoConflict = new Dictionary<string,int>();
            Conflicts = new Dictionary<string,int>();
        }
        
        public WebAndThumbSync(User u)
        {
            email = u.Email;
            NoConflict = new Dictionary<string,int>();
            Conflicts = new Dictionary<string, int>();
            try
            {
                CreateMetaData(email);
            }
            catch
            {
                //Exceptions: ArgumentException, ConnectionFailureException, WebTransferException, IOException, UnauthorizedAccessException
                throw;
            }
        }
       
        /// <summary>
        /// Generate the hashcode of a file
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>hashcode of file</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private string GenerateHash(string path)
        {
            //Pre-conditions
            if (path.Equals("") || path == null)
                throw new ArgumentException("Parameter cannot be empty/null", "path");

            FileStream fs = null;
            string hash = "";

            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.Read);
                MD5 md5 = MD5.Create();
                hash = BitConverter.ToString(md5.ComputeHash(fs)).Replace(@"-", @"").ToLower();
            }
            catch (IOException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch(Exception)
            {
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return hash;
        }

        /// <summary>
        /// Add all files with its hashcode from a given directory into a dictionary
        /// </summary>
        /// <param name="dir">directory</param>
        /// <param name="dict">dictionary</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void GenerateHashDictionary(string dir, Dictionary<string, string> dict)
        {
            //Pre-conditions
            if (dir.Equals("") || dir == null)
                throw new ArgumentException("Parameter cannot be empty/null", "dir");

            try
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
            catch (IOException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Creates metadata object from thumbdrive, thumbdrive's metadata file, web, web's metadatafile
        /// </summary>
        /// <param name="email">user's email</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="WebTransferException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void CreateMetaData(string email)
        {
            //Pre-conditions
            if (email.Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            localMeta = new MetaData();
            webMeta = new MetaData();

            try
            {
                if (File.Exists(LocalMetaDataPath))
                {
                    //Create Local Metadata
                    localMeta.DeSerialize(LocalMetaDataPath);
                }

                if (s3.ListFiles(email + '/' + WebMetaDataFileName).Count == 1)
                {
                    //Create Web Metadata - download from web then deserialize
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
            catch (ConnectionFailureException)
            {
                throw;
            }
            catch (Exception)
            {
                //Exceptions: ArgumentException, WebTransferException, IOException, UnauthorizedAccessException
                throw;
            }
        }

        /// <summary>
        /// Compares the hashcode of the entry in two metadata
        /// </summary>
        /// <param name="data1">metadata</param>
        /// <param name="data2">metadata</param>
        /// <param name="key">key in metadata</param>
        /// <returns>
        /// true - hashcode is the same in both metadata
        /// false - hashcode is different in both metadata
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        private bool HashIsDifferent(MetaData data1, MetaData data2, string key)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");
            
            //Get and compare hashcodes base on key given
            if (data1.GetEntryValue(key).Equals(data2.GetEntryValue(key)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks for file deletion using two metadata 
        /// </summary>
        /// <param name="hash">metadata</param>
        /// <param name="meta">metadata</param>
        /// <param name="key">path of file</param>
        /// <returns>
        /// true - file has been deleted in hash metadata but not in meta metadata
        /// false - file has not been deleted
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        private bool FileIsDeleted(MetaData hash, MetaData meta, string key)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            if (hash.GetEntryValue(key).Equals("") && !meta.GetEntryValue(key).Equals(""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,int> CheckConflicts()
        {
            CheckConflictsHelper(localHash, localMeta, webHash, webMeta, UPLOAD);
            CheckConflictsHelper(webHash, webMeta, localHash, localMeta, DOWNLOAD);

            return FilterConflicts();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="meta1"></param>
        /// <param name="hash2"></param>
        /// <param name="meta2"></param>
        /// <param name="direction"></param>
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
                        if (FileIsDeleted(hash2, meta2, entry.Key))
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
                            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                                CreateDirectory(Path.GetDirectoryName(localPath));

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
                catch (ConnectionFailureException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string gameName = key.Substring(0, key.IndexOf('/'));
                    errorList.Add(new SyncError(gameName, processName, ex.Message));
                }
            }

            try
            {
                //Serialize and Upload Metadata
                localHash.Serialize(LocalMetaDataPath);
                webHash.Serialize(WebMetaDataPath);
                s3.UploadFile(WebMetaDataPath, email + "/" + WebMetaDataFileName);
                File.Delete(WebMetaDataPath);
            }
            catch (ConnectionFailureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                errorList.Add(new SyncError("Local and Web Metadata Files", processName, ex.Message));
            }

            return errorList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,int> FilterConflicts()
        {
            Dictionary<string, int> filteredConflicts = new Dictionary<string, int>();

            foreach (string key in Conflicts.Keys)
            {
                filteredConflicts[GetGamesAndTypes(key)] = 0;
            }

            return filteredConflicts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolvedConflicts"></param>
        public void MergeConflicts(Dictionary<string, int> resolvedConflicts)
        {
            foreach (string key in Conflicts.Keys)
            {
                int side = resolvedConflicts[GetGamesAndTypes(key)];
                switch (Conflicts[key])
                {
                    case 12:
                        if (side == 1) NoConflict[key] = 1;
                        else if (side == 2) NoConflict[key] = 2;
                        break;
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

        /// <summary>
        /// Gets the game name and type of save (config/save)
        /// </summary>
        /// <param name="key">path to file</param>
        /// <returns>game and type of save</returns>
        private string GetGamesAndTypes(string key)
        {
            string game = key.Substring(0, key.IndexOf('/'));
            string type = key.Substring(key.IndexOf('/') + 1);

            return game + "/" + type.Substring(0, type.IndexOf('/'));
        }

        /// <summary>
        /// Update all metadatas
        /// </summary>
        /// <param name="key">path to file</param>
        /// <param name="value">hashcode of file</param>
        /// <exception cref="ArgumentException"></exception>
        private void UpdateMetaData(string key, string value)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");
            if (value == null)
                throw new ArgumentException("Parameter cannot be null", "value");

            //Update metadata
            localMeta.UpdateEntryValue(key, value);
            localHash.UpdateEntryValue(key, value);
            webMeta.UpdateEntryValue(key, value);
            webHash.UpdateEntryValue(key, value);
        }

        /// <summary>
        /// Add new entry in all metadatas
        /// </summary>
        /// <param name="key">path to file</param>
        /// <param name="value">hashcode of file</param>
        /// <exception cref="ArgumentException"></exception>
        private void AddMetaData(string key, string value)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");
            if (value == null)
                throw new ArgumentException("Parameter cannot be null", "value");

            //Update metadata
            localMeta.AddEntry(key, value);
            localHash.AddEntry(key, value);
            webMeta.AddEntry(key, value);
            webHash.AddEntry(key, value);
        }

        /// <summary>
        /// Delete an entry from all metadatas
        /// </summary>
        /// <param name="key">path to file</param>
        /// <exception cref="ArgumentException"></exception>
        private void DeleteMetaData(string key)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            //Delete entry from metadata
            localMeta.DeleteEntry(key);
            localHash.DeleteEntry(key);
            webMeta.DeleteEntry(key);
            webHash.DeleteEntry(key);
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="newFolderPath">path to folder</param>
        /// <exception cref="CreateFolderFailedException">unable to create folder</exception>
        protected void CreateDirectory(string newFolderPath)
        {
            if (!Directory.Exists(newFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                }
                catch (Exception ex)
                {
                    throw new CreateFolderFailedException("Unable to create new folder: " + newFolderPath, ex);
                }
            }
        }
    }
}
