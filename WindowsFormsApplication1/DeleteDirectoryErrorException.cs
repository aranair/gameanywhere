using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    class DeleteDirectoryErrorException : System.ApplicationException
    {
        public string errorMessage;
        public DeleteDirectoryErrorException() {}
        public DeleteDirectoryErrorException(String message) { errorMessage = message; }
        public DeleteDirectoryErrorException(String message, System.Exception inner) { }
    }
}
