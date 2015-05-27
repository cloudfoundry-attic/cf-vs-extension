namespace CloudFoundry.VisualStudio.Model
{
    using System;

    internal class ErrorEventArgs : EventArgs
    {
        public Exception Error
        {
            get;
            set;
        }
    }
}
