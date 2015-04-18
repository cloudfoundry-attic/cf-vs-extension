using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HP.CloudFoundry.UI.VisualStudio.Controls
{
    public partial class LoginUserControl : UserControl
    {
        public string Email
        {
            get
            {
                return this.textBoxEmail.Text;
            }
        }

        public string Password
        {
            get
            {
                return this.textBoxPassword.Text;
            }
        }

        public string TargetUrl
        {
            get
            {
                return this.textBoxTarget.Text;
            }
            set
            {
                this.textBoxTarget.Text = value;
            }
        }

        public bool IgnoreSSLErrors
        {
            get
            {
                return this.checkBoxIgnoreSSLErrors.Checked;
            }
        }

        public void ShowLoginError(string message)
        {
            this.labelLoginError.Visible = true;
            this.labelLoginError.ForeColor = Color.Red;
            this.labelLoginError.Text = string.Format("* {0}", message);
        }

        public void HideLoginErrorLabel()
        {
            this.labelLoginError.Visible = false;
        }

        public void EnableTargetUrlTextBox(bool value)
        {
            this.textBoxTarget.Enabled = value;
        }

        public LoginUserControl()
        {
            InitializeComponent();
        }

        private void textBoxTarget_Leave(object sender, EventArgs e)
        {
            if (!this.textBoxTarget.Text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !this.textBoxTarget.Text.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                this.textBoxTarget.Text = "http://" + this.textBoxTarget.Text;
            }
        }

        private void textBoxEmail_Enter(object sender, EventArgs e)
        {
            textBoxEmail.SelectionStart = 0;
            textBoxEmail.SelectionLength = textBoxEmail.Text.Length;
        }

        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            textBoxPassword.SelectionStart = 0;
            textBoxPassword.SelectionLength = textBoxPassword.Text.Length;
        }       

    }
}
