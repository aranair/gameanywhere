﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere
{
    public class ConnectionFailureException : Exception
    {
        public ConnectionFailureException() { }
        public ConnectionFailureException(String message) { }
        public ConnectionFailureException(String message, System.Exception inner) { }
    }
}
