//Eu Xing Long Nicholas
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Diagnostics;
using Amazon.SimpleDB;


namespace GameAnywhere.Process
{
    /// <summary>
    /// User account information and its related methods.
    /// </summary>
    public class User
    {
        #region Data Members
        /// <summary>
        /// User's Email which will be use as a username for login.
        /// </summary>
        private string email;

        /// <summary>
        /// User's password.
        /// </summary>
        private string password;

        /// <summary>
        /// To access Amazon Web Services SimpleDB.
        /// </summary>
        private SimpleDB db;

        /// <summary>
        /// State of user, either UserLoggedIn or Guest.
        /// </summary>
        private UserState state;
        enum UserState { UserLoggedIn, Guest };
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the data members.
        /// </summary>
        public User()
        {
            email = string.Empty;
            password = string.Empty;
            db = new SimpleDB();
            state = UserState.Guest;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Properties for Email and Password.
        /// </summary>
        /// <value>Get and Set user's email.</value>
        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        /// <value>Get and Set user's password.</value>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Login user.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="password">User password.</param>
        /// <returns>
        /// True - User exists and State changes to UserLoggedIn.
        /// False - Wrong password / User does not exist / User exists but not activated.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public bool Login(string email, string password)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Parameter cannot be empty/null", "password");

            Debug.Assert(password.Length >= 6);

            if (UserExists(email) && IsAccountActivated(email))
            {
                //Login user
                string userPassword = db.GetAttribute(email, "Password");
                string md5Password = CreateMD5Hash(email + password);
                if (userPassword.Equals(md5Password))
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

        /// <summary>
        /// Logout user.
        /// </summary>
        public void Logout()
        {
            state = UserState.Guest;
            email = string.Empty;
            password = string.Empty;
        }

        /// <summary>
        /// Registers a new account.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="password">User password.</param>
        /// <returns>
        /// 1 - Registration complete.
        /// 2 - Duplicate account.
        /// 3 - Unactivated account.
        /// </returns>
        /// <exception cref="AmazonSimpleDBException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public int Register(string email, string password)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Parameter cannot be empty/null", "password");

            Debug.Assert(password.Length >= 6);

            if (!UserExists(email))
            {
                //Registration complete
                string activationKey = CreateMD5Hash(email.Substring(email.Length / 2) + password);
                string md5Password = CreateMD5Hash(email + password);

                //Insert user info into DB
                db.InsertItem(email, md5Password, activationKey);
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

        /// <summary>
        /// Retrieve user’s password.
        /// An email will be sent to the user’s email with the password.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <returns>
        /// 1 - Retrieve Complete – Emailed user’s password successfully.
        /// 2 - Unactivated account - user registered but account not activated yet.
        /// 3 - Invalid User – User does not exist in GameAnywhere DB.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public int RetrievePassword(string email)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            if (UserExists(email))
            {
                if (IsAccountActivated(email))
                {
                    //Retrieve complete
                    string password = db.GetAttribute(email, "Password");
                    string resetKey = CreateMD5Hash(email + password);

                    //Insert new attribute to item DB
                    db.AddAttributeValue(email, "ResetKey", resetKey);
                    SendPasswordEmail(email, resetKey);
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

        /// <summary>
        /// Change user's password.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="oldPassword">User old password.</param>
        /// <param name="newPassword">User new password.</param>
        /// <returns>
        /// 1 - Password changed.
        /// 2 - Invalid old password.
        /// 3 - Invalid user.
        /// </returns>
        /// <exception cref="AmazonSimpleDBException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public int ChangePassword(string email, string oldPassword, string newPassword)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(oldPassword))
                throw new ArgumentException("Parameter cannot be empty/null", "oldPassword");
            if (String.IsNullOrEmpty(newPassword))
                throw new ArgumentException("Parameter cannot be empty/null", "newPassword");

            Debug.Assert(newPassword.Length >= 6);

            if (UserExists(email) && IsAccountActivated(email))
            {
                string userPassword = db.GetAttribute(email, "Password");
                string md5OldPassword = CreateMD5Hash(email + oldPassword);

                //Check if current password matches in DB
                if (userPassword.Equals(md5OldPassword))
                {
                    //Change to new password and update item in DB
                    string md5NewPassword = CreateMD5Hash(email + newPassword);
                    db.UpdateAttributeValue(email, "Password", md5NewPassword);
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

        /// <summary>
        /// Resends activation email to user to activate account.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <returns>
        /// 0 - Account not activated and password provided is wrong.
        /// 1 - Activation email sent. Successfully emailed user a link to activate his/her account.
        /// 2 - Account already activated.
        /// 3 - Invalid user. User does not exist in GameAnywhere DB.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public int ResendActivation(string email, string inputPassword)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(inputPassword))
                throw new ArgumentException("Parameter cannot be empty/null", "inputPassword");

            Debug.Assert(inputPassword.Length >= 6);

            if (UserExists(email))
            {
                if (!IsAccountActivated(email))
                {
                    string password = db.GetAttribute(email, "Password");
                    string md5InputPassword = CreateMD5Hash(email + inputPassword);
                    string activationKey = CreateMD5Hash(email.Substring(email.Length / 2) + inputPassword);

                    //Account not activated and wrong password provided
                    if (!md5InputPassword.Equals(password))
                    {
                        return 0;
                    }
                    else
                    {
                        SendActivationEmail(email, inputPassword, activationKey);
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

        /// <summary>
        /// Checks if user is logged in.
        /// </summary>
        /// <returns>
        /// True - User is logged in, UserState is UserLoggedIn.
        /// False - User is not logged in, UserState is Guest.
        /// </returns>
        public bool IsLoggedIn()
        {
            if (state == UserState.UserLoggedIn)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Checks if user exists in DB.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <returns>
        /// True - User exists in DB.
        /// False - User not found in DB.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        private bool UserExists(string email)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            if (db.ItemExists(email))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if user’s account has been activated in DB.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <returns>
        /// True - User is already activated.
        /// False - User is not activated.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        private bool IsAccountActivated(string email)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");

            string activationStatus = db.GetAttribute(email, "ActivationStatus");

            //Checks if account is activated
            if (activationStatus.Equals("1"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends activation email to user.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="password">User password.</param>
        /// <param name="activationKey">Activation key.</param>
        /// <exception cref="ArgumentException"></exception>
        private bool SendActivationEmail(string email, string password, string activationKey)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Parameter cannot be empty/null", "password");
            if (String.IsNullOrEmpty(activationKey))
                throw new ArgumentException("Parameter cannot be empty/null", "activationKey");

            Debug.Assert(password.Length >= 6);

            //Setup mail message
            MailMessage message = GetActivationMessage(email, password, activationKey);

            //Setup smtp client
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("gameanywhere@gmail.com", "*");

            //Send email
            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception)
            {
                //Exceptions: System.Net.Mail.SmtpException, System.Net.Mail.SmtpFailedRecipientsException - Failure sending mail
                return false;
            }
        }

        private MailMessage GetActivationMessage(string email, string password, string activationKey)
        {
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
                    <a href=""http://game-anywhere.com/activate/?key={0}"">http://game-anywhere.com/activate/?key={0}</a>
                    <br><br>
                    You may login with the following details after activation:<br>
                    Username: {1}<br>
                    Password: {2}<br>
                    <br><br>
                    Regards,<br>
                    <b>GameAnywhere Team</b>
                </body></html>
                ", activationKey, email, password);
            return message;
        }

        /// <summary>
        /// Sends user's password by email.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="resetKey">Key to reset password.</param>
        /// <exception cref="ArgumentException"></exception>
        private bool SendPasswordEmail(string email, string resetKey)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("Parameter cannot be empty/null", "email");
            if (String.IsNullOrEmpty(resetKey))
                throw new ArgumentException("Parameter cannot be empty/null", "resetKey");

            //Setup mail message
            MailMessage message = GetRetrievePasswordMessage(email, resetKey);

            //Setup smtp client
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("gameanywhere@gmail.com", "*");

            //Send email
            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception)
            {
                //Exceptions: System.Net.Mail.SmtpException, System.Net.Mail.SmtpFailedRecipientsException - Failure sending mail
                return false;
            }
        }

        private MailMessage GetRetrievePasswordMessage(string email, string resetKey)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress("gameanywhere@gmail.com", "GameAnywhere");
            message.To.Add(email);
            message.Subject = "[GameAnywhere] Password Retrieval";
            message.IsBodyHtml = true;
            message.Body = string.Format(@"
                <html><body>
                    You have just requested to retrieve your password. We need you to reset your password in order to maintain security of your account.<br><br>
                    Please click on the link below to reset your password:<br>
                    <a href=""http://game-anywhere.com/reset/?key={0}"">http://game-anywhere.com/reset/?key={0}</a>
                    <br><br>
                    You may choose to ignore this email if you did not made this request.
                    <br><br>
                    Regards,<br>
                    <b>GameAnywhere Team</b>
                </body></html>
                ", resetKey);
            return message;
        }

        /// <summary>
        /// Used to generate the activation key.
        /// </summary>
        /// <param name="input">Any string.</param>
        /// <returns>A unique string of uppercase characters.</returns>
        /// <exception cref="ArgumentException"></exception>
        private string CreateMD5Hash(string input)
        {
            //Pre-conditions
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("Parameter cannot be empty/null", "input");

            //Use input string to calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            //Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion
    }
}
