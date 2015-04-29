using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using CloudFoundry.VisualStudio.Forms;
using CloudFoundry.VisualStudio.ProjectPush;
using CloudFoundry.VisualStudio.TargetStore;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication;
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

namespace CloudFoundry.VisualStudio
{
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : DialogWindow
    {
        public string PublishProfile { get; set; }
        private CloudFoundryClient client;
        private Project currentProj;
        public EditDialog(AppPackage package, Project currentProject)
        {
            InitializeComponent();

            this.currentProj = currentProject;

            this.PublishProfile = "push";
            this.DataContext = package;
            Init(package);
        }

        private async void Init(AppPackage package)
        {
            if (package.CFServerUri != string.Empty)
            {
                this.IsEnabled = false;
                await InitClient(package);
                await Load(package).ContinueWith((antecedent) =>
                {
                    if (antecedent.IsFaulted)
                    {
                        var errorMessages = new List<string>();
                        ErrorFormatter.FormatExceptionMessage(antecedent.Exception, errorMessages);
                        MessageBoxHelper.DisplayError(string.Join(Environment.NewLine, errorMessages));
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            SelectLoadedValues(package);
                        });
                    }
                });
                this.IsEnabled = true;
            }
            else
            {
                StatusIcon.Source = GetBitmapImageFromResources("warning");
                StatusIcon.ToolTip = "Please set a target";
                this.IsEnabled = true;
            }
        }

        public BitmapImage GetBitmapImageFromResources(string type)
        {
            System.Drawing.Bitmap bmp = null;

            switch (type)
            {
                case "refresh": { bmp = CloudFoundry.VisualStudio.Resources.StatusStarted; break; }
                case "ok": { bmp = CloudFoundry.VisualStudio.Resources.StatusRunning; break; }
                case "error": { bmp = CloudFoundry.VisualStudio.Resources.Error; break; }
                case "warning": { bmp = CloudFoundry.VisualStudio.Resources.SSLDisabled; break; }
                default: break;
            }

            return Converters.ImageConverter.ConvertBitmapToBitmapImage(bmp);
        }
        private void GetRoutes(AppPackage package)
        {
            //TODO: Generate controls for multiple routes
            string[] routesList = package.CFRoutes.Split(';');

            foreach (string route in routesList)
            {
                string cleanroute = route.Replace("http://", string.Empty).Replace("https://", string.Empty);
                string domain = cleanroute.Substring(route.IndexOf('.') + 1);
                string host = cleanroute.Split('.').First().ToLower(CultureInfo.InvariantCulture);

                HostName.Text = host;
                DomainsCombo.SelectedItem = DomainsCombo.Items.Cast<ListAllDomainsForSpaceDeprecatedResponse>().Where(o => o.Name == domain).FirstOrDefault();
            }
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            if (currentProj != null)
            {
                saveDialog.InitialDirectory = System.IO.Path.GetDirectoryName(currentProj.FullName);
            }
            saveDialog.Filter = string.Format(CultureInfo.InvariantCulture, "CF Publish file | *{0}", CloudFoundry_VisualStudioPackage.extension);
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveCurrentToFile(saveDialog.FileName);
                this.ProfilePath.Text = saveDialog.FileName;

            }


        }

        private void SaveCurrentToFile(string filepath)
        {
            SaveContext();
            AppPackage package = this.DataContext as AppPackage;
            if (package != null)
            {
                package.SaveToFile(filepath);
                if (currentProj != null)
                {
                    currentProj.ProjectItems.AddFromFile(filepath);
                }
            }
        }

        private void SaveContext()
        {
            AppPackage package = this.DataContext as AppPackage;
            if (package != null)
            {
                package.CFRoutes = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", HostName.Text, DomainsCombo.Text);
            }
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            SaveContext();
            AppPackage package = this.DataContext as AppPackage;
            if (package != null)
            {
                package.Save();
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            System.Windows.Forms.OpenFileDialog dialogOpen = new System.Windows.Forms.OpenFileDialog();
            if (currentProj != null)
            {
                dialogOpen.InitialDirectory = System.IO.Path.GetDirectoryName(currentProj.FullName);
            }
            dialogOpen.Filter = string.Format(CultureInfo.InvariantCulture, "CF Publish file | *{0}", CloudFoundry_VisualStudioPackage.extension);

            if (dialogOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.DataContext = null;
                AppPackage package = new AppPackage();
                try
                {
                    package.LoadFromFile(dialogOpen.FileName);
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.DisplayError(string.Format(CultureInfo.InvariantCulture, "Cannot load {0}. File is corrupt.", dialogOpen.FileName));
                    Logger.Error("Error loading profile file", ex);
                    this.IsEnabled = true;
                    return;
                }

                this.PublishProfile = System.IO.Path.GetFileName(dialogOpen.FileName).Replace(CloudFoundry_VisualStudioPackage.extension, "");
                if (package != null)
                {
                    Init(package);
                }
            }
            else
            {
                this.IsEnabled = true;
            }
        }

        //This populates the comboboxes with the values required by the package
        private async Task Load(AppPackage package)
        {
            var orgs = await client.Organizations.ListAllOrganizations().ConfigureAwait(false);
            var stacks = await client.Stacks.ListAllStacks().ConfigureAwait(false);

            Dispatcher.Invoke(() =>
            {
                this.ProfilePath.Text = package.ConfigFile;
                this.LocalBuild.IsChecked = package.CFLocalBuild;
                LocalBuild_Click(LocalBuild, null);
                if (package.CFLocalBuild)
                {
                    this.Platform.Text = package.CFMSBuildPlatform;
                    this.Configuration.Text = package.CFMSBuildConfiguration;
                }

                OrgCombo.DisplayMemberPath = "Name";
                OrgCombo.ItemsSource = orgs;
                OrgCombo.SelectedValuePath = "EntityMetadata.Guid";

                StacksCombo.DisplayMemberPath = "Name";
                StacksCombo.SelectedValuePath = "Name";
                StacksCombo.ItemsSource = stacks;
            });

            string selectedOrg = string.Empty;
            string selectedSpace = string.Empty;

            if (package.CFOrganization != string.Empty)
            {
                selectedOrg = orgs.Where(o => o.Name == package.CFOrganization).FirstOrDefault().EntityMetadata.Guid;

                var spaces = await client.Organizations.ListAllSpacesForOrganization(new Guid(selectedOrg));

                if (package.CFSpace != string.Empty)
                {
                    selectedSpace = spaces.Where(o => o.Name == package.CFSpace).FirstOrDefault().EntityMetadata.Guid;
                }

                var domains = await client.Spaces.ListAllDomainsForSpaceDeprecated(new Guid(selectedSpace));

                Dispatcher.Invoke(() =>
                {
                    SpacesCombo.ItemsSource = spaces;
                    SpacesCombo.DisplayMemberPath = "Name";
                    SpacesCombo.SelectedValuePath = "EntityMetadata.Guid";

                    DomainsCombo.ItemsSource = domains;
                    DomainsCombo.DisplayMemberPath = "Name";
                    DomainsCombo.SelectedValuePath = "EntityMetadata.Guid";
                });
            }
        }

        //This makes the proper selections in the comboboxes
        private void SelectLoadedValues(AppPackage package)
        {
            if (package.CFOrganization != string.Empty)
            {
                OrgCombo.SelectedValue = OrgCombo.Items.Cast<ListAllOrganizationsResponse>().Where(o => o.Name == package.CFOrganization).Select(o => o.EntityMetadata.Guid).FirstOrDefault();
            }
            if (package.CFSpace != string.Empty)
            {
                SpacesCombo.SelectedValue = SpacesCombo.Items.Cast<ListAllSpacesForOrganizationResponse>().Where(o => o.Name == package.CFSpace).Select(o => o.EntityMetadata.Guid).FirstOrDefault();
            }
            if (package.CFStack != string.Empty)
            {
                StacksCombo.SelectedValue = StacksCombo.Items.Cast<ListAllStacksResponse>().Where(o => o.Name == package.CFStack).Select(o => o.Name).FirstOrDefault();
            }
            GetRoutes(package);

            if (this.DataContext == null)
            {
                this.DataContext = package;
            }
        }


        private async void ChangeTarget_Click(object sender, RoutedEventArgs e)
        {
            AppPackage package = this.DataContext as AppPackage;
            if (package == null)
            {
                package = new AppPackage();
            }
            if (package != null)
            {
                using (var loginForm = new LoginWizardForm())
                {
                    var result = loginForm.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        var target = loginForm.CloudTarget;

                        if (target != null)
                        {
                            package.CFServerUri = target.TargetUrl.ToString();
                            package.CFUser = target.Email;
                            package.CFSkipSSLValidation = target.IgnoreSSLErrors;

                            CloudCredentialsManager.Save(target.TargetUrl, target.Email, loginForm.Password);

                            Dispatcher.Invoke(() =>
                            {
                                TargetInfo.Text = package.CFServerUri;
                            });

                            await InitClient(package);
                        }
                    }
                }
            }
        }

        private void SetStatusInfo(string imageType, string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusIcon.Source = GetBitmapImageFromResources(imageType);
                StatusIcon.ToolTip = message;
            });
        }

        private async Task InitClient(AppPackage package)
        {
            string imageType = string.Empty;
            string message = string.Empty;

            if (package != null)
            {
                client = new CloudFoundryClient(new Uri(package.CFServerUri), new System.Threading.CancellationToken(), null, package.CFSkipSSLValidation);

                if (package.CFRefreshToken != string.Empty)
                {
                    try
                    {
                        await client.Login(package.CFRefreshToken).ConfigureAwait(false);
                        imageType = "refresh";
                        message = "You are using a specific token to login";
                    }
                    catch (Exception ex)
                    {
                        imageType = "error";
                        message = string.Format(CultureInfo.InvariantCulture, "Could not login using the token in your profile. {0}", ex.Message);
                        Logger.Warning(message);
                        SetStatusInfo(imageType, message);
                        return;
                    }
                }
                else
                {
                    if (package.CFUser == string.Empty)
                    {
                        imageType = "warning";
                        message = "Please configure credentials for your target.";
                        SetStatusInfo(imageType, message);
                        return;
                    }
                    if (package.CFPassword == string.Empty)
                    {
                        if (package.CFSavedPassword == true)
                        {
                            string password = CloudCredentialsManager.GetPassword(new Uri(package.CFServerUri), package.CFUser);
                            if (password == null)
                            {
                                imageType = "warning";
                                message = "Please configure credentials for your target.";
                                SetStatusInfo(imageType, message);
                                return;
                            }
                            CloudCredentials creds = new CloudCredentials();
                            creds.User = package.CFUser;
                            creds.Password = password;
                            try
                            {
                                await client.Login(creds).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                imageType = "error";
                                message = ex.Message;
                                SetStatusInfo(imageType, message);
                                return;
                            }

                            imageType = "ok";
                            message = "Target configuration is valid";

                        }
                        else
                        {
                            imageType = "warning";
                            message = "Please configure credentials for your target.";
                            SetStatusInfo(imageType, message);
                            return;
                        }
                    }
                    else
                    {
                        CloudCredentials creds = new CloudCredentials();
                        creds.User = package.CFUser;
                        creds.Password = package.CFPassword;
                        try
                        {
                            await client.Login(creds).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            imageType = "error";
                            message = string.Format(CultureInfo.InvariantCulture, "{0}. Your password is saved in clear text in the profile!", ex.Message);
                            SetStatusInfo(imageType, message);
                            return;
                        }
                        imageType = "warning";
                        message = "Target login was successful, but you password is saved in clear text in profile!";
                    }
                }
            }
            SetStatusInfo(imageType, message);
            await Load(package);
        }

        private async void OrgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrgCombo.SelectedValue != null && OrgCombo.IsEnabled)
            {
                var orgInfo = OrgCombo.SelectedValue.ToString();

                await FillSpaces(orgInfo);
            }
        }

        private async Task FillSpaces(string orgInfo)
        {
            if (orgInfo != string.Empty)
            {
                var spaces = await client.Organizations.ListAllSpacesForOrganization(new Guid(orgInfo)).ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    SpacesCombo.ItemsSource = spaces;
                    SpacesCombo.DisplayMemberPath = "Name";
                    SpacesCombo.SelectedValuePath = "EntityMetadata.Guid";
                });
            }
        }

        private async void SpacesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpacesCombo.SelectedValue != null && SpacesCombo.IsEnabled)
            {
                var spaceInfo = SpacesCombo.SelectedValue.ToString();

                await FillDomains(spaceInfo);

            }
        }

        private async Task FillDomains(string spaceInfo)
        {
            if (spaceInfo != string.Empty)
            {
                var domains = await client.Spaces.ListAllDomainsForSpaceDeprecated(new Guid(spaceInfo)).ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    DomainsCombo.ItemsSource = domains;
                    DomainsCombo.DisplayMemberPath = "Name";
                    DomainsCombo.SelectedValuePath = "EntityMetadata.Guid";
                });
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.ProfilePath.Text))
            {
                var messagebox = MessageBoxHelper.WarningQuestion("Are you sure you want to exit? Your unsaved the changes will be lost.");
                if (messagebox == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                AppPackage package = new AppPackage();
                package.LoadFromFile(this.ProfilePath.Text);
                if (!package.IsEqualTo(this.DataContext as AppPackage))
                {
                    var messagebox = MessageBoxHelper.WarningQuestion("Are you sure you want to exit? Your unsaved the changes will be lost.");
                    if (messagebox == System.Windows.Forms.DialogResult.No)
                    {
                        return;
                    }
                }
            }
            this.Close();
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.ProfilePath.Text))
            {
                SaveAs_Click(sender, e);
                return;
            }

            SaveCurrentToFile(this.ProfilePath.Text);
            MessageBoxHelper.DisplayInfo("Target successfully saved.");



        }

        private void LocalBuild_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.Configuration.IsEnabled = !(checkBox.IsChecked == null ? false : (bool)checkBox.IsChecked);
            this.Platform.IsEnabled = !(checkBox.IsChecked == null ? false : (bool)checkBox.IsChecked);
        }

    }
}
