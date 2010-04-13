//Leow Guan Jad Wilmer
using System;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when an error occurred while deleting directory.
    /// </summary>
    class DeleteDirectoryErrorException : Exception
    {
        public string errorMessage;
        public DeleteDirectoryErrorException() {}
        public DeleteDirectoryErrorException(String message) { errorMessage = message; }
        public DeleteDirectoryErrorException(String message, System.Exception inner) { }
    }
}
