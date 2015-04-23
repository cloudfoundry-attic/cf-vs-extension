namespace CloudFoundry.VisualStudio.Controls
{
    partial class LoginUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxTarget = new System.Windows.Forms.TextBox();
            this.labelEmail = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxEmail = new System.Windows.Forms.TextBox();
            this.labelTargetUrl = new System.Windows.Forms.Label();
            this.labelLoginError = new System.Windows.Forms.Label();
            this.checkBoxIgnoreSSLErrors = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBoxTarget
            // 
            this.textBoxTarget.Location = new System.Drawing.Point(29, 36);
            this.textBoxTarget.Name = "textBoxTarget";
            this.textBoxTarget.Size = new System.Drawing.Size(256, 20);
            this.textBoxTarget.TabIndex = 0;
            this.textBoxTarget.Leave += new System.EventHandler(this.textBoxTarget_Leave);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Location = new System.Drawing.Point(28, 96);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(32, 13);
            this.labelEmail.TabIndex = 47;
            this.labelEmail.Text = "Email";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(29, 164);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(256, 20);
            this.textBoxPassword.TabIndex = 2;
            this.textBoxPassword.Enter += new System.EventHandler(this.textBoxPassword_Enter);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(26, 144);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(53, 13);
            this.labelPassword.TabIndex = 45;
            this.labelPassword.Text = "Password";
            // 
            // textBoxEmail
            // 
            this.textBoxEmail.Location = new System.Drawing.Point(29, 116);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new System.Drawing.Size(256, 20);
            this.textBoxEmail.TabIndex = 1;
            this.textBoxEmail.Enter += new System.EventHandler(this.textBoxEmail_Enter);
            // 
            // labelTargetUrl
            // 
            this.labelTargetUrl.AutoSize = true;
            this.labelTargetUrl.Location = new System.Drawing.Point(26, 20);
            this.labelTargetUrl.Name = "labelTargetUrl";
            this.labelTargetUrl.Size = new System.Drawing.Size(222, 13);
            this.labelTargetUrl.TabIndex = 42;
            this.labelTargetUrl.Text = "Enter target URL (ex. http://api.hpcloud.com)";
            // 
            // labelLoginError
            // 
            this.labelLoginError.AutoSize = true;
            this.labelLoginError.ForeColor = System.Drawing.Color.Red;
            this.labelLoginError.Location = new System.Drawing.Point(291, 170);
            this.labelLoginError.Name = "labelLoginError";
            this.labelLoginError.Size = new System.Drawing.Size(11, 13);
            this.labelLoginError.TabIndex = 49;
            this.labelLoginError.Text = "*";
            this.labelLoginError.Visible = false;
            // 
            // checkBoxIgnoreSSLErrors
            // 
            this.checkBoxIgnoreSSLErrors.AutoSize = true;
            this.checkBoxIgnoreSSLErrors.Location = new System.Drawing.Point(29, 190);
            this.checkBoxIgnoreSSLErrors.Name = "checkBoxIgnoreSSLErrors";
            this.checkBoxIgnoreSSLErrors.Size = new System.Drawing.Size(109, 17);
            this.checkBoxIgnoreSSLErrors.TabIndex = 50;
            this.checkBoxIgnoreSSLErrors.Text = "Ignore SSL Errors";
            this.checkBoxIgnoreSSLErrors.UseVisualStyleBackColor = true;
            // 
            // LoginUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxIgnoreSSLErrors);
            this.Controls.Add(this.labelLoginError);
            this.Controls.Add(this.textBoxTarget);
            this.Controls.Add(this.labelEmail);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxEmail);
            this.Controls.Add(this.labelTargetUrl);
            this.Name = "LoginUserControl";
            this.Size = new System.Drawing.Size(489, 241);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTarget;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.Label labelTargetUrl;
        private System.Windows.Forms.Label labelLoginError;
        private System.Windows.Forms.CheckBox checkBoxIgnoreSSLErrors;

    }
}
