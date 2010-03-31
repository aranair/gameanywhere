using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    class CreateFolderFailedException : System.ApplicationException
    {
        public string errorMessage = "Unable to create folder in directory.";
        public CreateFolderFailedException() { }
        public CreateFolderFailedException(String message) { errorMessage = message; }
        public CreateFolderFailedException(String message, System.Exception inner) { }
    }
}
