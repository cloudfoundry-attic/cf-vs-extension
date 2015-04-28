using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace CloudFoundry.VisualStudio.TargetStore
{
    public class CloudTarget
    {
        private enum CloudTargetPart
        {
            TypeTag = 0,
            TargetUrl = 1,
            Description = 2,
            Email = 3,
            IgnoreSSLErrors = 4,
            Version = 5
        }

        private const int CloudTargetPartsCount = 6;
        
        // Increase the * part in v2-* any time there's a breaking change
        private static readonly string[] V2ApiTags = new string[] { "v2-1" };

        private string email;
        private Uri targetUrl;
        private Guid targetId;
        private string description;
        private bool ignoreSSLErrors;
        private string version;

        private CloudTarget()
        {
        }

        public static CloudTarget CreateV2Target(Uri targetUri, string description, string email, bool ignoreSSLErrors, string version)
        {
            return new CloudTarget()
            {
                targetId = Guid.NewGuid(),
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

            string apiTypeTag = registryText[(int)CloudTargetPart.TypeTag];

            if (CloudTarget.V2ApiTags.Contains(apiTypeTag))
            {

                Uri targetUrl = new Uri(registryText[(int)CloudTargetPart.TargetUrl]);
                string description = registryText[(int)CloudTargetPart.Description];
                string email = registryText[(int)CloudTargetPart.Email];
                bool ignoreSSLErrors = Convert.ToBoolean((int)CloudTargetPart.IgnoreSSLErrors);
                string version = registryText[(int)CloudTargetPart.Version];

                CloudTarget registryTarget = CloudTarget.CreateV2Target(targetUrl, description, email, ignoreSSLErrors, version);

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
            string[] result = new string[CloudTargetPartsCount];
            result[(int)CloudTargetPart.TypeTag] = CloudTarget.V2ApiTags[0];
            result[(int)CloudTargetPart.TargetUrl] = this.TargetUrl.OriginalString;
            result[(int)CloudTargetPart.Description] = this.Description;
            result[(int)CloudTargetPart.Email] = this.Email;
            result[(int)CloudTargetPart.IgnoreSSLErrors] = this.ignoreSSLErrors.ToString();
            result[(int)CloudTargetPart.Version] = this.version;

            return result;
        }

        public override string ToString()
        {
            return this.DisplayName;
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
