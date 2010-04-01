using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    public class WebTransferException : System.ApplicationException
    {
        public string errorMessage;
        public WebTransferException() { }
        public WebTransferException(String message) { errorMessage = message; }
        public WebTransferException(String message, System.Exception inner) { }
    }
}
