using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    internal abstract class CloudItem
    {
        private CloudItemType cloudItemType = CloudItemType.Target;
        private volatile bool isExpanded = false;
        private volatile bool wasRefreshed = false;

        public event EventHandler<ErrorEventArgs> RefreshChildrenError;
        public event EventHandler<EventArgs> RefreshChildrenComplete;
        private AsyncObservableCollection<CloudItem> children = new AsyncObservableCollection<CloudItem>();
        private System.Threading.CancellationToken cancellationToken;

        protected CloudItem(CloudItemType cloudItemType)
        {
            this.cloudItemType = cloudItemType;

            if (this.cloudItemType != CloudItemType.LoadingPlaceholder && 
                this.cloudItemType != CloudItemType.App && 
                this.cloudItemType != CloudItemType.Route && 
                this.cloudItemType != CloudItemType.Service && 
                this.cloudItemType != CloudItemType.Error)
            {
                this.children.Add(new LoadingPlaceholder());
            }
        }

        [Browsable(false)]
        public CancellationToken CancellationToken
        {
            get
            {
                return this.cancellationToken;
            }
        }

        [Browsable(false)]
        public CloudItemType ItemType
        {
            get
            {
                return cloudItemType;
            }
        }

        [Browsable(false)]
        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                this.isExpanded = value;

                if (this.isExpanded && !this.wasRefreshed)
                {
                    this.RefreshChildren();
                }
            }
        }

        public void RefreshChildren()
        {
            var populateChildrenTask = this.UpdateChildren();
            this.cancellationToken = new System.Threading.CancellationToken();

            populateChildrenTask.ContinueWith((antecedent) =>
                {
                    if (antecedent.IsFaulted)
                    {
                        this.children.Clear();

                        CloudError error = new CloudError(antecedent.Exception);

                        this.children.Add(error);

                        if (this.RefreshChildrenError != null)
                        {
                            this.RefreshChildrenError(this, new ErrorEventArgs(){
                                Error = antecedent.Exception
                            });
                        }
                    }
                    else
                    {
                        this.children.Clear();

                        foreach (var child in antecedent.Result)
                        {
                            this.children.Add(child);
                        }

                        this.wasRefreshed = true;
                        if (this.RefreshChildrenComplete != null)
                        {
                            this.RefreshChildrenComplete(this, new EventArgs());
                        }
                    }
                });
        }

        [Browsable(false)]
        public BitmapImage Icon
        {
            get
            {
                return ImageConverter.ConvertBitmapToBitmapImage(this.IconBitmap);
            }
        }

        [Browsable(false)]
        public abstract string Text
        {
            get;
        }

        protected abstract Bitmap IconBitmap
        {
            get;
        }

        protected abstract Task<IEnumerable<CloudItem>> UpdateChildren();

        [Browsable(false)]
        public ObservableCollection<CloudItem> Children
        {
            get
            {
                return this.children;
            }
        }

        [Browsable(false)]
        public abstract ObservableCollection<CloudItemAction> Actions
        {
            get;
        }
    }
}
