using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when an error occurred while deleting directory.
    /// </summary>
    class DeleteDirectoryErrorException : System.ApplicationException
    {
        public string errorMessage;
        public DeleteDirectoryErrorException() {}
        public DeleteDirectoryErrorException(String message) { errorMessage = message; }
        public DeleteDirectoryErrorException(String message, System.Exception inner) { }
    }
}
