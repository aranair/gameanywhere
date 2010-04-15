//Eu Xing Long Nicholas
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    /// <summary>
    /// The exception that is thrown when a file upload to S3 is corrupted.
    /// </summary>
    public class WebTransferException : Exception
    {
        public string errorMessage;
        public WebTransferException() { }
        public WebTransferException(String message) { errorMessage = message; }
        public WebTransferException(String message, System.Exception inner) { }
    }
}
