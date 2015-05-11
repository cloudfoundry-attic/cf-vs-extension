using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using CloudFoundry.VisualStudio.Forms;
using CloudFoundry.VisualStudio.ProjectPush;
using CloudFoundry.VisualStudio.TargetStore;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.PlatformUI;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool IsBadInit { get; set; }
        public EditDialog(AppPackage package, Project currentProject)
        {
            InitializeComponent();

            this.currentProj = currentProject;

            if (string.IsNullOrWhiteSpace(package.CFAppPath))
            {
                if ((package = SetCFAppPath(package)) == null)
                {
                    MessageBoxHelper.DisplayError("You cannot build a solution on the server if its path does not contain the projects that you want to be published");
                    IsBadInit = true;
                    return;
                }
            }

            this.PublishProfile = "push";
            this.DataContext = package;
            this.ProfilePath.Text = package.ConfigFile;

            LoadProjectConfigurationsAndPlatforms();
            Init(package);
            IsBadInit = false;
        }

        private void LoadProjectConfigurationsAndPlatforms()
        {
            if (this.currentProj != null && this.currentProj.ConfigurationManager != null)
            {
                var configurations = this.currentProj.ConfigurationManager.ConfigurationRowNames as Array;
                var platforms = this.currentProj.ConfigurationManager.SupportedPlatforms as Array;

                if (configurations != null)
                {
                    this.Configurations.ItemsSource = configurations;
                }

                if (platforms != null)
                {
                    this.Platforms.ItemsSource = platforms;
                }
            }
        }

        private async void Init(AppPackage package)
        {
            if (package.CFServerUri != string.Empty)
            {
                this.IsEnabled = false;
                bool initClientError = false;
                await InitClient(package).ContinueWith((antecedent) =>
                {
                    if (antecedent.IsFaulted)
                    {
                        var errorMessages = new List<string>();
                        ErrorFormatter.FormatExceptionMessage(antecedent.Exception, errorMessages);
                        MessageBoxHelper.DisplayError(string.Join(Environment.NewLine, errorMessages));
                        initClientError = true;
                    }
                });
                if (!initClientError)
                {
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
                }

                Dispatcher.Invoke(() =>
                    {
                        this.IsEnabled = true;

                        if (OrgCombo.SelectedItem == null && (OrgCombo.Items != null && OrgCombo.Items.Count > 0))
                        {
                            OrgCombo.SelectedItem = OrgCombo.Items[0];
                            //OrgCombo_SelectionChanged(OrgCombo, null);
                        }
                    });
            }
            else
            {
                StatusIcon.Source = GetBitmapImageFromResources("warning");
                StatusIcon.ToolTip = "Please set a target";
                this.Publish.IsEnabled = false;
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
                string projectDir = System.IO.Path.GetDirectoryName(currentProj.FullName);
                string profilesDir = System.IO.Path.Combine(projectDir, "Properties", "PublishProfiles");

                if (Directory.Exists(profilesDir))
                {
                    saveDialog.RestoreDirectory = true;
                    saveDialog.InitialDirectory = profilesDir;
                }
                else if (Directory.Exists(projectDir))
                {
                    saveDialog.RestoreDirectory = true;
                    saveDialog.InitialDirectory = projectDir;
                }
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
                    string projRoot = System.IO.Path.GetDirectoryName(currentProj.FullName);
                    string projProfileDirPath = System.IO.Path.Combine(projRoot, "Properties", "PublishProfiles");

                    if (projProfileDirPath.ToUpperInvariant() == System.IO.Path.GetDirectoryName(filepath).ToUpperInvariant())
                    {
                        currentProj.ProjectItems.AddFromFile(filepath);
                    }
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
            if (ValidateServicesInformation() == false)
            {
                return;
            }
            SaveContext();
            AppPackage package = this.DataContext as AppPackage;

            if (package != null)
            {
                if (string.IsNullOrWhiteSpace(this.ProfilePath.Text))
                {
                    string tempFile = System.IO.Path.GetTempFileName();
                    package.SaveToFile(tempFile);
                }
                else
                {
                    package.SaveToFile(this.ProfilePath.Text);
                }

                var componentModel = (IComponentModel)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(SComponentModel));

                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                if (installerServices.IsPackageInstalled(currentProj, CloudFoundry_VisualStudioPackage.packageId) == false)
                {
                    MessageBoxHelper.DisplayWarning(string.Format(CultureInfo.InvariantCulture, "Cannot publish to Cloud Foundry{0}Cloud Foundry MSBuild Tasks is not installed in your project, please install it and try again", Environment.NewLine));
                }
                else
                {
                    Push(package);
                }
                this.Close();
            }
            else
            {
                MessageBoxHelper.DisplayWarning("Nothing to push, please check settings and try again");
            }


        }

        private bool ValidateServicesInformation()
        {
            string[] servicesList = Services.Text.Split(';');
            foreach (string serviceDetails in servicesList)
            {
                if (string.IsNullOrWhiteSpace(serviceDetails) == false)
                {
                    if (serviceDetails.Split(',').Length != 3)
                    {
                        MessageBoxHelper.DisplayWarning(string.Format(CultureInfo.InvariantCulture, "Insufficient information in {0}", serviceDetails));
                        return false;
                    }
                }
            }
            return true;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            System.Windows.Forms.OpenFileDialog dialogOpen = new System.Windows.Forms.OpenFileDialog();
            if (currentProj != null)
            {
                string projectDir = System.IO.Path.GetDirectoryName(currentProj.FullName);
                string profilesDir = System.IO.Path.Combine(projectDir, "Properties", "PublishProfiles");

                if (Directory.Exists(profilesDir))
                {
                    dialogOpen.RestoreDirectory = true;
                    dialogOpen.InitialDirectory = profilesDir;
                }
                else if (Directory.Exists(projectDir))
                {
                    dialogOpen.RestoreDirectory = true;
                    dialogOpen.InitialDirectory = projectDir;
                }
            }

            dialogOpen.Filter = string.Format(CultureInfo.InvariantCulture, "CF Publish file | *{0}", CloudFoundry_VisualStudioPackage.extension);

            if (dialogOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ProfilePath.Text = dialogOpen.FileName;
                this.DataContext = null;
                AppPackage package = new AppPackage();
                try
                {
                    package.LoadFromFile(dialogOpen.FileName);
                    this.DataContext = package;
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
                    OrgCombo.ItemsSource = null;
                    SpacesCombo.ItemsSource = null;
                    DomainsCombo.ItemsSource = null;
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
                this.LocalBuild.IsChecked = package.CFLocalBuild;
                LocalBuild_Click(LocalBuild, null);
                if (package.CFLocalBuild)
                {
                    this.Platforms.Text = package.CFMSBuildPlatform;
                    this.Configurations.Text = package.CFMSBuildConfiguration;
                }

                OrgCombo.DisplayMemberPath = "Name";
                OrgCombo.ItemsSource = orgs;
                OrgCombo.SelectedValuePath = "EntityMetadata.Guid";

                StacksCombo.ItemsSource = stacks;
                StacksCombo.DisplayMemberPath = "Name";
                StacksCombo.SelectedValuePath = "Name";
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
                        this.IsEnabled = false;
                        var target = loginForm.CloudTarget;

                        if (target != null)
                        {
                            package.CFOrganization = string.Empty;
                            package.CFPassword = string.Empty;
                            package.CFSavedPassword = true;
                            package.CFServerUri = target.TargetUrl.ToString();
                            package.CFUser = target.Email;
                            package.CFSkipSSLValidation = target.IgnoreSSLErrors;

                            if ((package = SetCFAppPath(package)) == null)
                            {
                                MessageBoxHelper.DisplayError("You cannot build a solution on the server if its path does not contain the projects that you want to be published");
                                return;
                            }

                            CloudCredentialsManager.Save(target.TargetUrl, target.Email, loginForm.Password);

                            Dispatcher.Invoke(() =>
                            {
                                TargetInfo.Text = package.CFServerUri;
                            });

                            OrgCombo.ItemsSource = null;
                            StacksCombo.ItemsSource = null;
                            SpacesCombo.ItemsSource = null;
                            DomainsCombo.ItemsSource = null;

                            bool initClientError = false;
                            await InitClient(package).ContinueWith((antecedent) =>
                            {
                                if (antecedent.IsFaulted)
                                {
                                    var errorMessages = new List<string>();
                                    ErrorFormatter.FormatExceptionMessage(antecedent.Exception, errorMessages);
                                    MessageBoxHelper.DisplayError(string.Join(Environment.NewLine, errorMessages));
                                    initClientError = true;
                                }
                            });
                            if (!initClientError)
                            {
                                await Load(package).ContinueWith((antecedent) =>
                                {
                                    if (antecedent.IsFaulted)
                                    {
                                        var errorMessages = new List<string>();
                                        ErrorFormatter.FormatExceptionMessage(antecedent.Exception, errorMessages);
                                        MessageBoxHelper.DisplayError(string.Join(Environment.NewLine, errorMessages));
                                    }
                                });
                            }

                            this.IsEnabled = true;

                            if (OrgCombo.SelectedItem == null && (OrgCombo.Items != null && OrgCombo.Items.Count > 0))
                            {
                                OrgCombo.SelectedItem = OrgCombo.Items[0];
                                //OrgCombo_SelectionChanged(OrgCombo, null);
                            }
                        }
                    }
                }
            }
        }

        private AppPackage SetCFAppPath(AppPackage package)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

            string projectPath = System.IO.Path.GetDirectoryName(currentProj.FullName);
            string solutionPath = System.IO.Path.GetDirectoryName(dte.Solution.FullName);

            if (System.IO.Directory.GetDirectories(solutionPath).Contains(projectPath) == false)
            {
                return null;
            }

            package.CFAppPath = GetRelativePath(solutionPath, projectPath);

            return package;
        }

        private void SetStatusInfo(string imageType, string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusIcon.Source = GetBitmapImageFromResources(imageType);
                StatusIcon.ToolTip = message;
            });
        }
        private void DisablePublishButton()
        {
            Dispatcher.Invoke(() =>
            {
                this.Publish.IsEnabled = false;
            });
        }

        private async Task InitClient(AppPackage package)
        {
            string imageType = string.Empty;
            string message = string.Empty;
            this.Publish.IsEnabled = true;

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
                        DisablePublishButton();
                        throw ex;
                    }
                }
                else
                {
                    if (package.CFUser == string.Empty)
                    {
                        imageType = "warning";
                        message = "Please configure credentials for your target.";
                        SetStatusInfo(imageType, message);
                        DisablePublishButton();
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
                                DisablePublishButton();
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
                                DisablePublishButton();
                                throw ex;
                            }

                            imageType = "ok";
                            message = "Target configuration is valid";

                        }
                        else
                        {
                            imageType = "warning";
                            message = "Please configure credentials for your target.";
                            SetStatusInfo(imageType, message);
                            DisablePublishButton();
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
                            DisablePublishButton();
                            throw ex;
                        }
                        imageType = "warning";
                        message = "Target login was successful, but your password is saved in clear text in profile!";
                    }
                }
            }
            SetStatusInfo(imageType, message);
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

                    if (SpacesCombo.SelectedItem == null && (SpacesCombo.Items != null && SpacesCombo.Items.Count > 0))
                    {
                        SpacesCombo.SelectedItem = SpacesCombo.Items[0];
                    }
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

                    if (DomainsCombo.SelectedItem == null && (DomainsCombo.Items != null && DomainsCombo.Items.Count > 0))
                    {
                        DomainsCombo.SelectedItem = DomainsCombo.Items[0];
                    }
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
            this.Configurations.IsEnabled = !(checkBox.IsChecked == null ? false : (bool)checkBox.IsChecked);
            this.Platforms.IsEnabled = !(checkBox.IsChecked == null ? false : (bool)checkBox.IsChecked);
        }

        private async void Push(AppPackage package)
        {
            DTE dte = (DTE)CloudFoundry_VisualStudioPackage.GetGlobalService(typeof(DTE));

            var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            var output = (OutputWindow)window.Object;
            bool paneFound = false;

            OutputWindowPane pane = null;
            
            try
            {
                foreach (OutputWindowPane item in output.OutputWindowPanes)
                {
                    if (item.Name == "Publish")
                    {
                        pane = output.OutputWindowPanes.Item("Publish");
                        paneFound = true;
                        break;
                    }
                }
                if (paneFound == false)
                {
                    pane = output.OutputWindowPanes.Add("Publish");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
 
            dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();

            if (currentProj != null)
            {
                string msBuildPath = Microsoft.Build.Utilities.ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", "12.0");
                string projectPath = currentProj.FullName;
                string solutionPath = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
                string projectName = currentProj.Name;
                string profileName = this.PublishProfile;
                bool localBuild = package.CFLocalBuild;

                await System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    string arguments = string.Empty;

                    if (localBuild == true)
                    {
                        arguments = string.Format(CultureInfo.InvariantCulture, @"/p:DeployOnBuild=true;PublishProfile=""{0}"" ""{1}""",
                            package.ConfigFile, projectPath);
                    }
                    else
                    {
                        arguments = string.Format(CultureInfo.InvariantCulture,
                            @"/p:DeployOnBuild=true;PublishProfile=""{0}"" /p:PUBLISH_WEBSITE={1} ""{2}""", package.ConfigFile,
                            projectName, projectPath);
                    }

                    var startInfo = new ProcessStartInfo(msBuildPath)
                    {
                        Arguments = arguments,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(projectPath),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    pane.OutputString("> msbuild " + startInfo.Arguments);


                    using (var process = System.Diagnostics.Process.Start(startInfo))
                    {
                        process.OutputDataReceived += (s, e) =>
                        {
                            lock (pane)
                            {
                                pane.OutputString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", e.Data, Environment.NewLine));
                            }
                        };

                        process.ErrorDataReceived += (s, e) =>
                        {
                            lock (pane)
                            {
                                if (!string.IsNullOrWhiteSpace(e.Data))
                                {
                                    pane.OutputString(string.Format(CultureInfo.InvariantCulture, "ERROR: {0},{1}", e.Data, Environment.NewLine));
                                }
                            }
                        };

                        process.BeginErrorReadLine();
                        process.BeginOutputReadLine();

                        process.WaitForExit();
                    }

                });
            }
        }

        internal string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                folder += System.IO.Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }
    }
}
