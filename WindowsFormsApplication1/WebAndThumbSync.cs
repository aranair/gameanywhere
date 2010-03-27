using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GameAnywhere
{
    /// <summary>
    /// This class contains all the required methods to synchronize game files store on the external drive and S3.
    /// CheckConflicts() has to always be called first so that conflicting files can be automatically resolved.
    /// Conflicts that has to be resolved would be then passed back to the other layers to be resolved.
    /// SynchronizeGames() takes in the resolved conflicts and does the actual synchronization.
    /// </summary>
    class WebAndThumbSync
    {
        /// <summary>
        /// noConflict stores the list of files that can be automatically resolved.
        /// Conflicts stores the list of files that require external resolution.
        /// The integer values represent the direction or conflict of the resolution result.
        /// </summary>
        public Dictionary<string, int> NoConflict, Conflicts;

        /// <summary>
        /// Metadata objects stored as class data members.
        /// localHash and localMeta are current and stored Metadata objects of local files.
        /// webHash and webMeta are current and stored Metadata objects of files stored on the web.
        /// </summary>
        private MetaData localHash, localMeta, webHash, webMeta;

        private string email;
        private Storage s3 = new Storage();

        public static readonly string LocalMetaDataFileName = "metadata.ga";
        public static readonly string WebMetaDataFileName = "metadata.web";
        private static string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");
        private string LocalMetaDataPath = Path.Combine(syncFolderPath, LocalMetaDataFileName);
        private string WebMetaDataPath = Path.Combine(syncFolderPath, WebMetaDataFileName);

        /// <summary>
        /// The list of integer values used for conflict resolution.
        /// </summary>
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
        /// Generates the hashcode of all the files in the SyncFolder directory and its subdirectories.
        /// </summary>
        /// <param name="dir">path to the game directory in SyncFolder.</param>
        /// <param name="dict">Dictionary object to store the key-values.</param>
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
                    dict[file.Replace(syncFolderPath + @"\", "").Replace(@"\", "/")] = s3.GenerateHash(file);
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
        /// Checks if the hashcode of a file stored in two Metadata files are different.
        /// </summary>
        /// <param name="data1">First Metadata object</param>
        /// <param name="data2">Second Metadata object</param>
        /// <param name="key">Key of the filename to compare the hashcode.</param>
        /// <returns>
        /// true - hashcode is the same in both Metadata objects
        /// false - hashcode is different in both Metadata objects
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        private bool HashIsDifferent(MetaData meta1, MetaData meta2, string key)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");
            
            //Get and compare hashcodes base on key given
            if (meta1.GetEntryValue(key).Equals(meta2.GetEntryValue(key)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if a file is deleted by comparing two Metadata objects.
        /// If the key is found in the stored Metadata but not found in the current Metadata, the file is deleted.
        /// </summary>
        /// <param name="hash">Current Metadata</param>
        /// <param name="meta">Stored Metadata</param>
        /// <param name="key">path of file</param>
        /// <returns>
        /// true - file has been deleted
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
        /// Checks for synchronization conflicts between two locations.
        /// Files that can be resolved automatically will be stored in the class data member, noConflict.
        /// Files that must be resolved externally will be stored in the class data member, Conflicts.
        /// </summary>
        /// <returns>The Conflicts that has been filtered by FilterConflicts().</returns>
        public Dictionary<string,int> CheckConflicts()
        {
            CheckConflictsHelper(localHash, localMeta, webHash, webMeta, UPLOAD);
            CheckConflictsHelper(webHash, webMeta, localHash, localMeta, DOWNLOAD);

            return FilterConflicts();
        }

        /// <summary>
        /// Helper method for CheckConflicts().
        /// This method has to be called twice with two permutations to fully check the conflicts between two locations.
        /// Call 1:
        ///         First location - Local files
        ///         Second location - Web files
        ///         Direction - UPLOAD
        /// Call 2:
        ///         First location - Web files
        ///         Second location - Local files
        ///         Direction - DOWNLOAD
        /// </summary>
        /// <param name="hash1">Current Metadata object of the first location.</param>
        /// <param name="meta1">Stored Metadata object of the first location.</param>
        /// <param name="hash2">Current Metadata object of the second location.</param>
        /// <param name="meta2">Stored Metadata object of the second location.</param>
        /// <param name="direction">Direction of synchronization for resolution.</param>
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
                            UpdateMetaData(key, s3.GenerateHash(localPath));
                            break;
                        case DOWNLOAD:
                            //Create the target directory if needed
                            //Console.WriteLine(Path.GetDirectoryName(localDirName));
                            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                                CreateDirectory(Path.GetDirectoryName(localPath));

                            s3.DownloadFile(localPath, webPath);
                            UpdateMetaData(key, s3.GenerateHash(localPath));
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
        /// Filters the Conflicts Dictionary so that only one type of each game is returned.
        /// This list is passed to other layers for the conflicts to be resolved.
        /// The default value for each key is 0.
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
        /// Merges the filtered Dictionary that has been resolved by other layers into the noConflict Dictionary.
        /// The only values of resolvedConflicts are 1 and 2, which determine the direction of all files of that type.
        /// The conflicts will be merged as follows:
        /// UPDOWNCONFLICT      - 1:UPLOAD,   2:DOWNLOAD
        /// DELETELOCALCONFLICT - 1:UPLOAD,   2:DELETELOCAL
        /// DELETEWEBCONFLICT   - 1:DOWNLOAD, 2:DELETEWEB
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
        /// Gets the game name and type of save (config/save) from a key.
        /// </summary>
        /// <param name="key">key of filename</param>
        /// <returns>game and type of save in the form "[game]/[type]".</game></returns>
        private string GetGamesAndTypes(string key)
        {
            string game = key.Substring(0, key.IndexOf('/'));
            string type = key.Substring(key.IndexOf('/') + 1);

            return game + "/" + type.Substring(0, type.IndexOf('/'));
        }

        /// <summary>
        /// Update all Metadata objects
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
        /// Add new entry in all Metadata objects
        /// </summary>
        /// <param name="key">key of filename</param>
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
        /// Delete an entry from all Metadata objects
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
