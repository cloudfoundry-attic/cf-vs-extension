using System;

namespace HP.CloudFoundry.UI.VisualStudio.Model
{
    class ErrorEventArgs : EventArgs
    {
        public Exception Error
        {
            get;
            set;
        }
    }
}
