//Goh Haoyu Gerald
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when an insufficient space error occurs.
    /// </summary>
    class InsufficientSpaceException : Exception
    {
        public InsufficientSpaceException() { }
        public InsufficientSpaceException(String message) { }
        public InsufficientSpaceException(String message, System.Exception inner) { }
    }
}
