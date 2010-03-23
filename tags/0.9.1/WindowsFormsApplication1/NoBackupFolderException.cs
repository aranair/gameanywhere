using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    public class NoBackupFolderException : System.ApplicationException
    {
        public NoBackupFolderException() { }
        public NoBackupFolderException(String message) { }
        public NoBackupFolderException(String message, System.Exception inner) { }
    }
}
