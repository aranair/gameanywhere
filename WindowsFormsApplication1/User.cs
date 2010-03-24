using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Diagnostics;


namespace GameAnywhere
{
    class User
    {
        /// <summary>
        /// Data Members
        /// </summary>
        private string email;
        private string password;
        private SimpleDB db;
        UserState state;
        enum UserState { UserLoggedIn, Guest };

        /// <summary>
        /// Constructor
        /// </summary>
        public User()
        {
            email = "";
            password = "";
            db = new SimpleDB();
            state = UserState.Guest;
        }

        /// <summary>
        /// Accessors & Mutators
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        /// <returns>
        /// true - User exists and State changes to UserLoggedIn
        /// false - Wrong password / User does not exist / User exists but not activated
        /// </returns>
        /// Exceptions: ArgumentException, ConnectionFailureException
        public bool Login(string email, string password)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (password.Trim().Equals("") || password == null)
                throw new ArgumentException("Parameter cannot be empty/null", "password");

            Debug.Assert(password.Length >= 6);

            try
            {
                if (UserExists(email) && IsAccountActivated(email))
                {
                    //Login user
                    string userPassword = db.GetAttribute(email, "Password");
                    if (userPassword.Equals(password))
                    {
                        this.email = email;
                        this.password = password;
                        this.state = UserState.UserLoggedIn;
                        return true;
                    }
                    else
                    {
                        //User exists but wrong password
                        return false;
                    }
                }
                else
                {
                    //User doesn't exist or Exists but not activated
                    return false;
                }
            }
            catch
            {
                //Exceptions: ArgumentException, ConnectionFailureException
                throw;
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        public void Logout()
        {
            state = UserState.Guest;
        }

        /// <summary>
        /// Registers a new account with GameAnywhere
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        /// <returns>
        /// 1 - Registration complete
        /// 2 - Duplicate account
        /// 3 - Unactivated account
        /// </returns>
        /// Exceptions: ArgumentException, SmtpException, ConnectionFailureException, AmazonSimpleDBException
        public int Register(string email, string password)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (password.Trim().Equals("") || password == null)
                throw new ArgumentException("Parameter cannot be empty/null", "password");

            Debug.Assert(password.Length >= 6);

            try
            {
                if (!UserExists(email))
                {
                    //Registration complete
                    string activationKey = CreateMD5Hash(email + password);
                    db.InsertItem(email, password, activationKey);
                    SendActivationEmail(email, password, activationKey);
                    return 1;
                }
                else
                {
                    if (IsAccountActivated(email))
                    {
                        //Duplicate account "Account has already been registered"
                        return 2;
                    }
                    else
                    {
                        //Unactivated account "Account has already been registered and needs activation"
                        return 3;
                    }
                }
            }
            catch
            {
                //Exceptions: ArgumentException, SmtpException, ConnectionFailureException, AmazonSimpleDBException
                throw;
            }
        }

        /// <summary>
        /// Retrieve user’s password.
        /// An email will be sent to the user’s email with the password.
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// 1 - Retrieve Complete – Emailed user’s password successfully
        /// 2 - Unactivated account - user registered but account not activated yet
        /// 3 - Invalid User – User does not exist in GameAnywhere DB
        /// </returns>
        /// Exceptions: ArgumentException, SmtpException, ConnectionFailureException
        public int RetrievePassword(string email)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            try
            {
                if (UserExists(email))
                {
                    if (IsAccountActivated(email))
                    {
                        //Retrieve complete
                        string password = db.GetAttribute(email, "Password");
                        SendPasswordEmail(email, password);
                        return 1;
                    }
                    else
                    {
                        //Unactivated account
                        return 2;
                    }
                }
                else
                {
                    //Invalid user
                    return 3;
                }
            }
            catch
            {
                //Exceptions: ArgumentException, SmtpException, ConnectionFailureException
                throw;
            }
        }

        /// <summary>
        /// Change user's password
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="oldPassword">user old password</param>
        /// <param name="newPassword">user new password</param>
        /// <returns>
        /// 1 - Password changed
        /// 2 - Invalid old password
        /// 3 - Invalid user
        /// </returns>
        /// Exceptions: ArgumentException, SmtpException, ConnectionFailureException, AmazonSimpleDBException
        public int ChangePassword(string email, string oldPassword, string newPassword)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (oldPassword.Trim().Equals("") || oldPassword == null)
                throw new ArgumentException("Parameter cannot be empty/null", "oldPassword");
            if (newPassword.Trim().Equals("") || newPassword == null)
                throw new ArgumentException("Parameter cannot be empty/null", "newPassword");

            Debug.Assert(newPassword.Length >= 6);

            try
            {
                if (UserExists(email) && IsAccountActivated(email))
                {
                    string userPassword = db.GetAttribute(email, "Password");
                    if (userPassword.Equals(oldPassword))
                    {
                        //Password changed
                        db.UpdateAttributeValue(email, "Password", newPassword);
                        return 1;
                    }
                    else
                    {
                        //Invalid password
                        return 2;
                    }
                }
                else
                {
                    //Invalid user
                    return 3;
                }
            }
            catch
            {
                //Exceptions: ArgumentException, SmtpException, ConnectionFailureException, AmazonSimpleDBException
                throw;
            }
        }

        /// <summary>
        /// Resends activation email to user to activate account
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// 1 - Activation email sent. Successfully emailed user a link to activate his/her account
        /// 2 - Account already activated
        /// 3 - Invalid user. User does not exist in GameAnywhere DB
        /// </returns>
        /// Exceptions: ArgumentException, ConnectionFailureException, SmtpException
        public int ResendActivation(string email, string inputPassword)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (inputPassword.Trim().Equals("") || inputPassword == null)
                throw new ArgumentException("Parameter cannot be empty/null", "inputPassword");

            Debug.Assert(inputPassword.Length >= 6);

            try
            {
                if (UserExists(email))
                {
                    if (!IsAccountActivated(email))
                    {
                        string password = db.GetAttribute(email, "Password");
                        string activationKey = CreateMD5Hash(email + password);
                        //Wrong password
                        if (!inputPassword.Equals(password))
                        {
                            return 0;
                        }
                        else
                        {
                            SendActivationEmail(email, password, activationKey);
                            return 1;
                        }              
                    }
                    else
                    {
                        //Account already activated
                        return 2;
                    }
                }
                else
                {
                    //Invalid user
                    return 3;
                }
            }
            catch
            {
                //Exceptions: ArgumentException, ConnectionFailureException, SmtpException
                throw;
            }
        }

        /// <summary>
        /// Checks if user is logged in to GameAnywhere
        /// </summary>
        /// <returns>
        /// true - User is logged in, UserState is UserLoggedIn
        /// false - User is not logged in, UserState is Guest
        /// </returns>
        public bool IsLoggedIn()
        {
            if (state == UserState.UserLoggedIn)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if user exists in GameAnywhere DB
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// true - User exists in DB
        /// false - User not found in DB
        /// </returns>
        /// Exceptions: ArgumentException, ConnectionFailureException
        private bool UserExists(string email)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            try
            {
                if (db.ItemExists(email))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //Exceptions: ConnectionFailureException
                throw;
            }
        }

        /// <summary>
        /// Checks if user’s account has been activated in DB
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// true - User is already activated
        /// false - User is not activated
        /// </returns>
        /// Exceptions: ArgumentException, ConnectionFailureException
        private bool IsAccountActivated(string email)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            try
            {
                string activationStatus = db.GetAttribute(email, "ActivationStatus");

                if (activationStatus.Equals("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //Exceptions: ConnectionFailureException
                throw;
            }
        }

        /// <summary>
        /// Sends activation email to user
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        /// <param name="activationKey">activation key</param>
        /// Exceptions: ArgumentException, SmtpException
        private void SendActivationEmail(string email, string password, string activationKey)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (password.Trim().Equals("") || password == null)
                throw new ArgumentException("Parameter cannot be empty/null", "password");
            if (activationKey.Trim().Equals("") || activationKey == null)
                throw new ArgumentException("Parameter cannot be empty/null", "activationKey");

            Debug.Assert(password.Length >= 6);

            //Setup mail message
            MailMessage message = new MailMessage();
            message.From = new MailAddress("gameanywhere@gmail.com", "GameAnywhere");
            message.To.Add(email);
            message.Subject = "[GameAnywhere] Account Activation";
            message.IsBodyHtml = true;
            message.Body = string.Format(@"
                <html><body>
                    Thank you for registering with GameAnywhere!<br><br>
                    Your account still needs to be activated in order to use GameAnywhere's web features.<br>
                    Please click on the link below to activate your account:<br>
                    <a href=""http://dailysneakers.com/activate/?key={0}"">http://dailysneakers.com/activate/?key={0}</a>
                    <br><br>
                    You may login with the following details after activation:<br>
                    Username: {1}<br>
                    Password: {2}<br>
                    <br><br>
                    Regards,<br>
                    <b>GameAnywhere Team</b>
                </body></html>
                ", activationKey, email, password);

            //Setup smtp client
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("gameanywhere@gmail.com", "cs3215666666");

            //Send email
            try
            {
                client.Send(message);
                message = null;
                client = null;
            }
            catch
            {
                //Exceptions: System.Net.Mail.SmtpException, System.Net.Mail.SmtpFailedRecipientsException - Failure sending mail
                throw new SmtpException("Failure sending email.");
            }
        }

        /// <summary>
        /// Sends user's password by email
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        /// Exceptions: ArgumentException, SmtpException
        private void SendPasswordEmail(string email, string password)
        {
            //Pre-conditions
            if (email.Trim().Equals("") || email == null)
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (password.Trim().Equals("") || password == null)
                throw new ArgumentException("Parameter cannot be empty/null", "password");

            Debug.Assert(password.Length >= 6);

            //Setup mail message
            MailMessage message = new MailMessage();
            message.From = new MailAddress("gameanywhere@gmail.com", "GameAnywhere");
            message.To.Add(email);
            message.Subject = "[GameAnywhere] Password Retrieval";
            message.IsBodyHtml = true;
            message.Body = string.Format(@"
                <html><body>
                    You have just requested to retrieve your password. Your login details are as follows:<br><br>
                    Username: {0}<br>
                    Password: {1}<br>
                    <br><br>
                    Regards,<br>
                    <b>GameAnywhere Team</b>
                </body></html>
                ", email, password);

            //Setup smtp client
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("gameanywhere@gmail.com", "cs3215666666");

            //Send email
            try
            {
                client.Send(message);
                message = null;
                client = null;
            }
            catch (Exception)
            {
                //Exceptions: System.Net.Mail.SmtpException, System.Net.Mail.SmtpFailedRecipientsException - Failure sending mail
                throw new SmtpException("Failure sending email.");
            }
        }

        /// <summary>
        /// Used to generate the activation key
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A unique string of uppercase characters</returns>
        /// Exceptions: ArgumentException
        private string CreateMD5Hash(string input)
        {
            //Pre-conditions
            if (input.Trim().Equals("") || input == null)
                throw new ArgumentException("Parameter cannot be empty/null", "input");

            //Use input string to calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            //Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sb.Append(hashBytes[i].ToString("x2")); 
            }
            return sb.ToString();
        }
    }
}
