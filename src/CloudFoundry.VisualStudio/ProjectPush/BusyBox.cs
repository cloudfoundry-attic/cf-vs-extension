using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    public class BusyBox : NotificationObject
    {
        private bool isBusy = false;
        private string busyMessage;

        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string BusyMessage
        {
            get { return busyMessage; }
            set { busyMessage = value; RaisePropertyChanged(() => BusyMessage); }
        }

        public void SetMessage(string message)
        {
            this.IsBusy = true;
            this.BusyMessage = message;
        }


    }
}
