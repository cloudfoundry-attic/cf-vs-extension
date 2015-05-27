namespace CloudFoundry.VisualStudio.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public partial class LoginUserControl : UserControl
    {
        public LoginUserControl()
        {
            this.InitializeComponent();
        }

        public string Email
        {
            get { return this.textBoxEmail.Text; }
        }

        public string Password
        {
            get { return this.textBoxPassword.Text; }
        }

        public string TargetUrl
        {
            get { return this.textBoxTarget.Text; }
            set { this.textBoxTarget.Text = value; }
        }

        public bool IgnoreSSLErrors
        {
            get { return this.checkBoxIgnoreSSLErrors.Checked; }
        }

        public void ShowLoginError(string message)
        {
            this.labelLoginError.Visible = true;
            this.labelLoginError.ForeColor = Color.Red;
            this.labelLoginError.Text = string.Format(CultureInfo.InvariantCulture, "* {0}", message);
        }

        public void HideLoginErrorLabel()
        {
            this.labelLoginError.Visible = false;
        }

        public void EnableTargetUrlTextBox(bool value)
        {
            this.textBoxTarget.Enabled = value;
        }

        private void TextBoxTarget_Leave(object sender, EventArgs e)
        {
            if (!this.textBoxTarget.Text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !this.textBoxTarget.Text.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                this.textBoxTarget.Text = "http://" + this.textBoxTarget.Text;
            }
        }

        private void TextBoxEmail_Enter(object sender, EventArgs e)
        {
            this.textBoxEmail.SelectionStart = 0;
            this.textBoxEmail.SelectionLength = this.textBoxEmail.Text.Length;
        }

        private void TextBoxPassword_Enter(object sender, EventArgs e)
        {
            this.textBoxPassword.SelectionStart = 0;
            this.textBoxPassword.SelectionLength = this.textBoxPassword.Text.Length;
        }       
    }
}
