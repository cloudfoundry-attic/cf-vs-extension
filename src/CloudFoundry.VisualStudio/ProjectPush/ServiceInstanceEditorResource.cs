using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.Threading;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class ServiceInstanceEditorResource : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ListAllServicesResponse> serviceTypes = new ObservableCollection<ListAllServicesResponse>();

        private readonly ObservableCollection<ListAllServicePlansResponse> servicePlans = new ObservableCollection<ListAllServicePlansResponse>();

        private ErrorResource errorResource = new ErrorResource();
        private EntityGuid selectedService = null;
        private EntityGuid selectedPlan = null;
        private bool refreshingServiceInformations;
        private bool allowFinish = true;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ErrorResource Error
        {
            get 
            { 
                return errorResource; 
            }
            set 
            { 
                this.errorResource = value; 
                RaisePropertyChangedEvent("Error"); 
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
                if (SelectedServiceType != null)
                {
                    return this.servicePlans.Where(o => o.ServiceGuid == SelectedServiceType.ToGuid());
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
                return selectedService;
            }
            set
            {
                selectedService = value;
                RaisePropertyChangedEvent("AvailableServicePlans");
                if (AvailableServicePlans != null)
                {
                    this.SelectedServicePlan = AvailableServicePlans.FirstOrDefault().EntityMetadata.Guid;
                }
            }
        }

        public EntityGuid SelectedServicePlan
        {
            get
            {
                return selectedPlan;
            }
            set
            {
                selectedPlan = value;
                RaisePropertyChangedEvent("SelectedServicePlan");
            }
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
            RaisePropertyChangedEvent("SelectedServiceType");
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


        public ServiceInstanceEditorResource(CloudFoundryClient client)
        {
                InitServicesInformation(client);
        }

        private void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
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
                    OnUIThread(() => { ServiceTypes.Add(service); });
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


        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
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
                RaisePropertyChangedEvent("RefreshingServiceInformations");
                RaisePropertyChangedEvent("AllowFinish");
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
                RaisePropertyChangedEvent("AllowFinish");
            }
        }
    }
}
