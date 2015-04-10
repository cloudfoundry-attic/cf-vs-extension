using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
