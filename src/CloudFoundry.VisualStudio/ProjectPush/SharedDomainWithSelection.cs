using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class SharedDomainWithSelection : INotifyPropertyChanged
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
                return this.publishProfileResources.PublishProfile.Application.Domains.Contains(this.SharedDomain.Name);
            }
            set
            {
                if (value && !this.Selected)
                {
                    this.publishProfileResources.PublishProfile.Application.Domains.Add(this.SharedDomain.Name);
                }
                else if (this.Selected)
                {
                    this.publishProfileResources.PublishProfile.Application.Domains.Remove(this.SharedDomain.Name);
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
