using System;

namespace CloudFoundry.VisualStudio.Model
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
