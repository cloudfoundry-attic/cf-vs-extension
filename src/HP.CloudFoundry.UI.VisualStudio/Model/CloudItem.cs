using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.CloudController.V2.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public CancellationToken CancellationToken
        {
            get
            {
                return this.cancellationToken;
            }
        }

        public CloudItemType ItemType
        {
            get
            {
                return cloudItemType;
            }
        }

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

        public BitmapImage Icon
        {
            get
            {
                Bitmap bitmap = this.IconBitmap;

                if (bitmap != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        return bitmapImage;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public abstract string Text
        {
            get;
        }

        protected abstract Bitmap IconBitmap
        {
            get;
        }

        protected abstract Task<IEnumerable<CloudItem>> UpdateChildren();

        public ObservableCollection<CloudItem> Children
        {
            get
            {
                return this.children;
            }
        }
    }
}
