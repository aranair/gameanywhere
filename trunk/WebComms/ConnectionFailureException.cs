//Eu Xing Long Nicholas
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when there is no Internet connection.
    /// </summary>
    public class ConnectionFailureException : Exception
    {
        public ConnectionFailureException() { }
        public ConnectionFailureException(String message) { }
        public ConnectionFailureException(String message, System.Exception inner) { }
    }
}
