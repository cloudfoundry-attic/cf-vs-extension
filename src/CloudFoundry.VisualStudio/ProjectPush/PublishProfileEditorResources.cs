using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using CloudFoundry.UAA;
using CloudFoundry.VisualStudio.TargetStore;
using System.IO;
using System.Text.RegularExpressions;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class PublishProfileEditorResources : INotifyPropertyChanged
    {
        private ObservableCollection<ListAllOrganizationsResponse> orgs = new ObservableCollection<ListAllOrganizationsResponse>();
        private ObservableCollection<ListAllSpacesForOrganizationResponse> spaces = new ObservableCollection<ListAllSpacesForOrganizationResponse>();
        private ObservableCollection<ListAllStacksResponse> stacks = new ObservableCollection<ListAllStacksResponse>();
        private ObservableCollection<ListAllBuildpacksResponse> buildpacks = new ObservableCollection<ListAllBuildpacksResponse>();
        private ObservableCollection<ListAllSharedDomainsResponse> sharedDomains = new ObservableCollection<ListAllSharedDomainsResponse>();
        private ObservableCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = new ObservableCollection<ListAllPrivateDomainsForOrganizationResponse>();
        private ObservableCollection<ServiceInstanceSelection> serviceInstances = new ObservableCollection<ServiceInstanceSelection>();

        private ErrorResource errorResource = new ErrorResource();
        private string refreshMessage = "Please Wait...";

        public ErrorResource Error
        {
            get { return errorResource; }
        }

        public ObservableCollection<ListAllOrganizationsResponse> Orgs
        {
            get { return orgs; }
            set { orgs = value; }
        }

        public ObservableCollection<ListAllSpacesForOrganizationResponse> Spaces
        {
            get { return spaces; }
            set { spaces = value; }
        }

        public ObservableCollection<ListAllStacksResponse> Stacks
        {
            get { return stacks; }
            set { stacks = value; }
        }

        public ObservableCollection<ListAllBuildpacksResponse> Buildpacks
        {
            get { return buildpacks; }
            set { buildpacks = value; }
        }

        public string RefreshMessage
        {
            get { return refreshMessage; }
            set
            {
                refreshMessage = value;
                this.RaisePropertyChangedEvent("RefreshMessage");
            }
        }


        public IEnumerable<SharedDomainWithSelection> SharedDomains
        {
            get
            {
                return this.sharedDomains.Select(d => new SharedDomainWithSelection(this)
                {
                    SharedDomain = d
                });
            }
        }

        public IEnumerable<PrivateDomainsWithSelection> PrivateDomains
        {
            get
            {
                return this.privateDomains.Select(d => new PrivateDomainsWithSelection(this)
                {
                    PrivateDomain = d
                });
            }
        }

        public ObservableCollection<ServiceInstanceSelection> ServiceInstances
        {
            get
            {
                return this.serviceInstances;
            }
        }

        public string SelectedStack
        {
            get
            {
                return this.selectedPublishProfile.Application.StackName;
            }
            set
            {
                this.selectedPublishProfile.Application.StackName = value;
                this.RaisePropertyChangedEvent("SelectedStack");
            }
        }

        public string SelectedBuildpack
        {
            get
            {
                return this.selectedPublishProfile.Application.BuildpackUrl;
            }
            set
            {
                this.selectedPublishProfile.Application.BuildpackUrl = value;
                this.RaisePropertyChangedEvent("SelectedBuildpack");
            }
        }

        public PublishProfile SelectedPublishProfile
        {
            get
            {
                return this.selectedPublishProfile;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                this.selectedPublishProfile = value;

                this.Refresh(PublishProfileRefreshTarget.PublishProfile);

                this.RaisePropertyChangedEvent("SelectedPublishProfile");
            }
        }

        public CloudTarget[] CloudTargets
        {
            get;
            set;
        }

        public PublishProfile[] PublishProfiles
        {
            get;
            set;
        }

        private CloudTarget ToV2CloudTarget()
        {
            string description = string.Empty;

            // If this is a 'vanilla' publish profile, we can display it the same way; note that password can be a series of whitespaces
            if (!string.IsNullOrWhiteSpace(this.selectedPublishProfile.RefreshToken))
            {
                description = string.Format("Using explicit refresh token - {0}", this.selectedPublishProfile.ServerUri);
            }
            else if (!string.IsNullOrEmpty(this.selectedPublishProfile.Password))
            {
                description = string.Format("Using clear-text password - {0}", this.selectedPublishProfile.ServerUri);
            }
            else if (!this.selectedPublishProfile.SavedPassword)
            {
                description = string.Format("Invalid credential configuration - {0}", this.selectedPublishProfile.ServerUri);
            }

            return CloudTarget.CreateV2Target(
                this.selectedPublishProfile.ServerUri,
                description,
                this.selectedPublishProfile.User,
                this.selectedPublishProfile.SkipSSLValidation,
                string.Empty);
        }

        public CloudTarget SelectedCloudTarget
        {
            get
            {
                return this.ToV2CloudTarget();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                this.selectedPublishProfile.ServerUri = value.TargetUrl;
                this.selectedPublishProfile.User = value.Email;
                this.selectedPublishProfile.SkipSSLValidation = value.IgnoreSSLErrors;
                this.selectedPublishProfile.SavedPassword = true;
                this.selectedPublishProfile.Password = null;
                this.selectedPublishProfile.RefreshToken = null;
                this.selectedPublishProfile.PropertyChanged -= publishProfile_PropertyChanged;
                this.selectedPublishProfile.PropertyChanged += publishProfile_PropertyChanged;

                this.Refresh(PublishProfileRefreshTarget.Client);
                this.RaisePropertyChangedEvent("SelectedCloudTarget");
            }
        }

        public PublishProfileRefreshTarget LastRefreshTarget
        {
            get
            {
                return this.lastRefreshTarget;
            }
            set
            {
                this.lastRefreshTarget = value;
                RaisePropertyChangedEvent("LastRefreshTarget");
            }
        }

        private PublishProfile selectedPublishProfile;

        private CancellationToken cancellationToken;
        private CloudFoundryClient client;

        public CloudFoundryClient Client
        {
            get
            {
                return client;
            }
        }

        private int RefreshCounter
        {
            get
            {
                return this.refreshCounter;
            }
            set
            {
                this.refreshCounter = Math.Max(value, 0);
                this.RaisePropertyChangedEvent("Refreshing");
            }
        }


        public bool Refreshing
        {
            get
            {
                return refreshCounter != 0;
            }
        }

        public PublishProfileEditorResources(PublishProfile publishProfile, CancellationToken cancellationToken)
        {
            this.selectedPublishProfile = publishProfile;
            this.selectedPublishProfile.PropertyChanged += publishProfile_PropertyChanged;
            this.cancellationToken = cancellationToken;

            this.CloudTargets = CloudTargetManager.GetTargets();

            var publishProfiles = new List<PublishProfile>();

            var publishDirectory = Directory.GetParent(this.selectedPublishProfile.Path);

            if (Directory.Exists(publishDirectory.FullName))
            {
                foreach (var file in publishDirectory.GetFiles())
                {
                    if (file.Name.EndsWith(PushEnvironment.Extension))
                    {
                        try
                        {
                            PushEnvironment env = new PushEnvironment();
                            env.ProfileFilePath = file.FullName;
                            var profile = PublishProfile.Load(env);
                            publishProfiles.Add(profile);
                        }
                        catch (Exception ex)
                        {
                            // Ignore profiles that cannot be loaded.
                            Logger.Warning(string.Format("Cloud not load profile from {0}: {1}", file.Name, ex));
                        }
                    }
                }
            }
            this.PublishProfiles = publishProfiles.ToArray();
        }

        private void publishProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Organization":
                    {
                        this.Refresh(PublishProfileRefreshTarget.Spaces);
                        this.Refresh(PublishProfileRefreshTarget.PrivateDomains);
                    }
                    break;
                case "Space":
                    {
                        this.Refresh(PublishProfileRefreshTarget.ServiceInstances);
                    }
                    break;
            }
        }

        private void EnterRefresh()
        {
            this.RefreshCounter++;
            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;
        }

        private void ExitRefresh()
        {
            this.ExitRefresh(null);
        }

        private void ExitRefresh(Exception error)
        {
            this.RefreshCounter--;
            this.Error.HasErrors = error != null;
            if (this.Error.HasErrors)
            {
                List<string> errors = new List<string>();
                ErrorFormatter.FormatExceptionMessage(error, errors);
                StringBuilder sb = new StringBuilder();
                foreach (string errorLine in errors)
                {
                    sb.AppendLine(errorLine);
                }

                this.Error.ErrorMessage = sb.ToString();
            }
        }

        public void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
        }

        public void Refresh(PublishProfileRefreshTarget refreshTarget)
        {
            Task.Run(async () =>
            {
                this.EnterRefresh();

                switch (refreshTarget)
                {
                    case PublishProfileRefreshTarget.PublishProfile:
                        await this.RefreshPublishProfile();
                        break;
                    case PublishProfileRefreshTarget.Client:
                        await this.RefreshClient();
                        break;
                    case PublishProfileRefreshTarget.Organizations:
                        await this.RefreshOrganizations();
                        break;
                    case PublishProfileRefreshTarget.Spaces:
                        await this.RefreshSpaces();
                        break;
                    case PublishProfileRefreshTarget.ServiceInstances:
                        await this.RefreshServiceInstances();
                        break;
                    case PublishProfileRefreshTarget.Stacks:
                        await this.RefreshStacks();
                        break;
                    case PublishProfileRefreshTarget.Buildpacks:
                        await this.RefreshBuildpacks();
                        break;
                    case PublishProfileRefreshTarget.SharedDomains:
                        await this.RefreshSharedDomains();
                        break;
                    case PublishProfileRefreshTarget.PrivateDomains:
                        await this.RefreshPrivateDomains();
                        break;
                    default:
                        break;
                }
            }).ContinueWith((antecedent) =>
            {
                if (antecedent.Exception != null)
                {
                    this.ExitRefresh(antecedent.Exception);
                }
                else
                {
                    this.ExitRefresh();
                }
            }).Forget();
        }

        private async Task RefreshPublishProfile()
        {
            await Task.Run(() =>
            {
                this.SelectedCloudTarget = this.ToV2CloudTarget();
            });

        }

        private async Task RefreshClient()
        {
            this.RefreshMessage = "Loading Cloud Foundry client...";
            this.LastRefreshTarget = PublishProfileRefreshTarget.Client;

            this.client = new CloudFoundryClient(
                this.selectedPublishProfile.ServerUri,
                this.cancellationToken,
                null,
                this.selectedPublishProfile.SkipSSLValidation);

            AuthenticationContext authenticationContext = null;
            if (!string.IsNullOrWhiteSpace(this.selectedPublishProfile.RefreshToken))
            {
                authenticationContext = await client.Login(this.selectedPublishProfile.RefreshToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(this.selectedPublishProfile.Password))
                {
                    authenticationContext = await client.Login(new CloudCredentials()
                    {
                        User = this.selectedPublishProfile.User,
                        Password = this.selectedPublishProfile.Password
                    });
                }
                else if (this.selectedPublishProfile.SavedPassword == true)
                {
                    string password = CloudCredentialsManager.GetPassword(
                        this.selectedPublishProfile.ServerUri,
                        this.selectedPublishProfile.User);

                    authenticationContext = await client.Login(new CloudCredentials()
                    {
                        User = this.selectedPublishProfile.User,
                        Password = password
                    });
                }
                else
                {
                    throw new InvalidOperationException(@"Credentials are not configured correctly in your publish profile.
Either set CFSavedPassword to true and use credentials saved in the Windows Credential Manager (recommended), or set a CFPassword or CFRefreshToken.
Please note that credentials are saved automatically in the Windows Credential Manager if you use the Cloud Foundry Visual Studio Extensions to connect to a cloud.");
                }
            }
            
             await this.RefreshOrganizations();
             await this.RefreshStacks();
             await this.RefreshBuildpacks();
             await this.RefreshSharedDomains();
        }

        private async Task RefreshOrganizations()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Organizations;

            this.RefreshMessage = "Loading organizations...";
            List<ListAllOrganizationsResponse> orgsList = new List<ListAllOrganizationsResponse>();

            PagedResponseCollection<ListAllOrganizationsResponse> orgsResponse = await client.Organizations.ListAllOrganizations();

            while (orgsResponse != null && orgsResponse.Properties.TotalResults != 0)
            {
                foreach (var org in orgsResponse)
                {
                    orgsList.Add(org);
                }

                orgsResponse = await orgsResponse.GetNextPage();
            }

            OnUIThread(() =>
            {
                var oldSelectedOrg = this.selectedPublishProfile.Organization;

                this.orgs.Clear();
                foreach (var org in orgsList)
                {
                    this.orgs.Add(org);
                }

                if (this.orgs.Any(o => o.Name == oldSelectedOrg))
                {
                    this.selectedPublishProfile.Organization = oldSelectedOrg;
                }

                if (string.IsNullOrWhiteSpace(this.selectedPublishProfile.Organization))
                {
                    if (this.Orgs.Count > 0)
                    {
                        this.selectedPublishProfile.Organization = this.Orgs.FirstOrDefault().Name;
                    }
                }
            });
        }

        private async Task RefreshSpaces()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Spaces;
            this.RefreshMessage = "Loading spaces...";

            List<ListAllSpacesForOrganizationResponse> spacesList = new List<ListAllSpacesForOrganizationResponse>();

            var org = this.orgs.FirstOrDefault(o => o.Name == this.selectedPublishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await this.client.Organizations.ListAllSpacesForOrganization(org.EntityMetadata.Guid);

            while (spaces != null && spaces.Properties.TotalResults != 0)
            {
                foreach (var space in spaces)
                {
                    spacesList.Add(space);
                }

                spaces = await spaces.GetNextPage();
            }

            OnUIThread(() =>
            {
                var oldSelectedSpace = this.selectedPublishProfile.Space;

                this.spaces.Clear();
                foreach (var space in spacesList)
                {
                    this.spaces.Add(space);
                }

                if (this.spaces.Any(o => o.Name == oldSelectedSpace))
                {
                    this.selectedPublishProfile.Space = oldSelectedSpace;
                }

                if (string.IsNullOrWhiteSpace(this.selectedPublishProfile.Space))
                {
                    this.selectedPublishProfile.Space = this.spaces.FirstOrDefault().Name;
                }
            });

            await this.RefreshServiceInstances();
        }

        private async Task RefreshServiceInstances()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.ServiceInstances;
            this.RefreshMessage = "Detecting service instances...";

            List<ServiceInstanceSelection> serviceInstancesList = new List<ServiceInstanceSelection>();

            var space = this.spaces.FirstOrDefault(s => s.Name == this.selectedPublishProfile.Space);

            if (space == null)
            {
                return;
            }

            PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> serviceInstances = await this.client.Spaces.ListAllServiceInstancesForSpace(space.EntityMetadata.Guid);

            while (serviceInstances != null && serviceInstances.Properties.TotalResults != 0)
            {
                foreach (var serviceInstance in serviceInstances)
                {
                    var servicePlan = await this.client.ServicePlans.RetrieveServicePlan(serviceInstance.ServicePlanGuid);
                    var systemService = await this.client.Services.RetrieveService(servicePlan.ServiceGuid);


                    ServiceInstanceSelection serviceProfile = new ServiceInstanceSelection(this);

                    serviceProfile.ServiceInstance = serviceInstance;
                    serviceProfile.Service = systemService;
                    serviceProfile.ServicePlan = servicePlan;
                    serviceInstancesList.Add(serviceProfile);
                }

                serviceInstances = await serviceInstances.GetNextPage();
            }


            OnUIThread(() =>
            {
                this.serviceInstances.Clear();
                foreach (var serviceProfile in serviceInstancesList)
                {
                    this.serviceInstances.Add(serviceProfile);
                }
            });

            RaisePropertyChangedEvent("ServiceInstances");
        }

        private async Task RefreshStacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Stacks;
            this.RefreshMessage = "Detecting stacks...";
            List<ListAllStacksResponse> stacksList = new List<ListAllStacksResponse>();

            PagedResponseCollection<ListAllStacksResponse> stacksResponse = await this.client.Stacks.ListAllStacks();

            while (stacksResponse != null && stacksResponse.Properties.TotalResults != 0)
            {
                foreach (var stack in stacksResponse)
                {
                    stacksList.Add(stack);
                }

                stacksResponse = await stacksResponse.GetNextPage();
            }

            OnUIThread(() =>
            {
                var oldSelectedStack = this.SelectedStack;

                this.stacks.Clear();
                foreach (var stack in stacksList)
                {
                    this.stacks.Add(stack);
                }
                if (stacks.Any(o => o.Name == oldSelectedStack))
                {
                    this.SelectedStack = oldSelectedStack;
                }

                if (string.IsNullOrWhiteSpace(this.SelectedStack))
                {
                    this.SelectedStack = this.stacks.FirstOrDefault().Name;
                }
            });

            RaisePropertyChangedEvent("Stacks");
        }

        private async Task RefreshBuildpacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Buildpacks;
            this.RefreshMessage = "Detecting buildpacks...";
            List<ListAllBuildpacksResponse> buildpacksList = new List<ListAllBuildpacksResponse>();

            PagedResponseCollection<ListAllBuildpacksResponse> buildpacksResponse = await this.client.Buildpacks.ListAllBuildpacks();

            while (buildpacksResponse != null && buildpacksResponse.Properties.TotalResults != 0)
            {
                foreach (var buildpack in buildpacksResponse)
                {
                    buildpacksList.Add(buildpack);
                }

                buildpacksResponse = await buildpacksResponse.GetNextPage();
            }


            OnUIThread(() =>
            {
                var oldSelectedBuildpack = this.SelectedBuildpack;
                
                this.buildpacks.Clear();
                foreach (var buildpack in buildpacksList)
                {
                    this.buildpacks.Add(buildpack);
                }

                if (Uri.IsWellFormedUriString(oldSelectedBuildpack, UriKind.Absolute))
                {
                    this.SelectedBuildpack = oldSelectedBuildpack;
                }
                else
                {
                    if (this.buildpacks.Any(o => o.Name == oldSelectedBuildpack))
                    {
                        this.SelectedBuildpack = oldSelectedBuildpack;
                    }
                    else
                    {
                        this.SelectedBuildpack = String.Empty;
                    }
                }
            });

            RaisePropertyChangedEvent("Buildpacks");
        }

        private async Task RefreshSharedDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.SharedDomains;
            this.RefreshMessage = "Detecting shared domains...";
            List<ListAllSharedDomainsResponse> sharedDomainsList = new List<ListAllSharedDomainsResponse>();

            PagedResponseCollection<ListAllSharedDomainsResponse> sharedDomains = await this.client.SharedDomains.ListAllSharedDomains();

            while (sharedDomains != null && sharedDomains.Properties.TotalResults != 0)
            {
                foreach (var sharedDomain in sharedDomains)
                {
                    sharedDomainsList.Add(sharedDomain);
                }

                sharedDomains = await sharedDomains.GetNextPage();
            }

            OnUIThread(() =>
            {
                this.sharedDomains.Clear();
                foreach (var sharedDomain in sharedDomainsList)
                {
                    this.sharedDomains.Add(sharedDomain);
                }
            });

            RaisePropertyChangedEvent("SharedDomains");
        }

        private async Task RefreshPrivateDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.PrivateDomains;
            this.refreshMessage = "Detecting private domains...";
            List<ListAllPrivateDomainsForOrganizationResponse> privateDomainsList = new List<ListAllPrivateDomainsForOrganizationResponse>();

            var org = this.orgs.FirstOrDefault(o => o.Name == this.selectedPublishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = await this.client.Organizations.ListAllPrivateDomainsForOrganization(org.EntityMetadata.Guid);

            while (privateDomains != null && privateDomains.Properties.TotalResults != 0)
            {
                foreach (var privateDomain in privateDomains)
                {
                    privateDomainsList.Add(privateDomain);
                }

                privateDomains = await privateDomains.GetNextPage();
            }
            OnUIThread(() =>
            {
                this.privateDomains.Clear();
                foreach (var privateDomain in privateDomainsList)
                {
                    this.privateDomains.Add(privateDomain);
                }
            });

            RaisePropertyChangedEvent("PrivateDomains");
        }

        internal void CleanManifest()
        {
            List<string> selectedDomains = new List<string>();
            foreach (var privateDomain in this.PrivateDomains)
            {
                if (privateDomain.Selected)
                {
                    selectedDomains.Add(privateDomain.PrivateDomain.Name);
                }
            }

            foreach (var sharedDomain in this.SharedDomains)
            {
                if (sharedDomain.Selected)
                {
                    selectedDomains.Add(sharedDomain.SharedDomain.Name);
                }
            }

            this.selectedPublishProfile.Application.Domains.Clear();
            this.selectedPublishProfile.Application.Domains.AddRange(selectedDomains);

            List<string> selectedServices = new List<string>();
            foreach (var servInstance in this.serviceInstances)
            {
                if (servInstance.Selected)
                {
                    selectedServices.Add(servInstance.ServiceInstance.Name);
                }
            }

            this.selectedPublishProfile.Application.Services.Clear();
            this.selectedPublishProfile.Application.Services.AddRange(selectedServices);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private PublishProfileRefreshTarget lastRefreshTarget;
        private volatile int refreshCounter;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ValidateRoutes()
        {
            Regex regex = new Regex("^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]+$");

            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;

            foreach (string hostName in this.SelectedPublishProfile.Application.Hosts)
            {
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    this.Error.ErrorMessage = "Hostname cannot be empty";
                    this.Error.HasErrors = true;
                    break;
                }
                else
                {
                    if (regex.IsMatch(hostName) == false)
                    {
                        this.Error.ErrorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Invalid hostname {0}", hostName);
                        this.Error.HasErrors = true;
                        break;
                    }
                }
            }
        }
    }
}
