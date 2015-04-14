using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HP.CloudFoundry.UI.VisualStudio
{
    internal class SSLErrorsIgnorer
    {

        private static bool _ignore;

        private static bool InternalCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool Ignore
        {
            get
            {
                return SSLErrorsIgnorer._ignore;
            }
            set
            {
                SSLErrorsIgnorer._ignore = value;

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
