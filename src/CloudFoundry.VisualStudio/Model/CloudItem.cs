namespace CloudFoundry.VisualStudio.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using CloudFoundry.CloudController.V2.Client;
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
        private bool isEnabled = true;

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
                    this.cloudItemType != CloudItemType.AppFile &&
                    this.cloudItemType != CloudItemType.Route &&
                    this.cloudItemType != CloudItemType.Service &&
                    this.cloudItemType != CloudItemType.Error;
            }
        }

        protected bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }

            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged("IsEnabled");
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

        protected static HashSet<CloudItem> GetNodesWithConnection(CloudItem cloudItem, EntityGuid id)
        {
            HashSet<CloudItem> result = new HashSet<CloudItem>();

            Type thisType = cloudItem.GetType();
            var fields = thisType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (field.FieldType.Assembly.FullName.StartsWith("CloudFoundry.CloudController", StringComparison.OrdinalIgnoreCase))
                {
                    var value = field.GetValue(cloudItem);
                    var collection = value as IEnumerable;
                    if (collection != null)
                    {
                        foreach (var obj in collection)
                        {
                            if (ValueIsConnected(obj, id))
                            {
                                result.Add(cloudItem);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (ValueIsConnected(value, id))
                        {
                            result.Add(cloudItem);
                            break;
                        }
                    }
                }
            }

            foreach (var child in cloudItem.children)
            {
                foreach (var node in GetNodesWithConnection(child, id))
                {
                    result.Add(node);
                }
            }

            return result;
        }

        protected static CloudItem GetRootItem(CloudItem ci)
        {
            if (ci.parent == null)
            {
                return ci;
            }
            else
            {
                return GetRootItem(ci.parent);
            }
        }

        protected abstract Task<IEnumerable<CloudItem>> UpdateChildren();

        protected void EnableNodes(EntityGuid entityID, bool enabled)
        {
            var rootItem = GetRootItem(this);
            var nodesWithConnection = GetNodesWithConnection(rootItem, entityID);

            foreach (var node in nodesWithConnection)
            {
                node.IsEnabled = enabled;
            }
        }

        private static void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
        }

        private static bool ValueIsConnected(object obj, EntityGuid id)
        {
            var entityMetadataProperty = obj.GetType().GetProperty("EntityMetadata");
            if (entityMetadataProperty != null)
            {
                var meta = entityMetadataProperty.GetValue(obj) as Metadata;
                if (meta != null)
                {
                    if ((Guid?)meta.Guid == (Guid?)id)
                    {
                        return true;
                    }
                }
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj);

                if (propertyValue != null)
                {
                    var guidValue = propertyValue as Guid?;
                    if (guidValue != null)
                    {
                        if ((Guid?)guidValue == (Guid?)id)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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
