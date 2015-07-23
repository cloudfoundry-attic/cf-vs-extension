namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using Microsoft.VisualStudio.Threading;

    internal abstract class CloudItem : INotifyPropertyChanged
    {
        private readonly CloudItemType cloudItemType = CloudItemType.Target;
        private readonly ObservableCollection<CloudItem> children = new ObservableCollection<CloudItem>();
        private readonly object childRefreshLock = new object();
        private volatile bool isExpanded = false;
        private volatile bool wasRefreshed = false;
        private System.Threading.CancellationToken cancellationToken;
        private CloudItem parent = null;
        private bool executingBackgroundAction;

        protected CloudItem(CloudItemType cloudItemType)
        {
            this.cloudItemType = cloudItemType;

            if (this.HasRefresh)
            {
                this.children.Add(new LoadingPlaceholder());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                return this.cloudItemType;
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
                    this.RefreshChildren().Forget();
                }
            }
        }

        [Browsable(false)]
        public CloudItem Parent
        {
            get
            {
                return this.parent;
            }
        }

        [Browsable(false)]
        public BitmapImage Icon
        {
            get
            {
                return Converters.ImageConverter.ConvertBitmapToBitmapImage(this.IconBitmap);
            }
        }

        [Browsable(false)]
        public bool ExecutingBackgroundAction
        {
            get
            {
                return this.executingBackgroundAction;
            }

            set
            {
                this.executingBackgroundAction = value;
                this.NotifyPropertyChanged("ExecutingBackgroundAction");
            }
        }

        [Browsable(false)]
        public abstract string Text
        {
            get;
        }

        [Browsable(false)]
        public ObservableCollection<CloudItem> Children
        {
            get
            {
                return this.children;
            }
        }

        [Browsable(false)]
        public ObservableCollection<CloudItemAction> Actions
        {
            get
            {
                ObservableCollection<CloudItemAction> result = new ObservableCollection<CloudItemAction>();
                var menuAction = this.MenuActions;

                if (this.HasRefresh)
                {
                    result.Add(new CloudItemAction(this, "Refresh", Resources.Refresh, this.RefreshChildren));
                    if (menuAction != null && menuAction.Count() > 0)
                    {
                        result.Add(new CloudItemAction(this, "-", null, () => { return Task.Delay(0); }));
                    }
                }

                if (this.MenuActions != null)
                {
                    foreach (var action in this.MenuActions)
                    {
                        result.Add(action);
                    }
                }

                if (result.Count == 0)
                {
                    return null;
                }
                else
                {
                    return result;
                }
            }
        }

        protected bool HasRefresh
        {
            get
            {
                return this.cloudItemType != CloudItemType.LoadingPlaceholder &&
                    this.cloudItemType != CloudItemType.App &&
                    this.cloudItemType != CloudItemType.Route &&
                    this.cloudItemType != CloudItemType.Service &&
                    this.cloudItemType != CloudItemType.Error;
            }
        }

        protected abstract Bitmap IconBitmap
        {
            get;
        }

        [Browsable(false)]
        protected abstract IEnumerable<CloudItemAction> MenuActions
        {
            get;
        }

        public async Task RefreshChildren()
        {
            this.ExecutingBackgroundAction = true;
            var populateChildrenTask = this.UpdateChildren();
            this.cancellationToken = new System.Threading.CancellationToken();

            await populateChildrenTask.ContinueWith((antecedent) =>
            {
                lock (this.childRefreshLock)
                {
                    if (antecedent.IsFaulted)
                    {
                        OnUIThread(() => children.Clear());

                        CloudError error = new CloudError(antecedent.Exception);

                        OnUIThread(() => children.Add(error));
                    }
                    else
                    {
                        OnUIThread(() => children.Clear());

                        foreach (var child in antecedent.Result)
                        {
                            OnUIThread(() => children.Add(child));

                            child.AttachToParent(this);
                        }

                        wasRefreshed = true;
                    }

                    this.ExecutingBackgroundAction = false;
                }
            });
        }

        protected abstract Task<IEnumerable<CloudItem>> UpdateChildren();

        private static void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
        }

        private void AttachToParent(CloudItem parentItem)
        {
            this.parent = parentItem;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
