using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when no drive is found occurs.
    /// </summary>
    class DriveNotFoundException : System.ApplicationException
    {
        public DriveNotFoundException() { }
        public DriveNotFoundException(String message) { }
        public DriveNotFoundException(String message, System.Exception inner) { }
    }
}