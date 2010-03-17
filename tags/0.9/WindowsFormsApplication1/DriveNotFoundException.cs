using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    class DriveNotFoundException : System.ApplicationException
    {
        public DriveNotFoundException() { }
        public DriveNotFoundException(String message) { }
        public DriveNotFoundException(String message, System.Exception inner) { }
    }
}