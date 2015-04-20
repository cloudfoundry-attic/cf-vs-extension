using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using HP.CloudFoundry.UI.VisualStudio.Forms;
using HP.CloudFoundry.UI.VisualStudio.ProjectPush;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HP.CloudFoundry.UI.VisualStudio
{
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : DialogWindow
    {
        public EditDialog(AppPackage package, bool AllowPublish = true)
        {
            InitializeComponent();
            this.DataContext = package;
            PasswordBox.Password = package.CFPassword;
            if (AllowPublish == false)
            {
                Publish.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SavePassword();

            this.DialogResult = false;
            this.Close();
        }

        private void SavePassword()
        {
            AppPackage package = this.DataContext as AppPackage;
            if (package != null)
            {
                package.CFPassword = PasswordBox.Password;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SavePassword();

            this.DialogResult = true;
            this.Close();
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            try
            {   
                CloudFoundryClient client = new CloudFoundryClient(new Uri(ServerUri.Text), new System.Threading.CancellationToken());
                CloudCredentials creds = new CloudCredentials();
                creds.User = User.Text;
                creds.Password = PasswordBox.Password;
                string reftoken = client.Login(creds).Result.Token.RefreshToken;

                MessageBoxHelper.DisplayInfo("Connection Successful!");
                SavePassword();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayError("Unable to verify settings!", ex);
            }
        }
    }
}
