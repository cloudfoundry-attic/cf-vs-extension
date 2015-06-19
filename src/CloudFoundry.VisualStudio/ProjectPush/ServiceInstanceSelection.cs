using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class ServiceInstanceSelection : INotifyPropertyChanged
    {
        private PublishProfileEditorResources publishProfileResources;

        public ServiceInstanceSelection(PublishProfileEditorResources publishProfileResources)
        {
            this.publishProfileResources = publishProfileResources;
        }

        public ListAllServiceInstancesForSpaceResponse ServiceInstance
        {
            get;
            set;
        }

        public RetrieveServicePlanResponse ServicePlan
        {
            get;
            set;
        }

        public RetrieveServiceResponse Service
        {
            get;
            set;
        }

        public bool Selected
        {
            get
            {
                return this.publishProfileResources.PublishProfile.Application.Services.Contains(this.ServiceInstance.Name);
            }
            set
            {
                if (value && !this.Selected)
                {
                    this.publishProfileResources.PublishProfile.Application.Domains.Add(this.ServiceInstance.Name);
                }
                else if (this.Selected)
                {
                    this.publishProfileResources.PublishProfile.Application.Domains.Remove(this.ServiceInstance.Name);
                }

                RaisePropertyChangedEvent("Selected");
            }
        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
