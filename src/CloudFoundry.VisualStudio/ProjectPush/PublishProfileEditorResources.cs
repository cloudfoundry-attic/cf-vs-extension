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


        public PublishProfile PublishProfile
        {
            get { return publishProfile; }
            set { publishProfile = value; }
        }

        public CloudTarget[] CloudTargets
        {
            get;
            set;
        }

        public CloudTarget SelectedCloudTarget
        {
            get
            {
                string description = string.Empty;

                // If this is a 'vanila' publish profile, we can display it the same way; note that password can be a series of whitespaces
                if (!string.IsNullOrWhiteSpace(this.PublishProfile.RefreshToken))
                {
                    description = string.Format("Using explicit refresh token - {0}", this.PublishProfile.ServerUri);
                }
                else if (!string.IsNullOrEmpty(this.PublishProfile.Password))
                {
                    description = string.Format("Using clear-text password - {0}", this.PublishProfile.ServerUri);
                }
                else if (!this.PublishProfile.SavedPassword)
                {
                    description = string.Format("Invalid credential configuration - {0}", this.PublishProfile.ServerUri);
                }

                return CloudTarget.CreateV2Target(
                    this.PublishProfile.ServerUri,
                    description,
                    this.PublishProfile.User,
                    this.PublishProfile.SkipSSLValidation,
                    string.Empty);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                this.PublishProfile.ServerUri = value.TargetUrl;
                this.PublishProfile.User = value.Email;
                this.PublishProfile.SkipSSLValidation = value.IgnoreSSLErrors;
                this.PublishProfile.SavedPassword = true;
                this.PublishProfile.Password = null;
                this.PublishProfile.RefreshToken = null;

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

        private PublishProfile publishProfile;

        private bool refreshing = false;
        private CancellationToken cancellationToken;
        private CloudFoundryClient client;

        public CloudFoundryClient Client 
        {
            get 
            { 
                return client; 
            } 
        }
        
        public bool Refreshing
        {
            get
            {
                return refreshing;
            }
            set
            {
                refreshing = value;
                this.RaisePropertyChangedEvent("Refreshing");
            }
        }

        public PublishProfileEditorResources(PublishProfile publishProfile, CancellationToken cancellationToken)
        {
            this.publishProfile = publishProfile;
            this.publishProfile.PropertyChanged += publishProfile_PropertyChanged;
            this.cancellationToken = cancellationToken;

            this.CloudTargets = CloudTargetManager.GetTargets();
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
            this.Refreshing = true;
            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;
        }

        private void ExitRefresh()
        {
            this.ExitRefresh(null);
        }

        private void ExitRefresh(Exception error)
        {
            this.Refreshing = false;
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

        private async Task RefreshClient()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Client;

            this.client = new CloudFoundryClient(
                this.publishProfile.ServerUri,
                this.cancellationToken,
                null,
                this.publishProfile.SkipSSLValidation);

            AuthenticationContext authenticationContext = null;
            if (!string.IsNullOrWhiteSpace(this.PublishProfile.RefreshToken))
            {
                authenticationContext = await client.Login(this.PublishProfile.RefreshToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(this.PublishProfile.Password))
                {
                    authenticationContext = await client.Login(new CloudCredentials()
                    {
                        User = this.publishProfile.User,
                        Password = this.publishProfile.Password
                    });
                }
                else if (this.publishProfile.SavedPassword == true)
                {
                    string password = CloudCredentialsManager.GetPassword(
                        this.publishProfile.ServerUri,
                        this.publishProfile.User);

                    authenticationContext = await client.Login(new CloudCredentials()
                    {
                        User = this.publishProfile.User,
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
            await this.RefreshPrivateDomains();
            await this.RefreshServiceInstances();
        }

        private async Task RefreshOrganizations()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Organizations;

            OnUIThread(() => this.orgs.Clear());

            PagedResponseCollection<ListAllOrganizationsResponse> orgs = await client.Organizations.ListAllOrganizations();

            while (orgs != null && orgs.Properties.TotalResults != 0)
            {
                foreach (var org in orgs)
                {
                    OnUIThread(() => this.orgs.Add(org));
                }

                orgs = await orgs.GetNextPage();
            }

            await this.RefreshSpaces();
            await this.RefreshPrivateDomains();
        }

        private async Task RefreshSpaces()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Spaces;

            OnUIThread(() => this.spaces.Clear());

            var org = this.orgs.FirstOrDefault(o => o.Name == this.publishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaces = await this.client.Organizations.ListAllSpacesForOrganization(org.EntityMetadata.Guid);

            while (spaces != null && spaces.Properties.TotalResults != 0)
            {
                foreach (var space in spaces)
                {
                    OnUIThread(() => this.spaces.Add(space));
                }

                spaces = await spaces.GetNextPage();
            }

            await this.RefreshServiceInstances();
        }

        private async Task RefreshServiceInstances()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.ServiceInstances;

            OnUIThread(() => this.serviceInstances.Clear());

            var space = this.spaces.FirstOrDefault(s => s.Name == this.publishProfile.Space);

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

                    OnUIThread(() => this.serviceInstances.Add(serviceProfile));
                }

                serviceInstances = await serviceInstances.GetNextPage();
            }
            RaisePropertyChangedEvent("ServiceInstances");
        }

        private async Task RefreshStacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Stacks;

            OnUIThread(() => this.stacks.Clear());

            PagedResponseCollection<ListAllStacksResponse> stacks = await this.client.Stacks.ListAllStacks();

            while (stacks != null && stacks.Properties.TotalResults != 0)
            {
                foreach (var stack in stacks)
                {
                    OnUIThread(() => this.stacks.Add(stack));
                }

                stacks = await stacks.GetNextPage();
            }
        }

        private async Task RefreshBuildpacks()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.Buildpacks;

            OnUIThread(() => this.buildpacks.Clear());

            PagedResponseCollection<ListAllBuildpacksResponse> buildpacks = await this.client.Buildpacks.ListAllBuildpacks();

            while (buildpacks != null && buildpacks.Properties.TotalResults != 0)
            {
                foreach (var buildpack in buildpacks)
                {
                    OnUIThread(() => this.buildpacks.Add(buildpack));
                }

                buildpacks = await buildpacks.GetNextPage();
            }
        }

        private async Task RefreshSharedDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.SharedDomains;

            OnUIThread(() => this.sharedDomains.Clear());

            PagedResponseCollection<ListAllSharedDomainsResponse> sharedDomains = await this.client.SharedDomains.ListAllSharedDomains();

            while (sharedDomains != null && sharedDomains.Properties.TotalResults != 0)
            {
                foreach (var sharedDomain in sharedDomains)
                {
                    OnUIThread(() => this.sharedDomains.Add(sharedDomain));
                }

                sharedDomains = await sharedDomains.GetNextPage();
            }

            RaisePropertyChangedEvent("SharedDomains");
        }

        private async Task RefreshPrivateDomains()
        {
            this.LastRefreshTarget = PublishProfileRefreshTarget.PrivateDomains;

            OnUIThread(() => this.privateDomains.Clear());

            var org = this.orgs.FirstOrDefault(o => o.Name == this.publishProfile.Organization);

            if (org == null)
            {
                return;
            }

            PagedResponseCollection<ListAllPrivateDomainsForOrganizationResponse> privateDomains = await this.client.Organizations.ListAllPrivateDomainsForOrganization(org.EntityMetadata.Guid);

            while (privateDomains != null && privateDomains.Properties.TotalResults != 0)
            {
                foreach (var privateDomain in privateDomains)
                {
                    OnUIThread(() => this.privateDomains.Add(privateDomain));
                }

                privateDomains = await privateDomains.GetNextPage();
            }

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

            this.publishProfile.Application.Domains.Clear();
            this.publishProfile.Application.Domains.AddRange(selectedDomains);

            List<string> selectedServices = new List<string>();
            foreach (var servInstance in this.serviceInstances)
            {
                if (servInstance.Selected)
                {
                    selectedServices.Add(servInstance.ServiceInstance.Name);
                }
            }

            this.publishProfile.Application.Services.Clear();
            this.publishProfile.Application.Services.AddRange(selectedServices);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private PublishProfileRefreshTarget lastRefreshTarget;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
