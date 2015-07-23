namespace CloudFoundry.VisualStudio.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using CloudFoundry.VisualStudio.ProjectPush;
    using CloudFoundry.VisualStudio.TargetStore;
    using Microsoft.VisualStudio.PlatformUI;

    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LogOnForm : DialogWindow
    {
        private string version = string.Empty;
        private CloudTarget cloudTarget = null;

        public LogOnForm()
        {
            this.InitializeComponent();
            this.DataContext = new ErrorResource();
        }

        public CloudTarget CloudTarget
        {
            get
            {
                return this.cloudTarget;
            }
        }

        private async void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            this.busyIndicator.IsBusy = true;
            var targetUrl = this.tbUrl.Text;

            var errorResource = this.DataContext as ErrorResource;
            if (errorResource == null)
            {
                throw new InvalidOperationException("Invalid DataContext");
            }

            try
            {
                var client = new CloudFoundryClient(new Uri(targetUrl), new CancellationToken(), null, (bool)cbIgnoreSSK.IsChecked);

                CloudCredentials creds = new CloudCredentials();
                creds.User = this.tbUsername.Text;
                creds.Password = this.pbPassword.Password;

                var authContext = await client.Login(creds);
                var info = await client.Info.GetInfo();
                this.version = info.ApiVersion;

                this.cloudTarget = CloudTarget.CreateV2Target(
                            new Uri(this.tbUrl.Text),
                            this.tbDescription.Text,
                            this.tbUsername.Text,
                            (bool)this.cbIgnoreSSK.IsChecked,
                            this.version);

                CloudTargetManager.SaveTarget(this.cloudTarget);
                CloudCredentialsManager.Save(this.cloudTarget.TargetUrl, this.cloudTarget.Email, this.pbPassword.Password);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                var errorMessages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(ex, errorMessages);
                errorResource.ErrorMessage = string.Join(Environment.NewLine, errorMessages.ToArray());
                errorResource.HasErrors = true;
            }

            this.busyIndicator.IsBusy = false;
        }

        private void TbUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbUrl.Text))
            {
                return;
            }

            if (!this.tbUrl.Text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !this.tbUrl.Text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                this.tbUrl.Text = string.Format(CultureInfo.InvariantCulture, "https://{0}", this.tbUrl.Text);
            }
        }

        private void Wizard_Cancel(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBoxHelper.WarningQuestion("Do you really want to cancel ?");
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
