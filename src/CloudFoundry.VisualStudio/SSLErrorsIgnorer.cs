namespace CloudFoundry.VisualStudio
{
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    internal class SSLErrorsIgnorer
    {
        private static bool ignore;

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
                    ServicePointManager.ServerCertificateValidationCallback = null;
                }
            }
        }

        private static bool InternalCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
