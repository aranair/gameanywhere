using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    public class InvalidSyncDirectionException : System.ApplicationException
    {
        public InvalidSyncDirectionException() { }
        public InvalidSyncDirectionException(String message) { }
        public InvalidSyncDirectionException(String message, System.Exception inner) { }
    }
}
