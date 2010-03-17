using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    class SyncFolderFileDiscrepancyException : System.ApplicationException
    {
        public SyncFolderFileDiscrepancyException() { }
        public SyncFolderFileDiscrepancyException(String message) { }
        public SyncFolderFileDiscrepancyException(String message, System.Exception inner) { }
    }
}
