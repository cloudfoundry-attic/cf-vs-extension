namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.VisualStudio.PlatformUI;

    internal class ServiceInstanceSelection : ObservableObject
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
                return this.publishProfileResources.SelectedPublishProfile.Application.Services.Contains(this.ServiceInstance.Name);
            }

            set
            {
                if (value && !this.Selected)
                {
                    this.publishProfileResources.SelectedPublishProfile.Application.Services.Add(this.ServiceInstance.Name);
                }
                else if (this.Selected)
                {
                    this.publishProfileResources.SelectedPublishProfile.Application.Services.Remove(this.ServiceInstance.Name);
                }
            }
        }
    }
}
