namespace CloudFoundry.VisualStudio.Forms
{
    partial class LoginWizardForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginWizardForm));
            this.OrientationFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.loginTargetLinkLabel = new System.Windows.Forms.LinkLabel();
            this.summaryLinkLabel = new System.Windows.Forms.LinkLabel();
            this.bannerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pageSplitContainer = new System.Windows.Forms.SplitContainer();
            this.labelError = new System.Windows.Forms.Label();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.OrientationFlowLayoutPanel.SuspendLayout();
            this.bannerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pageSplitContainer)).BeginInit();
            this.pageSplitContainer.Panel1.SuspendLayout();
            this.pageSplitContainer.Panel2.SuspendLayout();
            this.pageSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // OrientationFlowLayoutPanel
            // 
            this.OrientationFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OrientationFlowLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
            this.OrientationFlowLayoutPanel.Controls.Add(this.loginTargetLinkLabel);
            this.OrientationFlowLayoutPanel.Controls.Add(this.summaryLinkLabel);
            this.OrientationFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.OrientationFlowLayoutPanel.Location = new System.Drawing.Point(3, 41);
            this.OrientationFlowLayoutPanel.Name = "OrientationFlowLayoutPanel";
            this.OrientationFlowLayoutPanel.Size = new System.Drawing.Size(160, 368);
            this.OrientationFlowLayoutPanel.TabIndex = 1;
            // 
            // loginTargetLinkLabel
            // 
            this.loginTargetLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.ControlText;
            this.loginTargetLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.loginTargetLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 20);
            this.loginTargetLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.loginTargetLinkLabel.LinkColor = System.Drawing.SystemColors.ControlDarkDark;
            this.loginTargetLinkLabel.Location = new System.Drawing.Point(0, 8);
            this.loginTargetLinkLabel.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.loginTargetLinkLabel.Name = "loginTargetLinkLabel";
            this.loginTargetLinkLabel.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.loginTargetLinkLabel.Size = new System.Drawing.Size(160, 18);
            this.loginTargetLinkLabel.TabIndex = 0;
            this.loginTargetLinkLabel.TabStop = true;
            this.loginTargetLinkLabel.Text = "Target Login";
            this.loginTargetLinkLabel.UseCompatibleTextRendering = true;
            this.loginTargetLinkLabel.VisitedLinkColor = System.Drawing.SystemColors.ControlText;
            this.loginTargetLinkLabel.Click += new System.EventHandler(this.loginTargetLinkLabel_Click);
            // 
            // summaryLinkLabel
            // 
            this.summaryLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.ControlText;
            this.summaryLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.summaryLinkLabel.Enabled = false;
            this.summaryLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 160);
            this.summaryLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.summaryLinkLabel.LinkColor = System.Drawing.SystemColors.ControlDarkDark;
            this.summaryLinkLabel.Location = new System.Drawing.Point(0, 30);
            this.summaryLinkLabel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.summaryLinkLabel.Name = "summaryLinkLabel";
            this.summaryLinkLabel.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.summaryLinkLabel.Size = new System.Drawing.Size(160, 18);
            this.summaryLinkLabel.TabIndex = 1;
            this.summaryLinkLabel.TabStop = true;
            this.summaryLinkLabel.Text = "Summary";
            this.summaryLinkLabel.UseCompatibleTextRendering = true;
            this.summaryLinkLabel.VisitedLinkColor = System.Drawing.SystemColors.ControlText;
            this.summaryLinkLabel.Click += new System.EventHandler(this.summaryLinkLabel_Click);
            // 
            // bannerPanel
            // 
            this.bannerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bannerPanel.BackColor = System.Drawing.Color.White;
            this.bannerPanel.Controls.Add(this.titleLabel);
            this.bannerPanel.Controls.Add(this.pictureBox1);
            this.bannerPanel.Location = new System.Drawing.Point(1, 1);
            this.bannerPanel.Name = "bannerPanel";
            this.bannerPanel.Size = new System.Drawing.Size(657, 34);
            this.bannerPanel.TabIndex = 2;
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(169, 8);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(51, 13);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Page title";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::CloudFoundry.VisualStudio.Resources.LogoBanner;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pageSplitContainer
            // 
            this.pageSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pageSplitContainer.IsSplitterFixed = true;
            this.pageSplitContainer.Location = new System.Drawing.Point(167, 41);
            this.pageSplitContainer.Name = "pageSplitContainer";
            this.pageSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // pageSplitContainer.Panel1
            // 
            this.pageSplitContainer.Panel1.AutoScroll = true;
            this.pageSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.pageSplitContainer.Panel1.Controls.Add(this.labelError);
            // 
            // pageSplitContainer.Panel2
            // 
            this.pageSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.pageSplitContainer.Panel2.Controls.Add(this.previousButton);
            this.pageSplitContainer.Panel2.Controls.Add(this.nextButton);
            this.pageSplitContainer.Panel2.Controls.Add(this.cancelButton);
            this.pageSplitContainer.Size = new System.Drawing.Size(489, 368);
            this.pageSplitContainer.SplitterDistance = 327;
            this.pageSplitContainer.TabIndex = 3;
            // 
            // labelError
            // 
            this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelError.AutoSize = true;
            this.labelError.Location = new System.Drawing.Point(29, 240);
            this.labelError.MaximumSize = new System.Drawing.Size(460, 0);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(0, 13);
            this.labelError.TabIndex = 52;
            this.labelError.Visible = false;
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(220, 6);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 1;
            this.previousButton.TabStop = false;
            this.previousButton.Text = "< &Previous";
            this.previousButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.previousButton.UseVisualStyleBackColor = false;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(299, 6);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 0;
            this.nextButton.Text = "&Next >";
            this.nextButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(405, 6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // LoginWizardForm
            // 
            this.AcceptButton = this.nextButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(659, 411);
            this.Controls.Add(this.pageSplitContainer);
            this.Controls.Add(this.bannerPanel);
            this.Controls.Add(this.OrientationFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login Wizard";
            this.Load += new System.EventHandler(this.LoginWizardForm_Load);
            this.OrientationFlowLayoutPanel.ResumeLayout(false);
            this.bannerPanel.ResumeLayout(false);
            this.bannerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pageSplitContainer.Panel1.ResumeLayout(false);
            this.pageSplitContainer.Panel1.PerformLayout();
            this.pageSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pageSplitContainer)).EndInit();
            this.pageSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel OrientationFlowLayoutPanel;
        private System.Windows.Forms.LinkLabel loginTargetLinkLabel;
        private System.Windows.Forms.LinkLabel summaryLinkLabel;
        private System.Windows.Forms.Panel bannerPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.SplitContainer pageSplitContainer;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Label labelError;
    }
}