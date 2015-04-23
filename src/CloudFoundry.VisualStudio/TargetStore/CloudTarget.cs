using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace CloudFoundry.VisualStudio.TargetStore
{
    public class CloudTarget
    {
        private static readonly string[] V2ApiTags = new string[] { "APIv2" };

        private string email;
        private string token;
        private Uri targetUrl;
        private Guid targetId;
        private string description;
        private bool ignoreSSLErrors;
        private string version;

        private CloudTarget()
        {
        }

        public static CloudTarget CreateV2Target(string token, Uri targetUri, string description, string email, bool ignoreSSLErrors, string version)
        {
            return new CloudTarget()
            {
                targetId = Guid.NewGuid(),
                token = token,
                targetUrl = targetUri,
                description = description,
                email = email,
                ignoreSSLErrors = ignoreSSLErrors,
                version = version
            };
        }

        public static CloudTarget FromRegistryText(KeyValuePair<string, string[]> target)
        {
            string[] registryText = target.Value;
            if (registryText == null || registryText.Length < 1)
            {
                throw new ArgumentException("Invalid registry setting.", "registryText");
            }

            string apiTypeTag = registryText[0];

            if (CloudTarget.V2ApiTags.Contains(apiTypeTag))
            {

                Uri targetUrl = new Uri(registryText[1]);
                string token = registryText[2];
                string description = registryText[3];
                string email = registryText[4];

                bool ignoreSSLErrors = Convert.ToBoolean(registryText[5]);
                string version = registryText[6];

                CloudTarget registryTarget = CloudTarget.CreateV2Target(token, targetUrl, description, email, ignoreSSLErrors, version);

                registryTarget.TargetId = Guid.Parse(target.Key);
                return registryTarget;
            }
            else
            {
                throw new InvalidOperationException("Unknown registry target.");
            }
        }

        public string[] ToRegistryText()
        {

            return new string[] {
                CloudTarget.V2ApiTags[0], 
                this.TargetUrl.OriginalString,
                this.Token,
                this.Description,
                this.Email,
                this.ignoreSSLErrors.ToString(),
                this.version
            };
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public string Token
        {
            get
            {
                return token;
            }
        }

        public string Version
        {
            get
            {
                return version;
            }
        }
        public string Email
        {
            get { return email; }
        }


        public Uri TargetUrl
        {
            get { return targetUrl; }
        }

        public Guid TargetId
        {
            get { return targetId; }
            set { targetId = value; }
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} ({1})", description.Length == 0 ? TargetUrl.Host : description, email);
            }
        }

        public bool IgnoreSSLErrors
        {
            get
            {
                return this.ignoreSSLErrors;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }
    }
}
