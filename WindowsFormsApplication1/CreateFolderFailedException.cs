using System;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when an error occurred while creating a folder.
    /// </summary>
    class CreateFolderFailedException : Exception
    {
        public string errorMessage = "Unable to create folder in directory.";
        public CreateFolderFailedException() { }
        public CreateFolderFailedException(String message) { errorMessage = message; }
        public CreateFolderFailedException(String message, System.Exception inner) { }
    }
}
