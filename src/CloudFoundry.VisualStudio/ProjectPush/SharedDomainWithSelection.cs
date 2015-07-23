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

    internal class SharedDomainWithSelection : ObservableObject
    {
        private PublishProfileEditorResources publishProfileResources;

        public SharedDomainWithSelection(PublishProfileEditorResources publishProfileResources)
        {
            this.publishProfileResources = publishProfileResources;
        }

        public ListAllSharedDomainsResponse SharedDomain
        {
            get;
            set;
        }

        public bool Selected
        {
            get
            {
                return this.publishProfileResources.SelectedPublishProfile.Application.Domains.Contains(this.SharedDomain.Name);
            }

            set
            {
                if (value && !this.Selected)
                {
                    this.publishProfileResources.SelectedPublishProfile.Application.Domains.Add(this.SharedDomain.Name);
                }
                else if (this.Selected)
                {
                    this.publishProfileResources.SelectedPublishProfile.Application.Domains.Remove(this.SharedDomain.Name);
                }
            }
        }
    }
}
