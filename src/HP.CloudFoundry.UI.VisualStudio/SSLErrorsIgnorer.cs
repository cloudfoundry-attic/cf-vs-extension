using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio
{
    internal class SSLErrorsIgnorer
    {

        private static bool ignore;

        private static bool InternalCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool Ignore
        {
            get
            {
                return SSLErrorsIgnorer.ignore;
            }
            set
            {
                SSLErrorsIgnorer.ignore = value;

                if (SSLErrorsIgnorer.Ignore)
                {
                    ServicePointManager.ServerCertificateValidationCallback += SSLErrorsIgnorer.InternalCallback;
                }
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback += SSLErrorsIgnorer.InternalCallback;
                }
            }
        }
    }
}
