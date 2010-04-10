using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAnywhere.Process
{
    class UserVerifier : Verifier
    {

        #region ResendActivation method
        public static TestResult VerifyResendActivation(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                case 3: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 4: //Checked expected returned type : 0
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                default: break;
            }

            return result;
        }
        #endregion

        #region ChangePassword method

        public static TestResult VerifyChangePassword(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                case 3:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }

                case 4: //check exception ArugmentException
                    {
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 5: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }

            return result;
        }
        #endregion

        # region Verify Register methods

        public static TestResult VerifyRegisterRetrievePassword(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                    //check expeceted return type
                    {
                        int outcome = (int)returnType;
                        int expected = (int)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }
                case 3:
                    {
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 4:
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }
            return result;
        }

        #endregion

        # region Verify Login methods

        public static TestResult VerifyLogin(int index, object returnType, object testClass, string exceptionThrown, object expectedOutput)
        {
            TestResult result = new TestResult(true);
            switch (index)
            {
                case 1:
                case 2:
                case 3:
                    //check expeceted return type
                    {
                        bool outcome = (bool)returnType;
                        bool expected = (bool)expectedOutput;
                        if (outcome != expected)
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Wrong return type");
                        }
                        else
                        {
                            result.AddRemarks("Passed!");
                        }
                        break;
                    }

                case 4: //check exception ArugmentException
                    {
                        Exception err = (Exception)returnType;
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                case 5: // WebException
                    {
                        Exception err = null;
                        if (returnType.GetType().Name.Equals("TargetInvocationException"))
                            err = (Exception)returnType;
                        else
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                            break;
                        }
                        if (!err.InnerException.GetType().Name.Equals(exceptionThrown))
                        {
                            result.Result = false;
                            result.AddRemarks("Failed! Exception being thrown");
                        }
                        else
                            result.AddRemarks("Passed!");
                        break;
                    }
                default: break;
            }

            return result;
        }

        #endregion

    }
}
