using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    class InsufficientSpaceException : System.ApplicationException
    {
        public InsufficientSpaceException() { }
        public InsufficientSpaceException(String message) { }
        public InsufficientSpaceException(String message, System.Exception inner) { }
    }
}
