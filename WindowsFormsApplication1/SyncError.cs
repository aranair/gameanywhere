using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// A data structure that stores the error information per file during synchronizing.
    /// </summary>
    public class SyncError
    {
        string filePath;
        string processName;
        string errorMessage;

        #region Properties
        /// <summary>
        /// The path of the file.
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        /// <summary>
        /// The process that encounter this sync error.
        /// </summary>
        public string ProcessName
        {
            get { return processName; }
            set { processName = value; }
        }
        /// <summary>
        /// The error message of this sync error.
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        #endregion

        /// <summary>
        /// Overloaded Contructor.
        /// </summary>
        /// <param name="path">The path that contains error.</param>
        /// <param name="process">The process that uses this path.</param>
        /// <param name="message">The error message encounted.</param>
        public SyncError(string path, string process, string message)
        {
            filePath = path;
            processName = process;
            errorMessage = message;
        }
    }
}
