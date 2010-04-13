//Tan Hong Zhou
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere.Process
{
    class TestResult
    {
        private bool result;
        private object returnType;
        private string remarks;

        public string Remarks
        {
            get { return remarks; }
            set { remarks = value; }
        }
        public bool Result
        {
            get { return result; }
            set { result = value; }
        }
        public object ReturnType
        {
            get { return returnType; }
            set { returnType = value; }
        }

        public TestResult(bool outcome)
        {
            result = outcome;
            returnType = null; remarks = "";
        }

        public void AddRemarks(string comments)
        {
            remarks += comments+"\n";
        }

    }
}
