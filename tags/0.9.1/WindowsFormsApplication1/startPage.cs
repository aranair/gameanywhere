﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

namespace GameAnywhere
{
    public partial class startPage : Form
    {
        #region Constructors and Propeties
        /// <summary>
        /// The controller that is required to call all the synchronization functions.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// Determines if there is a child form active.
        /// </summary>
        public bool ChildActive { get; set; }

        /// <summary>
        /// Determine if the user has clicked the "exit" button in this form to exit.
        /// </summary>
        public bool UserClosing { get; set; }

        /// <summary>
        /// List of all the panels in the form, used for further manipulation
        /// </summary>
        private List<Panel> panelList;

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="controller"></param>
        public startPage(Controller controller)
        {
            InitializeComponent();
            this.controller = controller;
            ChildActive = false;
            ResetErrorLabels();

            SetPanelList();

        }

        /// <summary>
        /// Helper function for the Constructor.
        /// It adds all the panels in this form into the list of panels to be manipulated later on
        /// </summary>
        private void SetPanelList()
        {
            panelList = new List<Panel>();
            panelList.Add(startPanel);
            panelList.Add(loginPanel);
            panelList.Add(registerPanel);
            panelList.Add(forgetPasswordPanel);
            panelList.Add(resendActivationPanel);
            panelList.Add(changePasswordPanel);
        }
        #endregion

        // Mouse event handlers, and other helper functions for all the buttons in the START Panel. (Subsequent Regions as well)
        #region Start Panel
        // This region handles all the mouse event handlers for the Computer To Thumbdrive synchronization button.
        #region computerToThumbdriveButton
        private void computerToThumbdriveButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(computerToThumbdriveButton, "GameAnywhere.Resources.computerToThumbdriveButtonMouseOver.gif", ImageLayout.Zoom);
        }

        private void computerToThumbdriveButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(computerToThumbdriveButton, "GameAnywhere.Resources.computerToThumbdriveButton.gif", ImageLayout.Zoom);
        }

        private void computerToThumbdriveButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(computerToThumbdriveButton, "GameAnywhere.Resources.computerToThumbdriveButtonMouseDown.gif", ImageLayout.Zoom);
        }

        private void computerToThumbdriveButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(computerToThumbdriveButton, "GameAnywhere.Resources.computerToThumbdriveButton.gif", ImageLayout.Zoom);
        }

        /// <summary>
        /// Handles actions for the form when Computer to Thumbdrive synchronization button is clicked.
        /// Pre-Condition: User clicked Computer to Thumbdrive synchronization button.
        /// Post-Condition: List of compatible games found from controller, then sent to ChooseGame form to be displayed.
        /// Or display an error if there are no compatible games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void computerToThumbdriveButton_MouseClick(object sender, MouseEventArgs e)
        {
            // This resets all the error labels that are in this form.
            ResetErrorLabels();

            controller.SetSyncDirection(OfflineSync.ComToExternal);

            // Get the list of compatible games to be displayed to user.
            List<Game> gameList = controller.GetGameList();

            // If there are no games.
            if (gameList.Count > 0)
            {
                errorLabel.Text = "";

                // Show the ChooseGame form for user to choose the files.
                ChooseGame chooseGameForm = new ChooseGame(controller, gameList, ref this.errorLabel);
                chooseGameForm.ShowDialog();
            }
            else
                errorLabel.Text = "No compatible games found.";
        }
        #endregion

        #region thumbdriveToComputerButton
        private void thumbdriveToComputerButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(thumbdriveToComputerButton, "GameAnywhere.Resources.thumbdriveToComputerButtonMouseOver.gif", ImageLayout.Zoom);
        }

        private void thumbdriveToComputerButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(thumbdriveToComputerButton, "GameAnywhere.Resources.thumbdriveToComputerButton.gif", ImageLayout.Zoom);
        }

        private void thumbdriveToComputerButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(thumbdriveToComputerButton, "GameAnywhere.Resources.thumbdriveToComputerButtonMouseDown.gif", ImageLayout.Zoom);
        }

        private void thumbdriveToComputerButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(thumbdriveToComputerButton, "GameAnywhere.Resources.thumbdriveToComputerButton.gif", ImageLayout.Zoom);
        }

        private void thumbdriveToComputerButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();
            controller.SetSyncDirection(OfflineSync.ExternalToCom);

            List<Game> gList = controller.GetGameList();

            if (gList.Count > 0)
            {
                errorLabel.Visible = false;
                ChooseGame cg = new ChooseGame(controller, gList, ref this.errorLabel);
                cg.ShowDialog();
            }
            else
            {
                errorLabel.Text = "No compatible games found.";
                errorLabel.Visible = true;
            }
        }
        #endregion

        #region registerButton
        private void registerButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(registerButton, "GameAnywhere.Resources.registerButtonMouseOver.gif", ImageLayout.Center);
        }

        private void registerButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(registerButton, "GameAnywhere.Resources.registerButton.gif", ImageLayout.Center);
        }

        private void registerButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(registerButton, "GameAnywhere.Resources.registerButtonMouseDown.gif", ImageLayout.Center);
        }

        private void registerButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(registerButton, "GameAnywhere.Resources.registerButtonMouseOver.gif", ImageLayout.Center);
        }
        private void registerButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateRegisterPanel();
        }
        #endregion

        #region loginButton
        private void loginButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(loginButton, "GameAnywhere.Resources.loginButtonMouseOver.gif", ImageLayout.Center);
        }

        private void loginButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(loginButton, "GameAnywhere.Resources.loginButton.gif", ImageLayout.Center);
        }

        private void loginButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(loginButton, "GameAnywhere.Resources.loginButtonMouseDown.gif", ImageLayout.Center);
        }

        private void loginButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(loginButton, "GameAnywhere.Resources.loginButtonMouseOver.gif", ImageLayout.Center);
        }
        private void loginButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateLoginPanel();
        }
        #endregion

        #region exitButton
        private void exitButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(exitButton, "GameAnywhere.Resources.exitButtonMouseOver.gif", ImageLayout.Center);
        }

        private void exitButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(exitButton, "GameAnywhere.Resources.exitButton.gif", ImageLayout.Center);
        }

        private void exitButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(exitButton, "GameAnywhere.Resources.exitButtonMouseDown.gif", ImageLayout.Center);
        }

        private void exitButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(exitButton, "GameAnywhere.Resources.exitButtonMouseOver.gif", ImageLayout.Center);
        }
        private void exitButton_MouseClick(object sender, MouseEventArgs e)
        {
            // There is backup -> run warning dialog
            if (controller.EndProgram())
            {
                restoreWarning formRestoreWarning = new restoreWarning(this, controller);
                DisplayWarningDialog(formRestoreWarning);
            }
            else
            {
                UserClosing = true;
                this.Close();
            }

        }

        #endregion
        #endregion

        #region Login Panel ( and all it's content )

        // Mouse event handlers for the smaller, white buttons in the Login Panel namely: 
        // Resend Activation, Forget Password, Change Password buttons
        #region resendActivationLoginPanelButton
        private void resendActivationLoginPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateResendActivationPanel();
        }

        private void resendActivationLoginPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetMouseDownColor(resendActivationLoginPanelButton);
        }

        private void resendActivationLoginPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetMouseEnterColor(resendActivationLoginPanelButton);
        }


        private void resendActivationLoginPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetMouseLeaveColor(resendActivationLoginPanelButton);
        }

        private void resendActivationLoginPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetMouseLeaveColor(resendActivationLoginPanelButton);
        }
        #endregion
        #region forgetPasswordLoginPanelButton
        private void forgetPasswordLoginPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateForgetPasswordPanel();
        }

        private void forgetPasswordLoginPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetMouseDownColor(forgetPasswordLoginPanelbutton);
        }

        private void forgetPasswordLoginPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetMouseEnterColor(forgetPasswordLoginPanelbutton);
        }

        private void forgetPasswordLoginPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetMouseLeaveColor(forgetPasswordLoginPanelbutton);
        }

        private void forgetPasswordLoginPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetMouseLeaveColor(forgetPasswordLoginPanelbutton);
        }
        #endregion
        #region changePassswordLoginPanelButton
        private void changePasswordLoginPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateChangePasswordPanel();
        }



        private void changePasswordLoginPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetMouseDownColor(changePasswordLoginPanelButton);
        }

        private void changePasswordLoginPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetMouseEnterColor(changePasswordLoginPanelButton);
        }

        private void changePasswordLoginPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetMouseLeaveColor(changePasswordLoginPanelButton);
        }

        private void changePasswordLoginPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetMouseLeaveColor(changePasswordLoginPanelButton);
        }
        #endregion
        // Centralized controls for the above 3 buttons.
        #region grayButton colour and mouseclick events
        private void cancelGrayButtons_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateLoginPanel();
        }
        private void SetMouseDownColor(Button button)
        {
            button.ForeColor = System.Drawing.Color.DimGray;
        }
        private void SetMouseEnterColor(Button button)
        {
            button.ForeColor = System.Drawing.Color.DarkGray;
        }
        private void SetMouseLeaveColor(Button button)
        {
            button.ForeColor = System.Drawing.Color.LightGray;
        }
        #endregion

        // Mouse event handlers for login, cancel buttons.
        #region loginLoginPanelButton
        private void loginLoginPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(loginLoginPanelButton, "GameAnywhere.Resources.loginButtonMouseOver.gif", ImageLayout.Center);
        }

        private void loginLoginPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(loginLoginPanelButton, "GameAnywhere.Resources.loginButton.gif", ImageLayout.Center);
        }

        private void loginLoginPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(loginLoginPanelButton, "GameAnywhere.Resources.loginButtonMouseDown.gif", ImageLayout.Center);
        }

        private void loginLoginPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(loginLoginPanelButton, "GameAnywhere.Resources.loginButton.gif", ImageLayout.Center);
        }
        /// <summary>
        /// Mouse CLICK event handler for the login button in the Login Panel.
        /// Pre-condition: Login button in login Panel is clicked.
        /// Post-condition: Either login the user, or display the appropriate errors according to the user details entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginLoginPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();

            // Login Details are VALID ( before checking with data base )
            if (CheckLoginDetails())
            {
                // Log out current user if any.
                controller.Logout();

                // Attempt to login.
                bool loginResult = false;
                try
                {
                    // loginResult = false if login fails.
                    // loginResult = true  if login succeeds.
                    loginResult = controller.Login(emailTextBox.Text, passwordTextBox.Text);
                }
                catch (ConnectionFailureException)
                {
                    MessageBox.Show("Unable to connect to Web Server");
                    return;
                }

                // Login succeeds
                if (loginResult)
                {
                    // Go back to first page and display success text.
                    InitiateStartPanel();
                    SetErrorLabel("User Logged in.", System.Drawing.Color.DeepSkyBlue);
                }
                // Login fails
                else 
                {
                    loginFailedLabel.Text = "Invalid user details";
                }
                ResetAllLoginPanelTextBoxes();
            }
        }

        /// <summary>
        /// Responsible for preliminary checks of the validity of the login details.
        /// </summary>
        /// <returns>True if login details are error free. False if login details are invalid.</returns>
        private bool CheckLoginDetails()
        {
            bool isErrorFree = true;
            
            // Password check
            string passwordFeedback = IsValidPassword(passwordTextBox.Text);

            // Email check
            string emailFeedback = IsValidEmail(emailTextBox.Text);

            // Email is invalid
            if (!emailFeedback.Equals("Email Perfect"))
            {
                loginFailedLabel.Text = emailFeedback + " ";
                isErrorFree = false;
            }
            // If email is valid, and password is invalid.
            else if (!passwordFeedback.Equals("Password Perfect"))
            {
                loginFailedLabel.Text += passwordFeedback;
                isErrorFree = false;
            }
            return isErrorFree;
        }
        #endregion
        #region cancelLoginPanelButton
        private void cancelLoginPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelLoginPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }

        private void cancelLoginPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelLoginPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }

        private void cancelLoginPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelLoginPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }

        private void cancelLoginPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelLoginPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }
        private void cancelLoginPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateStartPanel();
        }
        #endregion
        
        /// <summary>
        /// Resets a particular input box in Login Panel according to the string passed in.
        /// </summary>
        /// <param name="p">Either: "Password" or "Email"</param>
        private void ResetLoginPanelTextBoxes(string p)
        {
            if (p.Equals("Password"))
            {
                passwordTextBox.Text = "";
            }
            else if (p.Equals("Email"))
            {
                emailTextBox.Text = "";
            }

        }

        #endregion

        #region RegisterPanel

        #region registerRegisterPanelButton
        private void registerRegisterPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(registerRegisterPanelButton, "GameAnywhere.Resources.registerButtonMouseOver.gif", ImageLayout.Center);
        }
        private void registerRegisterPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(registerRegisterPanelButton, "GameAnywhere.Resources.registerButton.gif", ImageLayout.Center);
        }
        private void registerRegisterPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(registerRegisterPanelButton, "GameAnywhere.Resources.registerButtonMouseDown.gif", ImageLayout.Center);
        }
        private void registerRegisterPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(registerRegisterPanelButton, "GameAnywhere.Resources.registerButtonMouseOver.gif", ImageLayout.Center);
        }

        /// <summary>
        /// Handles mouse click event for the Register button in the Register Panel
        /// Pre-condition: User has clicked the register button.
        /// Post-condition: User is either registered or the appropriate error message is displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void registerRegisterPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();
            
            // If email is valid, and password is valid.
            if (CheckEmailErrorsRegisterPanel() && CheckPasswordErrorsRegisterPanel())
            {
                // Login details are valid.

                // This portion tries to register user and gets appropriate feedback for it.
                int errorCode = 0;
                try
                {
                     errorCode = controller.Register(emailRegisterPanelTextBox.Text, passwordRegisterPanelTextBox.Text);
                }
                catch (ConnectionFailureException)
                {
                    MessageBox.Show("Unable to connect to Web Server");
                    return;
                }
                
                switch (errorCode)
                {
                    case 1:
                        {
                            InitiateStartPanel();
                            SetErrorLabel("Registration Complete", System.Drawing.Color.DeepSkyBlue);
                            break;
                        }// Successfully Registered.
                    case 2:
                        {
                            emailInvalidLabel.Text = "Email is already taken.";
                            break;
                        } // Duplicate Account
                    case 3:
                        {
                            emailInvalidLabel.Text = "Email is registered but not activated.";
                            break;
                        } // Unactivated Account
                    case 4:
                        {
                            emailInvalidLabel.Text = "Invalid email";
                            break;
                        }  // Invalid Email
                    default: break;
                }
                
                ResetAllRegisterPanelTextboxes();
            }

        }
        #endregion

        #region cancelRegisterPanelButton
        private void cancelRegisterPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelRegisterPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }

        private void cancelRegisterPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelRegisterPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }

        private void cancelRegisterPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelRegisterPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }

        private void cancelRegisterPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelRegisterPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }
        private void cancelRegisterPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            InitiateStartPanel();
        }
        #endregion

        /// <summary>
        /// Checks password input fields for the Register Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if password is valid. False if password is invalid.</returns>
        private bool CheckPasswordErrorsRegisterPanel()
        {
            bool isErrorFree = true;
            
            // Gets error feedback from the helper function regarding the password entered by user in the Register Panel
            string passwordFeedback = IsValidPassword(passwordRegisterPanelTextBox.Text);

            // If Password is invalid.
            if (!passwordFeedback.Equals("Password Perfect"))
            {
                passwordInvalidLabel.Text = passwordFeedback;
                ResetRegisterPanelTextBoxes("Password");
                isErrorFree = false;
            }
            // Password and Confirm Password fields mismatch.
            else if (!passwordRegisterPanelTextBox.Text.Equals(passwordReEnterRegisterPanelTextBox.Text))
            {
                passwordInvalidLabel.Text = "Passwords do not match.";
                ResetRegisterPanelTextBoxes("Password");
                isErrorFree = false;
            }

            return isErrorFree;
        }

        /// <summary>
        /// Checks email input fields for the Register Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if email is valid. False if email is invalid.</returns>
        private bool CheckEmailErrorsRegisterPanel()
        {
            bool isErrorFree = true;
            // Gets feedback from IsValidEmail Helper function regarding the email entered in the Register panel
            string emailFeedback = IsValidEmail(emailRegisterPanelTextBox.Text);
            if (!emailFeedback.Equals("Email Perfect"))
            {
                emailInvalidLabel.Text = emailFeedback;
                ResetRegisterPanelTextBoxes("Email");
                isErrorFree = false;
            }
            return isErrorFree;
        }

        /// <summary>
        /// Resets a particular input box in Register Panel according to the string passed in.
        /// </summary>
        /// <param name="p">Either: "Password" or "Email"</param>
        private void ResetRegisterPanelTextBoxes(string p)
        {
            if (p.Equals("Password"))
            {
                passwordTextBox.Text = "";
                passwordRegisterPanelTextBox.Text = "";
                passwordReEnterRegisterPanelTextBox.Text = "";
            }
            else if (p.Equals("Email"))
            {
                emailRegisterPanelTextBox.Text = "";
            }
        }

        
        #endregion

        #region Change Password Panel

        // Mouse Event Handlers
        #region cancelChangePasswordPanelButton
        private void cancelChangePasswordPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelChangePasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }

        private void cancelChangePasswordPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelChangePasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }

        private void cancelChangePasswordPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelChangePasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }

        private void cancelChangePasswordPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelChangePasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }
        #endregion
        #region confirmButton ( Change Password )

        /// <summary>
        /// Handles mouse click event for Confirm Button for the Change Password Panel.
        /// Pre-condition: User has clicked the Confirm Button for the Change Password panel.
        /// Post-condition: User has either successfully changed his/her password or error messages are displayed to him.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmChangePasswordPanelButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();
            if (CheckEmailErrorsChangePasswordPanel() && CheckPasswordErrorsChangePasswordPanel())
            {
                int errorCode = 0;
                try
                {
                    // Attempts to change password with the user entered data and gets feedback regarding the success of the operation.
                    errorCode = controller.ChangePassword(emailChangePasswordPanelTextBox.Text, passwordChangePasswordPanelTextBox.Text, newpasswordChangePasswordPanelTextBox.Text);
                }
                catch (ConnectionFailureException)
                {
                    MessageBox.Show("Unable to connect to Web Server");
                    return;
                }

                switch (errorCode)
                {
                    case 1:
                        {
                            InitiateStartPanel();
                            SetErrorLabel("Password successfully changed.", System.Drawing.Color.DeepSkyBlue);
                            break;
                        }   // Successfully changed password.
                    case 2:
                        {
                            userDetailsInvalidChangePasswordPanelLabel.Text = "Invalid user details.";
                            break;
                        }   // Invalid user details.
                    case 3:
                        {
                            userDetailsInvalidChangePasswordPanelLabel.Text = "Invalid user details.";
                            break;
                        }   // Invalid user details.

                    default: break;
                }
                ResetAllChangePasswordPanelTextBoxes();

            }
        }
        private void confirmChangePasswordPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(confirmChangePasswordPanelButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonMouseDown.gif", ImageLayout.Zoom);
        }
        private void confirmChangePasswordPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(confirmChangePasswordPanelButton, "GameAnywhere.Resources.confirmGameChoicePopupButtonMouseOver.gif", ImageLayout.Zoom);
        }
        private void confirmChangePasswordPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(confirmChangePasswordPanelButton, "GameAnywhere.Resources.confirmGameChoicePopupButton.gif", ImageLayout.Zoom);
        }
        private void confirmChangePasswordPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(confirmChangePasswordPanelButton, "GameAnywhere.Resources.confirmGameChoicePopupButton.gif", ImageLayout.Zoom);
        }

        #endregion

        /// <summary>
        /// Checks password input fields for the Change Password Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if all password fields in Change Password panel is valid. False if any one of them is invalid.</returns>
        private bool CheckPasswordErrorsChangePasswordPanel()
        {
            bool isErrorFree = true;
            // Checks and gets feedback for password and new password fields.
            string passwordFeedback = IsValidPassword(passwordChangePasswordPanelTextBox.Text);
            string newPasswordFeedback = IsValidPassword(newpasswordChangePasswordPanelTextBox.Text);
            
            // Original password is invalid.
            if (!passwordFeedback.Equals("Password Perfect"))
            {
                userDetailsInvalidChangePasswordPanelLabel.Text = passwordFeedback;
                ResetChangePasswordPanelTextBoxes("Password");
                isErrorFree = false;
            }
            // New password is invalid.
            else if (!newPasswordFeedback.Equals("Password Perfect"))
            {
                newpasswordMismatchChangePasswordPanelLabel.Text = newPasswordFeedback;
                ResetChangePasswordPanelTextBoxes("New Password");
                isErrorFree = false;
            }
            // New passwords mismatch
            else if (!newpasswordChangePasswordPanelTextBox.Text.Equals(confirmNewpasswordChangePasswordPanelTextBox.Text))
            {
                newpasswordMismatchChangePasswordPanelLabel.Text = "Passwords do not match.";
                ResetChangePasswordPanelTextBoxes("New Password");
                isErrorFree = false;
            }
            return isErrorFree;
        }

        /// <summary>
        /// Checks email input fields for the Change Password Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if email is valid. False if email is invalid.</returns>
        private bool CheckEmailErrorsChangePasswordPanel()
        {
            bool isErrorFree = true;
            string emailFeedback = IsValidEmail(emailChangePasswordPanelTextBox.Text);
            if (!emailFeedback.Equals("Email Perfect"))
            {
                userDetailsInvalidChangePasswordPanelLabel.Text = emailFeedback;
                ResetRegisterPanelTextBoxes("Email");
                isErrorFree = false;
            }
            return isErrorFree;
        }

        /// <summary>
        /// Resets a particular input box in Change Password Panel according to the string passed in.
        /// </summary>
        /// <param name="p">Either: "Password" or "Email"  or "New Password"</param>
        private void ResetChangePasswordPanelTextBoxes(string p)
        {
            if (p.Equals("Password"))
            {
                passwordChangePasswordPanelTextBox.Text = "";
            }
            else if (p.Equals("Email"))
            {
                emailChangePasswordPanelTextBox.Text = "";
            }
            else if (p.Equals("New Password"))
            {
                newpasswordChangePasswordPanelTextBox.Text = "";
                confirmNewpasswordChangePasswordPanelTextBox.Text = "";
            }

        }     
       
        #endregion

        #region Forget Password Panel

        #region cancelForgetPasswordPanelButton
        private void cancelForgetPasswordPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelForgetPasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }

        private void cancelForgetPasswordPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelForgetPasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }

        private void cancelForgetPasswordPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelForgetPasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }

        private void cancelForgetPasswordPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelForgetPasswordPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }
        #endregion

        #region sendButton ( Forget Password )
        private void sendButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();
            // Gets feedback from helper function regarding the validity of the email entered by user in the Forget Password Panel.
            string emailFeedback = IsValidEmail(emailForgetPasswordPanelTextBox.Text);

            // Email is valid.
            if (emailFeedback.Equals("Email Perfect"))
            {
                int errorCode = 0;
                try
                {
                    // Attempts to send password to user's email account and gets appropriate feedback error code from controller.
                    errorCode = controller.RetrievePassword(emailForgetPasswordPanelTextBox.Text);
                }
                catch (ConnectionFailureException)
                {
                    MessageBox.Show("Unable to connect to Web Server");
                    return;
                }
                // Display the appropriate messages according to errorCode.
                switch (errorCode)
                {
                    case 1:
                        {
                            InitiateStartPanel();
                            SetErrorLabel("Password sent to email.", System.Drawing.Color.DeepSkyBlue);
                            break;
                        }// Success
                    case 2:
                        {
                            emailInvalidForgetPasswordPanelLabel.Text = "Unactivated Account.";
                            break;
                        }  // Invalid user
                    case 3:
                        {
                            emailInvalidForgetPasswordPanelLabel.Text = "Invalid Email.";
                            break;
                        }  // Invalid user
                    default: break;
                }
                ResetAllForgetPasswordPanelTextBoxes();
            }
            // Email is not valid.
            else
            {
                emailInvalidForgetPasswordPanelLabel.Text = emailFeedback;
                emailForgetPasswordPanelTextBox.Text = "";
            }
        }
        private void sendButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(sendButton, "GameAnywhere.Resources.sendButtonMouseDown.gif", ImageLayout.Zoom);
        }
        private void sendButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(sendButton, "GameAnywhere.Resources.sendButtonMouseOver.gif", ImageLayout.Zoom);
        }
        private void sendButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(sendButton, "GameAnywhere.Resources.sendButton.gif", ImageLayout.Zoom);
        }
        private void sendButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(sendButton, "GameAnywhere.Resources.sendButton.gif", ImageLayout.Zoom);
        }
        #endregion

        #endregion

        #region Resend Activation Panel
        #region cancelResendActivationPanelButton
        private void cancelResendActivationPanelButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelResendActivationPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseOver.gif", ImageLayout.Center);
        }

        private void cancelResendActivationPanelButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(cancelResendActivationPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }

        private void cancelResendActivationPanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelResendActivationPanelButton, "GameAnywhere.Resources.cancelLoginPanelButtonMouseDown.gif", ImageLayout.Center);
        }

        private void cancelResendActivationPanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(cancelResendActivationPanelButton, "GameAnywhere.Resources.cancelLoginPanelButton.gif", ImageLayout.Center);
        }
        #endregion
        #region resendButton ( Resend Activation )
        private void resendButton_MouseClick(object sender, MouseEventArgs e)
        {
            ResetErrorLabels();
            if (CheckEmailErrorsResendActivationPanel() && CheckPasswordErrorsResendActivationPanel())
            {
                int errorCode = 0;
                try
                {
                    errorCode = controller.ResendActivation(emailResendActivationPanelTextBox.Text, passwordResendActivationPanelTextBox.Text);
                }
                catch (ConnectionFailureException)
                {
                    MessageBox.Show("Unable to connect to Web Server");
                    return;
                }

                switch (errorCode)
                {
                    case 1:
                        {
                            InitiateStartPanel();
                            SetErrorLabel("Activation email resent.", System.Drawing.Color.DeepSkyBlue);
                            break;
                        } // Activation resent.
                    case 2:
                        {
                            ResetAllResendActivationPanelTextBoxes();
                            userDetailsInvalidResendActivationPanelLabel.Text = "Account has already been activated.";
                            break;
                        }// Account already activated
                    case 0:
                    case 3:
                        {
                            ResetAllResendActivationPanelTextBoxes();
                            userDetailsInvalidResendActivationPanelLabel.Text = "Invalid user details.";
                            break;
                        }// Invalid user details
                    default: break;
                }

            }

        }
        private void resendButton_MouseDown(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(resendButton, "GameAnywhere.Resources.resendButtonMouseDown.gif", ImageLayout.Zoom);
        }

        private void resendButton_MouseEnter(object sender, EventArgs e)
        {
            SetBackgroundImage(resendButton, "GameAnywhere.Resources.resendButtonMouseOver.gif", ImageLayout.Zoom);
        }

        private void resendButton_MouseLeave(object sender, EventArgs e)
        {
            SetBackgroundImage(resendButton, "GameAnywhere.Resources.resendButton.gif", ImageLayout.Zoom);
        }

        private void resendButton_MouseUp(object sender, MouseEventArgs e)
        {
            SetBackgroundImage(resendButton, "GameAnywhere.Resources.resendButton.gif", ImageLayout.Zoom);
        }
        #endregion


        /// <summary>
        /// Checks password input fields for the Resend Activation Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if password is valid. False if password is invalid.</returns>
        private bool CheckPasswordErrorsResendActivationPanel()
        {
            bool isErrorFree = true;
            string passwordFeedback = IsValidPassword(passwordResendActivationPanelTextBox.Text);
            if (!passwordFeedback.Equals("Password Perfect"))
            {
                userDetailsInvalidResendActivationPanelLabel.Text = passwordFeedback;
                passwordResendActivationPanelTextBox.Text = "";
                isErrorFree = false;
            }

            return isErrorFree;
        }

        /// <summary>
        /// Checks email input fields for the Resend Activation Panel, and edit the appropriate error labels to display errors.
        /// </summary>
        /// <returns>True if email is valid. False if email is invalid.</returns>
        private bool CheckEmailErrorsResendActivationPanel()
        {
            bool isErrorFree = true;
            string emailFeedback = IsValidEmail(emailResendActivationPanelTextBox.Text);
            if (!emailFeedback.Equals("Email Perfect"))
            {
                userDetailsInvalidResendActivationPanelLabel.Text = emailFeedback;
                emailResendActivationPanelTextBox.Text = "";
                isErrorFree = false;
            }
            return isErrorFree;
        }

        /// <summary>
        /// Resets a particular input box in Resend Activation Panel according to the string passed in.
        /// </summary>
        /// <param name="p">Either: "Password" or "Email"</param>
        private void ResetResendActivationPanelTextBoxes(string p)
        {
            if (p.Equals("Password"))
            {
                passwordResendActivationPanelTextBox.Text = "";
            }
            else if (p.Equals("Email"))
            {
                emailResendActivationPanelTextBox.Text = "";
            }
        }

        #endregion

        #region Helper Functions

        // This portion contains functions to initiate each of the panels.
        #region Panel Initializations
        private void InitiateStartPanel()
        {
            this.SuspendLayout();
            ResetErrorLabels();
            SetVisibilityAndUsability(startPanel, true, true);
            DisableOtherPanels(startPanel);
            SetVisibilityAndUsability(loginButton, true, true);
            SetVisibilityAndUsability(registerButton, true, true);
            SetVisibilityAndUsability(titlePanel, false, false);

            this.PerformLayout();
            this.ResumeLayout();
        }
        private void InitiateRegisterPanel()
        {
            this.SuspendLayout();

            ResetErrorLabels();
            ResetAllRegisterPanelTextboxes();
            SetVisibilityAndUsability(registerPanel, true, true);
            DisableOtherPanels(registerPanel);
            SetVisibilityAndUsability(loginButton, true, true);
            SetVisibilityAndUsability(registerButton, false, false);
            SetVisibilityAndUsability(titlePanel, true, true);
            SetBackgroundImage(titlePanel, "GameAnywhere.Resources.bannerRegisterPanel.gif", ImageLayout.Zoom);

            this.PerformLayout();
            this.ResumeLayout();
        }
        private void InitiateLoginPanel()
        {
            this.SuspendLayout();
            ResetErrorLabels();
            ResetAllLoginPanelTextBoxes();

            SetVisibilityAndUsability(loginPanel, true, true);
            DisableOtherPanels(loginPanel);
            SetVisibilityAndUsability(loginButton, false, false);
            SetVisibilityAndUsability(registerButton, true, true);
            SetVisibilityAndUsability(titlePanel, true, true);
            SetBackgroundImage(titlePanel, "GameAnywhere.Resources.bannerLoginPanel.gif", ImageLayout.Zoom);

            this.PerformLayout();
            this.ResumeLayout();

        }
        private void InitiateForgetPasswordPanel()
        {
            this.SuspendLayout();
            ResetErrorLabels();
            ResetAllForgetPasswordPanelTextBoxes();

            SetVisibilityAndUsability(forgetPasswordPanel, true, true);
            DisableOtherPanels(forgetPasswordPanel);
            EnableMainControlButtons();
            SetVisibilityAndUsability(titlePanel, true, true);
            SetBackgroundImage(titlePanel, "GameAnywhere.Resources.bannerForgetPasswordPanel.gif", ImageLayout.Zoom);

            this.PerformLayout();
            this.ResumeLayout();
        }
        private void InitiateResendActivationPanel()
        {
            this.SuspendLayout();
            ResetErrorLabels();
            ResetAllResendActivationPanelTextBoxes();

            SetVisibilityAndUsability(resendActivationPanel, true, true);
            DisableOtherPanels(resendActivationPanel);
            EnableMainControlButtons();
            SetVisibilityAndUsability(titlePanel, true, true);
            SetBackgroundImage(titlePanel, "GameAnywhere.Resources.bannerResendActivationPanel.gif", ImageLayout.Zoom);

            this.PerformLayout();
            this.ResumeLayout();
        }
        private void InitiateChangePasswordPanel()
        {
            this.SuspendLayout();
            ResetErrorLabels();
            ResetAllChangePasswordPanelTextBoxes();

            SetVisibilityAndUsability(changePasswordPanel, true, true);
            DisableOtherPanels(changePasswordPanel);
            EnableMainControlButtons();

            SetVisibilityAndUsability(titlePanel, true, true);

            SetBackgroundImage(titlePanel, "GameAnywhere.Resources.bannerChangePasswordPanel.gif", ImageLayout.Zoom);

            this.PerformLayout();
            this.ResumeLayout();
        }
        #endregion 

        // This portion contains functions to reset the individual panel's input fields.
        #region Reset Input TextBoxes 
        private void ResetAllLoginPanelTextBoxes()
        {
            ResetLoginPanelTextBoxes("Email");
            ResetLoginPanelTextBoxes("Password");
        }
        private void ResetAllForgetPasswordPanelTextBoxes()
        {
            emailForgetPasswordPanelTextBox.Text = "";
        }
        private void ResetAllResendActivationPanelTextBoxes()
        {
            ResetResendActivationPanelTextBoxes("Email");
            ResetResendActivationPanelTextBoxes("Password");
        }
        private void ResetAllRegisterPanelTextboxes()
        {
            ResetRegisterPanelTextBoxes("Email");
            ResetRegisterPanelTextBoxes("Password");
        }
        private void ResetAllChangePasswordPanelTextBoxes()
        {
            ResetChangePasswordPanelTextBoxes("Email");
            ResetChangePasswordPanelTextBoxes("Password");
            ResetChangePasswordPanelTextBoxes("New Password");
        }
        #endregion 

        // This portion contains functions for checking validity of passwords and emails.
        #region Validation

        /// <summary>
        /// Checks validity of passwords.
        /// </summary>
        /// <param name="password">Password string to be checked.</param>
        /// <returns>Feedback message for validity of the password passed in.</returns>
        private string IsValidPassword(string password)
        {
            if (password.Equals(""))
                return "Password field cannot be empty.";
            if (password.Length < 6)
                return "Password length must be 6 or more characters";

            return "Password Perfect";

        }

        /// <summary>
        /// Checks validity of emails.
        /// </summary>
        /// <param name="password">Email string to be checked.</param>
        /// <returns>Feedback message for validity of the password passed in</returns>
        public string IsValidEmail(string email)
        {
            if (email == null || email.Equals(""))
                return "Email field cannot be empty. ";

            // address is ok regarding the single @ sign
            if (!Regex.IsMatch(email, @"(\w+)@(\w+)\.(\w+)"))
                return "Invalid Email. ";

            return "Email Perfect";

        }
        #endregion

        // Enables the main control buttons.
        private void EnableMainControlButtons()
        {
            SetVisibilityAndUsability(loginButton, true, true);
            SetVisibilityAndUsability(registerButton, true, true);
        }

        // Sets the front page error label's text to the desired string and color.
        public void SetErrorLabel(string s, System.Drawing.Color c)
        {
            errorLabel.Text = s;
            errorLabel.ForeColor = c;
        }

        // Function disables all other panels other than the one passed in.
        private void DisableOtherPanels(Panel panelDisplayed)
        {
            foreach (Panel p in panelList)
            {
                if (!p.Name.Equals(panelDisplayed.Name))
                {
                    SetVisibilityAndUsability(p, false, false);
                }
            }
        }

        // Resets all error labels in this form. ( 9 labels )
        private void ResetErrorLabels()
        {
            // 9 Labels.
            passwordMismatchLabel.Text = "";
            passwordInvalidLabel.Text = "";
            emailInvalidLabel.Text = "";
            loginFailedLabel.Text = "";
            errorLabel.Text = "";

            emailInvalidForgetPasswordPanelLabel.Text = "";
            userDetailsInvalidChangePasswordPanelLabel.Text = "";
            newpasswordMismatchChangePasswordPanelLabel.Text = "";
            userDetailsInvalidResendActivationPanelLabel.Text = "";
        }

        /// <summary>
        /// Sets background image by retrieving the image file from the assembly.
        /// </summary>
        /// <param name="c">The control for setting the background image in.</param>
        /// <param name="resourcePath">The path to the image file.</param>
        /// <param name="imageLayout">The desired layout to set the background image.</param>
        public void SetBackgroundImage(System.Windows.Forms.Control c, string resourcePath, ImageLayout imageLayout)
        {
            System.IO.Stream imageStream = this.GetType().Assembly.GetManifestResourceStream(resourcePath);
            c.BackgroundImage = Image.FromStream(imageStream);
            c.BackgroundImageLayout = imageLayout;
            imageStream.Close();
        }

        // Sets control to be visible or enabled and vice versa.
        private void SetVisibilityAndUsability(System.Windows.Forms.Control c, bool makeVisible, bool makeEnabled)
        {
            c.Visible = makeVisible;
            c.Enabled = makeEnabled;
        }

        #endregion

        #region Form Closing Handlers (DO NOT TOUCH)
        private void startPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Form is closing through other means.
            if (!UserClosing)
            {
                restoreWarning formRestoreWarning = new restoreWarning(this, controller);
                // Dialog has not been displayed
                if (ChildActive == false)
                {
                    // There is a backup
                    if (controller.EndProgram())
                    {
                        // Display Dialog
                        DisplayWarningDialog(formRestoreWarning);
                    }

                    // User has chosen to come back to start page instead. 
                    // Hence, cancel the form closing status.
                    if (formRestoreWarning.cancelClicked)
                        e.Cancel = true;
                    // if cancelClicked is false -> user has either chosen Yes/No in the next screen.
                    // Startpage will then close.
                }
            }


            // When user exits through our custom-exit button, do nothing and let it close.

        }

        // To display a new warning dialog.
        private void DisplayWarningDialog(restoreWarning formRestoreWarning)
        {
            ChildActive = true;
            formRestoreWarning.ShowDialog();
            ChildActive = false;
        }
        #endregion

        

    }
}