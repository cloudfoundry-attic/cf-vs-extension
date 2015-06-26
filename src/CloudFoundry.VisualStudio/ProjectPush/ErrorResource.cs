using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    internal class ErrorResource : INotifyPropertyChanged
    {
        private bool hasErrors = false;
        private string errorMessage = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasErrors
        {
            get
            {
                return hasErrors;
            }
            set
            {
                hasErrors = value;
                this.RaisePropertyChangedEvent("HasErrors");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
                this.RaisePropertyChangedEvent("ErrorMessage");
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
    }
}
