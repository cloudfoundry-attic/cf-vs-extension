using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Threading;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    internal abstract class CloudItem : INotifyPropertyChanged
    {
        private readonly CloudItemType _cloudItemType = CloudItemType.Target;
        private volatile bool _isExpanded = false;
        private volatile bool _wasRefreshed = false;

        private readonly AsyncObservableCollection<CloudItem> _children = new AsyncObservableCollection<CloudItem>();
        private System.Threading.CancellationToken cancellationToken;

        protected bool HasRefresh
        {
            get
            {
                return _cloudItemType != CloudItemType.LoadingPlaceholder &&
                    _cloudItemType != CloudItemType.App &&
                    _cloudItemType != CloudItemType.Route &&
                    _cloudItemType != CloudItemType.Service &&
                    _cloudItemType != CloudItemType.Error;
            }
        }

        protected CloudItem(CloudItemType cloudItemType)
        {
            _cloudItemType = cloudItemType;

            if (this.HasRefresh)
            {
                _children.Add(new LoadingPlaceholder());
            }
        }

        [Browsable(false)]
        public CancellationToken CancellationToken
        {
            get
            {
                return cancellationToken;
            }
        }

        [Browsable(false)]
        public CloudItemType ItemType
        {
            get
            {
                return _cloudItemType;
            }
        }

        [Browsable(false)]
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;

                if (_isExpanded && !_wasRefreshed)
                {
                    RefreshChildren().Forget();

                }
            }
        }

        public async Task RefreshChildren()
        {
            this.ExecutingBackgroundAction = true;
            var populateChildrenTask = this.UpdateChildren();
            this.cancellationToken = new System.Threading.CancellationToken();

            await populateChildrenTask.ContinueWith((antecedent) =>
                {
                    if (antecedent.IsFaulted)
                    {
                        _children.Clear();

                        CloudError error = new CloudError(antecedent.Exception);

                        _children.Add(error);
                    }
                    else
                    {
                        _children.Clear();

                        foreach (var child in antecedent.Result)
                        {
                            _children.Add(child);
                        }

                        _wasRefreshed = true;
                    }

                    this.ExecutingBackgroundAction = false;
                });
        }

        [Browsable(false)]
        public BitmapImage Icon
        {
            get
            {
                return Converters.ImageConverter.ConvertBitmapToBitmapImage(IconBitmap);
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
                NotifyPropertyChanged("ExecutingBackgroundAction");
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
                return _children;
            }
        }

        [Browsable(false)]
        protected abstract IEnumerable<CloudItemAction> MenuActions
        {
            get;
        }

        [Browsable(false)]
        public ObservableCollection<CloudItemAction> Actions
        {
            get
            {
                ObservableCollection<CloudItemAction> result = new ObservableCollection<CloudItemAction>();

                if (this.HasRefresh)
                {
                    result.Add(new CloudItemAction(this, "Refresh", Resources.Refresh, RefreshChildren));
                    result.Add(new CloudItemAction(this, "-", null, () => { return Task.Delay(0); }));
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

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool executingBackgroundAction;
    }
}
