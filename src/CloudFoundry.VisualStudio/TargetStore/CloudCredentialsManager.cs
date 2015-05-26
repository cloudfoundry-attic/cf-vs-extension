namespace CloudFoundry.VisualStudio.TargetStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Simple.CredentialManager;

    internal class CloudCredentialsManager
    {
        public static void Save(Uri targetUri, string username, string password)
        {
            using (Credential creds = new Credential())
            {
                creds.Target = GetTargetString(targetUri, username);
                creds.Username = username;
                creds.Password = password;
                creds.PersistenceType = PersistenceType.LocalComputer;
                creds.Save();
            }
        }

        public static void Delete(Uri targetUri, string username)
        {
            using (Credential creds = new Credential())
            {
                creds.Target = GetTargetString(targetUri, username);
                creds.PersistenceType = PersistenceType.LocalComputer;

                if (creds.Exists())
                {
                    creds.Delete();
                }
            }
        }

        public static string GetPassword(Uri targetUri, string username)
        {
            using (Credential creds = new Credential())
            {
                creds.Target = GetTargetString(targetUri, username);
                creds.PersistenceType = PersistenceType.LocalComputer;

                if (creds.Exists())
                {
                    creds.Load();
                    return creds.Password;
                }
                else
                {
                    return null;
                }
            }
        }

        private static string GetTargetString(Uri targetUri, string username)
        {
            UriBuilder uriBuilder = new UriBuilder(targetUri);
            uriBuilder.UserName = HttpUtility.UrlEncode(username);
            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}
