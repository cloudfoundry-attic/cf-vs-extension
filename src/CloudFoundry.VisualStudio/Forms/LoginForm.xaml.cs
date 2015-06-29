using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using CloudFoundry.VisualStudio.ProjectPush;
using CloudFoundry.VisualStudio.TargetStore;
using Microsoft.VisualStudio.PlatformUI;
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

namespace CloudFoundry.VisualStudio.Forms
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : DialogWindow
    {
        private string version = string.Empty;
        private CloudTarget cloudTarget = null;

        public LoginForm()
        {
            InitializeComponent();
            this.DataContext = new ErrorResource();
        }

        public CloudTarget CloudTarget
        {
            get
            {
                return cloudTarget;
            }
        }

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
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

                cloudTarget = CloudTarget.CreateV2Target(
                            new Uri(this.tbUrl.Text),
                            this.tbDescription.Text,
                            this.tbUsername.Text,
                            (bool)this.cbIgnoreSSK.IsChecked,
                            this.version);

                CloudTargetManager.SaveTarget(cloudTarget);
                CloudCredentialsManager.Save(cloudTarget.TargetUrl, cloudTarget.Email, this.pbPassword.Password);

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
            this.IsEnabled = true;

        }

        private void tbUrl_LostFocus(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(this.tbUrl.Text))
            {
                return;
            }

            if (!this.tbUrl.Text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) &&
                !this.tbUrl.Text.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                this.tbUrl.Text = string.Format(CultureInfo.InvariantCulture, "https://{0}", this.tbUrl.Text);
            }
        }
    }
}
