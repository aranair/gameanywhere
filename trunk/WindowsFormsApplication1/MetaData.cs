using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameAnywhere.Process
{
    /// <summary>
    /// Stores information about each file.
    /// </summary>
    [Serializable]
    class MetaData
    {
        #region Data Members
        /// <summary>
        /// Stores the file path and hashcode of file.
        /// </summary>
        private Dictionary<string, string> fileTable;
        #endregion

        /// <summary>
        /// Accessors & Mutators
        /// </summary>
        public Dictionary<string, string> FileTable
        {
            get { return fileTable; }
            set { fileTable = value; }
        }

        #region Constructors
        /// <summary>
        /// Initialize the data members.
        /// </summary>
        public MetaData()
        {
            this.fileTable = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initialize the data members.
        /// </summary>
        /// <param name="hashTable"></param>
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
        #endregion

        /// <summary>
        /// Adds an entry to the file table.
        /// </summary>
        /// <param name="fileName">Path of file.</param>
        /// <param name="hashCode">Hash code of file.</param>
        /// <returns>
        /// True - Entry added successfully to table.
        /// False - Entry already exists in table.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AddEntry(string fileName, string hashCode)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");
            if (hashCode == null)
                throw new ArgumentException("Parameter cannot be null", "hashCode");

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
        /// Deletes an entry from the file table.
        /// </summary>
        /// <param name="fileName">Path of file.</param>
        /// <returns>
        /// True - Entry found and successfully removed from table.
        /// False - Entry not found.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public bool DeleteEntry(string fileName)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Remove entry
            return fileTable.Remove(fileName);
        }

        /// <summary>
        /// Updates entry with new value.
        /// </summary>
        /// <param name="fileName">Path of file.</param>
        /// <param name="newValue">New value of entry.</param>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateEntryValue(string fileName, string newValue)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");
            if (newValue == null)
                throw new ArgumentException("Parameter cannot be null", "newValue");

            //Adds new entry if fileName not found in table, and update entry if fileName found in table
            fileTable[fileName] = newValue;
        }

        /// <summary>
        /// Gets the value of the entry in the table.
        /// </summary>
        /// <param name="fileName">Path of file.</param>
        /// <returns>Value of the key.</returns>
        /// <exception cref="ArgumentException"></exception>
        public string GetEntryValue(string fileName)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Checks if entry is in table before accessing
            if (fileTable.ContainsKey(fileName))
            {
                return fileTable[fileName];
            }
            else
            {
                //Entry not found in metadata
                return "";
            }
        }

        /// <summary>
        /// Checks if entry exists in table.
        /// </summary>
        /// <param name="fileName">Path of file.</param>
        /// <returns>
        /// True - Entry exists table.
        /// False - Entry does not exist in table.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public bool EntryExist(string fileName)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            //Checks if entry exists in table
            return fileTable.ContainsKey(fileName);
        }

        /// <summary>
        /// Prints the list of entries found in the table.
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
        /// Serialize this MetaData object to a file.
        /// </summary>
        /// <param name="fileName">Path of metadata file.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Serialize(string fileName)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");

            Stream stream = null;

            try
            {
                stream = File.Open(fileName, FileMode.Create);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Close();
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
                if (stream != null) stream.Close();
            }
        }

        /// <summary>
        /// DeSerialize the file to a MetaData object.
        /// </summary>
        /// <param name="fileName">Path of metadata file.</param>
        /// <returns>MetaData object.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public MetaData DeSerialize(string fileName)
        {
            //Pre-conditions
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Parameter cannot be empty/null", "fileName");
            Stream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                MetaData obj = (MetaData)formatter.Deserialize(stream);
                stream.Close();
                fileTable = obj.fileTable;
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
            finally
            {
                if (stream != null) stream.Close();
            }

            return this;
        }
    }
}
