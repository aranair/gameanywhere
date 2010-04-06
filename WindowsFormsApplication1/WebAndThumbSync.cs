using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using GameAnywhere.Data;

namespace GameAnywhere.Process
{
    /// <summary>
    /// This class contains all the required methods to synchronize game files store on the external drive and S3.
    /// </summary>
    /// <remarks>
    /// CheckConflicts() has to always be called first so that conflicting files can be automatically resolved.
    /// The conflicts that has to be resolved would be then returned to caller to be resolved.
    /// SynchronizeGames() takes in the resolved conflicts and does the actual synchronization.
    /// </remarks>
    class WebAndThumbSync
    {
        /// <summary>
        /// noConflict stores the list of files that can be automatically resolved.
        /// Conflicts stores the list of files that require external resolution.
        /// </summary>
        /// <remarks>
        /// The integer values represent the direction or conflict of the resolution result.
        /// </remarks>
        public Dictionary<string, NoConflictDirection> NoConflict;
        public Dictionary<string, ConflictDirection> Conflicts;

        /// <summary>
        /// Metadata objects stored as class data members.
        /// </summary>
        /// <remarks>
        /// localHash and localMeta are current and stored Metadata objects of local files.
        /// webHash and webMeta are current and stored Metadata objects of files stored on the web.
        /// </remarks>
        private MetaData localHash, localMeta, webHash;

        private string email;
        private Storage s3 = new Storage();

        public static readonly string LocalMetaDataFileName = "metadata.ga";
        private static string syncFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SyncFolder");
        private string LocalMetaDataPath = Path.Combine(syncFolderPath, LocalMetaDataFileName);

        /// <summary>
        /// Enumeration of directions for files that do not require conflict resolution.
        /// </summary>
        public enum NoConflictDirection { Upload, Download, DeleteLocal, DeleteWeb, DeleteMetadata };
        /// <summary>
        /// Enumeration of conflicting directions for files that require conflict resolution.
        /// </summary>
        public enum ConflictDirection { UploadOrDownload, UploadOrDeleteLocal, DownloadOrDeleteWeb };
        /// <summary>
        /// Enumeration for CheckConflictsHelper method.
        /// </summary>
        private enum CheckConflictsDirection { LocalToWeb, WebToLocal };

        /// <summary>
        /// Current Metadata object of files stored on the web.
        /// </summary>
        internal MetaData WebHash
        {
            get { return webHash; }
            set { webHash = value; }
        }

        /// <summary>
        /// Current Metadata object of local files.
        /// </summary>
        internal MetaData LocalHash
        {
            get { return localHash; }
            set { localHash = value; }
        }

        /// <summary>
        /// Stored Metadata object of local files.
        /// </summary>
        internal MetaData LocalMeta
        {
            get { return localMeta; }
            set { localMeta = value; }
        }



        /// <summary>
        /// Constructor for WebAndThumbSync
        /// </summary>
        /// <param name="u">User object</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="WebTransferException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public WebAndThumbSync(User user)
        {
            email = user.Email;
            NoConflict = new Dictionary<string,NoConflictDirection>();
            Conflicts = new Dictionary<string, ConflictDirection>();
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
        /// <param name="dir">Path of the game directory in SyncFolder.</param>
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
                    if (Path.GetFileName(file).Equals(LocalMetaDataFileName)) continue; //ignore metadata file
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
        /// Create Metadata objects.
        /// </summary>
        /// <remarks>
        /// Creates the stored Metadata objects by deserializing the stored metadata file on the thumbdrive and web.
        /// Crates the current Metadata objects from the hashcodes of the current state of local files and files on the web.
        /// </remarks>
        /// <param name="email">user's email</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void CreateMetaData(string email)
        {
            //Pre-conditions
            if (email.Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            localMeta = new MetaData();

            try
            {
                if (File.Exists(LocalMetaDataPath))
                {
                    //Create Local Metadata
                    localMeta.DeSerialize(LocalMetaDataPath);
                }

                //Generate hash from files stored on web and create Metadata object
                webHash = new MetaData(s3.GetHashDictionary(email));

                //Generate hash from local files and create Metadata object
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
                //Exceptions: ArgumentException, IOException, UnauthorizedAccessException
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
        /// Checks if a file is deleted.
        /// </summary>
        /// <remarks>
        /// Compares two Metadata objects. 
        /// If the key is found in the stored Metadata but not found in the current Metadata, the file was deleted.
        /// </remarks>
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
        /// </summary>
        /// <remarks>
        /// Files that can be resolved automatically will be stored in the class data member, noConflict.
        /// Files that must be resolved externally will be stored in the class data member, Conflicts.
        /// </remarks>
        /// <returns>The Conflicts that has been filtered by FilterConflicts().</returns>
        public Dictionary<string,int> CheckConflicts()
        {
            CheckConflictsHelper(localHash, webHash, localMeta, CheckConflictsDirection.LocalToWeb);
            CheckConflictsHelper(webHash, localHash, localMeta, CheckConflictsDirection.WebToLocal);

            return FilterConflicts();
        }

        /// <summary>
        /// Helper method for CheckConflicts().
        /// </summary>
        /// <remarks>
        /// This method has to be called twice with two permutations to fully check the conflicts between two locations.
        /// Call 1:
        ///         First location - Local files
        ///         Second location - Web files
        ///         Direction - CheckConflictsDirection.LocalToWeb
        /// Call 2:
        ///         First location - Web files
        ///         Second location - Local files
        ///         Direction - CheckConflictsDirection.WebToLocal
        /// </remarks>
        /// <param name="hash1">Current Metadata object of the first location.</param>
        /// <param name="hash2">Current Metadata object of the second location.</param>
        /// <param name="meta">Stored Metadata object.</param>
        /// <param name="direction">Direction of synchronization for resolution.</param>
        private void CheckConflictsHelper(MetaData hash1, MetaData hash2, MetaData meta, CheckConflictsDirection direction)
        {
            foreach (KeyValuePair<string, string> entry in hash1.FileTable)
            {
                if (meta.EntryExist(entry.Key))
                {
                    //If hash1 and meta1 are different (i.e. changed)
                    if (HashIsDifferent(hash1, meta, entry.Key))
                    {
                        //If hash2 and meta2 have a deleted file
                        if (FileIsDeleted(hash2, meta, entry.Key))
                        {
                            if (direction == CheckConflictsDirection.LocalToWeb)
                            {
                                Conflicts[entry.Key] = ConflictDirection.UploadOrDeleteLocal;
                            }
                            else if (direction == CheckConflictsDirection.WebToLocal)
                            {
                                Conflicts[entry.Key] = ConflictDirection.DownloadOrDeleteWeb;
                            }
                        }
                        //If hash2 and meta2 are same (i.e. not changed)
                        else if (!HashIsDifferent(hash2, meta, entry.Key))
                        {
                            if (direction == CheckConflictsDirection.LocalToWeb)
                            {
                                NoConflict[entry.Key] = NoConflictDirection.Upload;
                            }
                            else if (direction == CheckConflictsDirection.WebToLocal)
                            {
                                NoConflict[entry.Key] = NoConflictDirection.Download;
                            }
                        }
                        //If hash2 and meta2 are different (i.e. conflict)
                        else
                        {
                            Conflicts[entry.Key] = ConflictDirection.UploadOrDownload;
                        }
                    }
                }
                else //New file
                {
                    if (hash2.EntryExist(entry.Key)) //both new files, conflict
                        Conflicts[entry.Key] = ConflictDirection.UploadOrDownload;
                    else
                    {
                        if (direction == CheckConflictsDirection.LocalToWeb)
                            NoConflict[entry.Key] = NoConflictDirection.Upload;
                        else if (direction == CheckConflictsDirection.WebToLocal)
                            NoConflict[entry.Key] = NoConflictDirection.Download;
                    }
                }
            }

            foreach (KeyValuePair<string, string> entry in meta.FileTable)
            {
                if (!hash1.EntryExist(entry.Key)) //Deleted file
                {
                    if (!hash2.EntryExist(entry.Key)) //both deleted, delete metadata
                        NoConflict[entry.Key] = NoConflictDirection.DeleteMetadata;
                    else if (Conflicts.ContainsKey(entry.Key))
                        continue;
                    else if (HashIsDifferent(hash2, meta, entry.Key))
                    {
                        if (direction == CheckConflictsDirection.LocalToWeb)
                        {
                            Conflicts[entry.Key] = ConflictDirection.DownloadOrDeleteWeb;
                        }
                        if (direction == CheckConflictsDirection.WebToLocal)
                        {
                            Conflicts[entry.Key] = ConflictDirection.UploadOrDeleteLocal;
                        }
                    }
                    else
                    {
                        if (direction == CheckConflictsDirection.LocalToWeb) //Delete from web
                        {
                            NoConflict[entry.Key] = NoConflictDirection.DeleteWeb;
                        }
                        if (direction == CheckConflictsDirection.WebToLocal)//Delete from thumb
                        {
                            NoConflict[entry.Key] = NoConflictDirection.DeleteLocal;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Synchronize the local files with the files on the web.
        /// </summary>
        /// <param name="resolvedConflicts">Conflicts that have been resolved.</param>
        /// <returns>The list of SyncErrors of files that have failed to be synchronized.</returns>
        /// <exception cref="ConnectionFailureException"></exception>
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
                        case NoConflictDirection.Upload:
                            //Upload file to S3
                            s3.UploadFile(localPath, webPath);
                            UpdateMetaData(key, s3.GenerateHash(localPath));
                            break;
                        case NoConflictDirection.Download:
                            //Create the target directory if needed
                            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                                CreateDirectory(Path.GetDirectoryName(localPath));
                            //Download file from S3
                            s3.DownloadFile(localPath, webPath);
                            UpdateMetaData(key, s3.GenerateHash(localPath));
                            break;
                        case NoConflictDirection.DeleteLocal:
                            //Delete file from thumbdrive
                            File.Delete(localPath);
                            DeleteMetaData(key);
                            break;
                        case NoConflictDirection.DeleteWeb:
                            //Delete file from S3
                            s3.DeleteDirectory(webPath);
                            DeleteMetaData(key);
                            break;
                        case NoConflictDirection.DeleteMetadata:
                            //Delete metadata entry
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
                //Serialize Metadata object
                localHash.Serialize(LocalMetaDataPath);
            }
            catch (ConnectionFailureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                errorList.Add(new SyncError("Metadata serialization failed.", processName, ex.Message));
            }

            return errorList;
        }

        /// <summary>
        /// Filters the Conflicts Dictionary so that only one type of each game is returned.
        /// </summary>
        /// <remarks>
        /// This list is returned to the caller for the conflicts to be resolved.
        /// The default value for each key is 0, and has to be changed to reflect the correct direction.
        /// </remarks>
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
        /// Merges the filtered Dictionary that has been resolved by the caller into the noConflict Dictionary.
        /// </summary>
        /// <remarks>
        /// The only values of resolvedConflicts are 1 and 2, which determine the direction of all files of that type.
        /// The conflicts will be merged as follows:
        /// UploadOrDownload      - 1:Upload,   2:Download
        /// UploadOrDeleteLocal   - 1:Upload,   2:DeleteLocal
        /// DownloadOrDeleteWeb   - 1:Download, 2:DeleteWeb
        /// </remarks>
        /// <param name="resolvedConflicts"></param>
        public void MergeConflicts(Dictionary<string, int> resolvedConflicts)
        {
            foreach (string key in Conflicts.Keys)
            {
                int direction = resolvedConflicts[GetGamesAndTypes(key)];
                switch (Conflicts[key])
                {
                    case ConflictDirection.UploadOrDownload:
                        if (direction == 1) NoConflict[key] = NoConflictDirection.Upload;
                        else if (direction == 2) NoConflict[key] = NoConflictDirection.Download;
                        break;
                    case ConflictDirection.UploadOrDeleteLocal:
                        if (direction == 1) NoConflict[key] = NoConflictDirection.Upload;
                        else if (direction == 2) NoConflict[key] = NoConflictDirection.DeleteLocal;
                        break;
                    case ConflictDirection.DownloadOrDeleteWeb:
                        if (direction == 1) NoConflict[key] = NoConflictDirection.Download;
                        else if (direction == 2) NoConflict[key] = NoConflictDirection.DeleteWeb;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the game name and type (config or savedGame) from a key.
        /// </summary>
        /// <remarks>
        /// Key is assumed to be of format [email]/[game]/[type]/[folder or file]/..
        /// This method returns [game]/[type].
        /// </remarks>
        /// <param name="key">Key of filename</param>
        /// <returns>A string of the game name and type.</game></returns>
        private string GetGamesAndTypes(string key)
        {
            string game = key.Substring(0, key.IndexOf('/'));
            string type = key.Substring(key.IndexOf('/') + 1);

            return game + "/" + type.Substring(0, type.IndexOf('/'));
        }

        /// <summary>
        /// Update all Metadata objects
        /// </summary>
        /// <remarks>
        /// Creates a new key if it doesn't exist.
        /// </remarks>
        /// <param name="key">Key of file</param>
        /// <param name="value">Hashcode of file</param>
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
            webHash.UpdateEntryValue(key, value);
        }

        /// <summary>
        /// Delete an entry from all Metadata objects
        /// </summary>
        /// <param name="key">Key of file</param>
        /// <exception cref="ArgumentException"></exception>
        private void DeleteMetaData(string key)
        {
            //Pre-conditions
            if (key.Equals("") || key == null)
                throw new ArgumentException("Parameter cannot be empty/null", "key");

            //Delete entry from metadata
            localMeta.DeleteEntry(key);
            localHash.DeleteEntry(key);
            webHash.DeleteEntry(key);
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="newFolderPath">Path to folder</param>
        /// <exception cref="CreateFolderFailedException">Unable to create folder</exception>
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
