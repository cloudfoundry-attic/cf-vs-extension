using Simple.CredentialManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CloudFoundry.VisualStudio.TargetStore
{
    class CloudCredentialsManager
    {
        static class CloudCredentials
        {
            private static string GetTargetString(Uri targetUri, string username)
            {
                UriBuilder uriBuilder = new UriBuilder(targetUri);
                uriBuilder.UserName = HttpUtility.UrlEncode(username);
                return uriBuilder.Uri.AbsoluteUri;
            }

            public static void Save(Uri targetUri, string username, string password)
            {
                Credential creds = new Credential();
                creds.Target = GetTargetString(targetUri, username);
                creds.Username = username;
                creds.Password = password;
                creds.PersistenceType = PersistenceType.LocalComputer;
                creds.Save();
            }

            public static void Delete(Uri targetUri, string username)
            {
                Credential creds = new Credential();
                creds.Target = GetTargetString(targetUri, username);
                creds.PersistenceType = PersistenceType.LocalComputer;

                if (creds.Exists())
                {
                    creds.Delete();
                }
            }

            public static string GetPassword(Uri targetUri, string username)
            {
                Credential creds = new Credential();
                creds.Target = GetTargetString(targetUri, username);
                creds.PersistenceType = PersistenceType.LocalComputer;

                if (creds.Exists())
                {
                    creds.Load();
                    return creds.Password;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
