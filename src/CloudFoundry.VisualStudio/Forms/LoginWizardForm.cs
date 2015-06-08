namespace CloudFoundry.VisualStudio.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;
    using CloudFoundry.CloudController.Common.Exceptions;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using CloudFoundry.UAA.Exceptions;
    using CloudFoundry.VisualStudio.Controls;
    using CloudFoundry.VisualStudio.TargetStore;

    public partial class LoginWizardForm : Form
    {
        private CloudTarget[] cloudTargets = new CloudTarget[0];
        private CloudTarget reloginTarget;
        private string refreshToken = string.Empty;
        private string targetUrl = string.Empty;
        private string version = string.Empty;

        private LoginUserControl loginControl = null;
        private Label summaryInfoLabel;
        private CloudFoundryClient client;

        public LoginWizardForm()
        {
            this.InitializeComponent();
        }

        private enum MessageType
        {
            Info,
            Error
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions for detailed logging purposes.")]
        public CloudTarget CloudTarget
        {
            get
            {
                return CloudTarget.CreateV2Target(
                            new Uri(this.targetUrl),
                            string.Empty,
                            this.loginControl.Email,
                            this.loginControl.IgnoreSSLErrors,
                            this.version);
            }
        }

        public string Password
        {
            get { return this.loginControl.Password; }
        }

        public void SetLoginUrl(Uri newLoginUrl)
        {
            if (this.loginControl != null)
            {
                this.loginControl.TargetUrl = newLoginUrl.ToString();
            }
        }

        public void SetReloginTarget(CloudTarget cloudTarget)
        {
            this.reloginTarget = cloudTarget;
        }

        private void LoginWizardForm_Load(object sender, EventArgs e)
        {
            this.LoginTargetLinkLabel_Click(sender, e);
        }

        private void LoginTargetLinkLabel_Click(object sender, EventArgs e)
        {
            this.summaryLinkLabel.BackColor = SystemColors.Control;
            this.summaryLinkLabel.LinkColor = SystemColors.ControlDarkDark;

            this.loginTargetLinkLabel.BackColor = SystemColors.Highlight;
            this.loginTargetLinkLabel.LinkColor = SystemColors.ControlLightLight;

            this.titleLabel.Text = "Target URL and credentials";

            this.previousButton.Enabled = false;
            this.nextButton.Text = "&Next >";
            this.nextButton.Enabled = true;
            this.cancelButton.Text = "Cancel";

            this.pageSplitContainer.Panel1.Controls.Remove(this.summaryInfoLabel);

            if (this.loginControl == null)
            {
                this.loginControl = new LoginUserControl();
            }

            if (this.reloginTarget != null)
            {
                this.loginControl.TargetUrl = this.reloginTarget.TargetUrl.ToString();
                this.loginControl.EnableTargetUrlTextBox(false);
            }

            this.pageSplitContainer.Panel1.Controls.Add((Control)this.loginControl);

            this.loginControl.HideLoginErrorLabel();
        }

        private void SummaryLinkLabel_Click(object sender, EventArgs e)
        {
            this.loginTargetLinkLabel.BackColor = SystemColors.Control;
            this.loginTargetLinkLabel.LinkColor = SystemColors.ControlDarkDark;

            this.summaryLinkLabel.BackColor = SystemColors.Highlight;
            this.summaryLinkLabel.LinkColor = SystemColors.ControlLightLight;

            this.titleLabel.Text = "Spaces Summary";

            this.EnableButtons();
            this.nextButton.Text = "Finish";
            this.cancelButton.Text = "Cancel";

            this.pageSplitContainer.Panel1.Controls.Remove((Control)this.loginControl);
            this.labelError.Visible = false;

            if (this.summaryInfoLabel == null)
            {
                this.summaryInfoLabel = new Label();
                this.summaryInfoLabel.AutoSize = true;
                this.summaryInfoLabel.Location = new System.Drawing.Point(3, 3);
                this.summaryInfoLabel.Name = "summaryInfoLabel";
                this.summaryInfoLabel.Size = new System.Drawing.Size(39, 17);
                this.summaryInfoLabel.TabIndex = 0;
                this.summaryInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, "You're done!{0}Press Finish to close the wizard and save your target.", Environment.NewLine);
            }

            this.pageSplitContainer.Panel1.Controls.Add(this.summaryInfoLabel);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (this.loginTargetLinkLabel.Enabled)
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            DisableButtons();
                            CleanErrorControls();
                        }));
                    }
                    else
                    {
                        DisableButtons();
                        CleanErrorControls();
                    }

                    string loginEx = string.Empty;

                    try
                    {
                        Logger.Info("Starting login process...");
                        targetUrl = this.loginControl.TargetUrl;
                        CancellationTokenSource cts = new CancellationTokenSource();
                        client = new CloudFoundryClient(new Uri(this.loginControl.TargetUrl), cts.Token, null, loginControl.IgnoreSSLErrors);

                        CloudCredentials creds = new CloudCredentials();
                        creds.User = this.loginControl.Email;
                        creds.Password = this.loginControl.Password;
                        this.refreshToken = client.Login(creds).Result.Token.RefreshToken;
                        this.version = client.Info.GetInfo().Result.ApiVersion;

                        if (reloginTarget != null)
                        {
                            Logger.Info(string.Format(CultureInfo.InvariantCulture, "Detected token refresh request for '{0}', target '{1}'", reloginTarget.Email, reloginTarget.TargetUrl));
                        }

                        Logger.Info("User completed login process.");

                        if (this.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(() =>
                            {
                                SetSummarySpacesControls(sender, e);
                            }));
                        }
                        else
                        {
                            SetSummarySpacesControls(sender, e);
                        }
                    }
                    catch (Exception ex)
                    {
                        List<string> messages = new List<string>();
                        FormatExceptionMessage(ex, messages);

                        loginEx = string.Join(Environment.NewLine, messages);

                        Logger.Error(loginEx, ex);

                        SetControlsForLoginError(loginEx);
                    }
                });
            }
            else if (this.nextButton.Text == "Finish")
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (this.summaryLinkLabel.Enabled)
            {
                this.LoginTargetLinkLabel_Click(sender, e);
                this.loginTargetLinkLabel.Enabled = true;
                this.summaryLinkLabel.Enabled = false;
                this.labelError.Visible = false;
            }
        }

        private void SetControlsForLoginError(string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    SetErrorLabel(error);
                    SetControlsForLoginError();
                }));
            }
            else
            {
                this.SetErrorLabel(error);
                this.SetControlsForLoginError();
            }
        }

        private void SetControlsForLoginError()
        {
            this.loginControl.Enabled = true;
            this.previousButton.Enabled = false;
            this.nextButton.Enabled = true;
            this.cancelButton.Enabled = true;
        }

        private void SetErrorLabel(string error)
        {
            this.labelError.Visible = true;
            this.labelError.ForeColor = Color.Red;
            this.labelError.Text = error;
        }

        private void DisableButtons()
        {
            this.loginControl.Enabled = false;
            this.previousButton.Enabled = false;
            this.nextButton.Enabled = false;
            this.cancelButton.Enabled = false;
        }

        private void EnableButtons()
        {
            this.loginControl.Enabled = true;
            this.previousButton.Enabled = true;
            this.nextButton.Enabled = true;
            this.cancelButton.Enabled = true;
        }

        private void CleanErrorControls()
        {
            this.loginControl.HideLoginErrorLabel();
            this.pageSplitContainer.Panel1.Controls.Add(this.labelError);
            this.labelError.Visible = false;
        }

        private void SetSummarySpacesControls(object sender, EventArgs e)
        {
            this.summaryLinkLabel.Enabled = true;
            this.loginTargetLinkLabel.Enabled = false;
            this.SummaryLinkLabel_Click(sender, e);
            this.nextButton.Focus();
        }

        private static void FormatExceptionMessage(Exception ex, List<string> message)
        {
            if (ex is AggregateException)
            {
                foreach (Exception iex in (ex as AggregateException).Flatten().InnerExceptions)
                {
                    FormatExceptionMessage(iex, message);
                }
            }
            else
            {
                message.Add(ex.Message);
                Logger.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    FormatExceptionMessage(ex.InnerException, message);
                }
            }
        }
    }
}