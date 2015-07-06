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
        private Guid selectedService;
        private bool enableForm = false;
        private string refreshMessage = string.Empty;

        public bool Refreshing
        {
            get
            {
                return enableForm;
            }
            set
            {
                enableForm = value;
                RaisePropertyChangedEvent("Refreshing");
            }
        }

        public string RefreshMessage
        {
            get 
            { 
                return refreshMessage; 
            }
            set
            {
                refreshMessage = value;
                RaisePropertyChangedEvent("RefreshMessage");
            }
        }

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
            }
        }

        private void EnterInit()
        {
            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;
        }

        private void ExitInit()
        {
            this.Refreshing = false;
            this.ExitInit(null);
        }

        private void ExitInit(Exception error)
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
            this.Refreshing = true;
            this.RefreshMessage = "Loading service informations...";
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

        public event PropertyChangedEventHandler PropertyChanged;

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
