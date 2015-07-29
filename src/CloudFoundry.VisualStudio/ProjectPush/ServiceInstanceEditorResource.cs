namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.VisualStudio.Threading;

    internal class ServiceInstanceEditorResource : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ListAllServicesResponse> serviceTypes = new ObservableCollection<ListAllServicesResponse>();

        private readonly ObservableCollection<ListAllServicePlansResponse> servicePlans = new ObservableCollection<ListAllServicePlansResponse>();

        private ErrorResource errorResource = new ErrorResource();
        private EntityGuid selectedService = null;
        private EntityGuid selectedPlan = null;
        private bool refreshingServiceInformations;
        private bool allowFinish = true;

        public ServiceInstanceEditorResource(CloudFoundryClient client)
        {
            this.InitServicesInformation(client);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public ErrorResource Error
        {
            get 
            {
                return this.errorResource; 
            }

            set 
            { 
                this.errorResource = value;
                this.RaisePropertyChangedEvent("Error"); 
            }
        }

        public ObservableCollection<ListAllServicesResponse> ServiceTypes
        {
            get
            {
                return this.serviceTypes;
            }
        }

        public IEnumerable<ListAllServicePlansResponse> AvailableServicePlans
        {
            get
            {
                if (this.SelectedServiceType != null)
                {
                    return this.servicePlans.Where(o => o.ServiceGuid == this.SelectedServiceType.ToGuid());
                }
                else
                {
                    return new List<ListAllServicePlansResponse>();
                }
            }
        }

        public EntityGuid SelectedServiceType
        {
            get
            {
                return this.selectedService;
            }

            set
            {
                this.selectedService = value;
                this.RaisePropertyChangedEvent("AvailableServicePlans");
                if (this.AvailableServicePlans != null)
                {
                    this.SelectedServicePlan = this.AvailableServicePlans.FirstOrDefault().EntityMetadata.Guid;
                }
            }
        }

        public EntityGuid SelectedServicePlan
        {
            get
            {
                return this.selectedPlan;
            }

            set
            {
                this.selectedPlan = value;
                this.RaisePropertyChangedEvent("SelectedServicePlan");
            }
        }
      
        public bool RefreshingServiceInformations 
        {
            get
            {
                return this.refreshingServiceInformations;
            }

            private set
            {
                this.refreshingServiceInformations = value;
                this.RaisePropertyChangedEvent("RefreshingServiceInformations");
                this.RaisePropertyChangedEvent("AllowFinish");
            }
        }

        public bool AllowFinish
        {
            get
            {
                return this.allowFinish && !this.refreshingServiceInformations;
            }

            set
            {
                this.allowFinish = value;
                this.RaisePropertyChangedEvent("AllowFinish");
            }
        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
        }

        private void EnterInit()
        {
            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;
        }

        private void ExitInit()
        {
            this.RefreshingServiceInformations = false;
            this.ExitInit(null);
            if (this.selectedService == null)
            {
                this.SelectedServiceType = this.ServiceTypes.FirstOrDefault().EntityMetadata.Guid;
            }

            this.RaisePropertyChangedEvent("SelectedServiceType");
        }

        private void ExitInit(Exception error)
        {
            this.RefreshingServiceInformations = false;
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

        private void InitServicesInformation(CloudFoundryClient client)
        {
            this.RefreshingServiceInformations = true;
            Task.Run(async () =>
            {
                this.EnterInit();

                var services = await client.Services.ListAllServices();
                foreach (var service in services)
                {
                    if (service.Active == true)
                    {
                        OnUIThread(() => { ServiceTypes.Add(service); });
                    }
                }

                var plans = await client.ServicePlans.ListAllServicePlans();
                foreach (var plan in plans)
                {
                    OnUIThread(() => { servicePlans.Add(plan); });
                }
            }).ContinueWith((antecedent) =>
            {
                if (antecedent.Exception != null)
                {
                    this.ExitInit(antecedent.Exception);
                }
                else
                {
                    this.ExitInit();
                }
            }).Forget();
        }
    }
}
