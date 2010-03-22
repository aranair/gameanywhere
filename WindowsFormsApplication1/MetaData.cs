using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameAnywhere
{
    /// <summary>
    /// MetaData Class
    /// Desc: Stores information about each file/directory's hashcode
    /// 
    /// Serialization Info:
    /// http://msdn.microsoft.com/en-us/library/4abbf6k0(VS.71).aspx
    /// Dictionary API:
    /// http://msdn.microsoft.com/en-us/library/xfhwa508.aspx
    /// </summary>
    [Serializable]
    class MetaData
    {
        /// <summary>
        /// Data members
        /// </summary>
        private Dictionary<string, string> fileTable;

        /// <summary>
        /// Accessors & Mutators
        /// </summary>
        public Dictionary<string, string> FileTable
        {
            get { return fileTable; }
            set { fileTable = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MetaData()
        {
            this.fileTable = new Dictionary<string, string>();
        }

        public MetaData(Dictionary<string, string> hashTable)
        {
            if (hashTable == null)
            {
                //Creates new empty table
                this.fileTable = new Dictionary<string, string>();
            }
            else
            {
                this.fileTable = hashTable;
            }
        }

        /// <summary>
        /// Adds an entry to the file table
        /// </summary>
        /// <param name="fileName">path of file/directory</param>
        /// <param name="hashCode">hash code of file</param>
        /// <returns>
        /// true - Entry added successfully to table
        /// false - Entry already exists in table
        /// </returns>
        /// Exceptions: ArgumentException
        public bool AddEntry(string fileName, string hashCode)
        {
            //Pre-conditions
            if (fileName.Equals("") || fileName == null)
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Add entry to fileTable
            if (!fileTable.ContainsKey(fileName))
            {
                fileTable.Add(fileName, hashCode);
                return true;
            }
            else
            {
                //Entry already exist
                return false;
            }
        }

        /// <summary>
        /// Deletes an entry from the file table
        /// </summary>
        /// <param name="fileName">path of file/directory</param>
        /// <returns>
        /// true - Entry found and successfully removed from table
        /// false - Entry not found
        /// </returns>
        /// Exceptions: ArgumentException
        public bool DeleteEntry(string fileName)
        {
            //Pre-conditions
            if (fileName.Equals("") || fileName == null)
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Remove entry
            return fileTable.Remove(fileName);
        }

        /// <summary>
        /// Updates entry with new value
        /// </summary>
        /// <param name="fileName">path of file/directory</param>
        /// <param name="newValue">new value of entry</param>
        public void UpdateEntryValue(string fileName, string newValue)
        {
            //Pre-conditions
            if (fileName.Equals("") || fileName == null)
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Adds new entry if fileName not found in table, and update entry if fileName found in table
            fileTable[fileName] = newValue;
        }

        /// <summary>
        /// Gets the value of the entry in the table
        /// </summary>
        /// <param name="fileName">path of file/directory</param>
        /// <returns>value of the key</returns>
        /// Exceptions: ArgumentException, KeyNotFoundException
        public string GetEntryValue(string fileName)
        {
            //Pre-conditions
            if (fileName.Equals("") || fileName == null)
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Checks if entry is in table before accessing
            if (fileTable.ContainsKey(fileName))
            {
                return fileTable[fileName];
            }
            else
            {
                //Entry not found in metadata
                //throw new KeyNotFoundException("Entry \"{fileName}\" not found in metadata");
                return "";
            }
        }

        /// <summary>
        /// Checks if entry exists in table
        /// </summary>
        /// <param name="fileName">path of file/directory</param>
        /// <returns>
        /// true - Entry exists table
        /// false - Entry does not exist in table
        /// </returns>
        /// Exceptions: ArgumentException
        public bool EntryExist(string fileName)
        {
            //Pre-conditions
            if (fileName.Equals("") || fileName == null)
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Checks if entry exists in table
            return fileTable.ContainsKey(fileName);
        }

        /// <summary>
        /// Prints the list of entries found in the table
        /// </summary>
        public void PrintMetaDataContents()
        {
            if (fileTable.Count > 0)
            {
                Console.WriteLine("-KEY-,-VALUE-");
                foreach (KeyValuePair<string, string> entry in fileTable)
                    Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No entry found in metadata");
            }
        }

        /// <summary>
        /// Serialize this MetaData object to a file
        /// </summary>
        /// <param name="fileName">path of metadata file</param>
        /// Exceptions???
        public void Serialize(string fileName)
        {
            Stream stream = File.Open(fileName, FileMode.Create);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// DeSerialize the file to a MetaData object
        /// </summary>
        /// <param name="fileName">path of metadata file</param>
        /// <returns>MetaData object</returns>
        /// Exceptions???
        public MetaData DeSerialize(string fileName)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Open);
            MetaData obj = (MetaData)formatter.Deserialize(stream);
            stream.Close();
            this.fileTable = obj.fileTable;
            return this;
        }
    }
}
