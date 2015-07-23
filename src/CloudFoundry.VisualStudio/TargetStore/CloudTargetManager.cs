namespace CloudFoundry.VisualStudio.TargetStore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public static class CloudTargetManager
    {
        private const string CompanyKey = "CloudFoundry";
        private const string ProductKey = "VisualStudio";
        private const string TargetsKey = "Targets";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions for detailed logging purposes.")]
        public static CloudTarget[] GetTargets()
        {
            Dictionary<string, string[]> targets = GetValuesFromCloudTargets();

            List<CloudTarget> result = new List<CloudTarget>(targets.Count);
            
            foreach (KeyValuePair<string, string[]> target in targets)
            {
                try
                {
                    Guid targetId = new Guid(target.Key);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format(CultureInfo.InvariantCulture, "There is an invalid key in the registry: {0}", target.Key), ex);
                    continue;
                }

                try
                {
                    var cloudTarget = CloudTarget.FromRegistryText(target);
                    result.Add(cloudTarget);
                }
                catch (Exception ex)
                {
                    Logger.Warning(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Error while loading target: {0}. It will be ignored. Details: {1}",
                            target.Key,
                            ex.ToString()));
                }
            }

            return result.ToArray();
        }

        public static void SaveTarget(CloudTarget target)
        {
            if (target == null)
            { 
                return;
            }
                
            if (GetValuesFromCloudTargets().ContainsKey(target.TargetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID already exists in the collection!");
            }

            CloudTargetManager.AddValueToCloudTargets(
                target.TargetId.ToString(),
                target.ToRegistryText());
        }

        public static void RemoveTarget(CloudTarget target)
        {
            if (target != null)
            {
                RemoveTarget(target.TargetId);
            }
        }

        public static void RemoveTarget(Guid targetId)
        {
            if (!GetValuesFromCloudTargets().ContainsKey(targetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID does not exist!");
            }

            RemoveValueFromCloudTargets(targetId.ToString());
        }

        private static RegistryKey SetupTargetsKey()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey companySubKey = softwareKey.OpenSubKey(CompanyKey, true);
            if (companySubKey == null)
            {
                softwareKey.CreateSubKey(CompanyKey);
                companySubKey = softwareKey.OpenSubKey(CompanyKey, true);
            }

            RegistryKey productSubKey = companySubKey.OpenSubKey(ProductKey, true);
            if (productSubKey == null)
            {
                companySubKey.CreateSubKey(ProductKey);
                productSubKey = companySubKey.OpenSubKey(ProductKey, true);
            }

            RegistryKey targetsSubKey = productSubKey.OpenSubKey(TargetsKey, true);
            if (targetsSubKey == null)
            {
                productSubKey.CreateSubKey(TargetsKey);
                targetsSubKey = productSubKey.OpenSubKey(TargetsKey, true);
            }

            return targetsSubKey;
        }

        private static void AddValueToCloudTargets(string valueName, string[] value)
        {
            RegistryKey cloudTargets = SetupTargetsKey();
            cloudTargets.SetValue(valueName, value, RegistryValueKind.MultiString);
        }

        private static void RemoveValueFromCloudTargets(string valueName)
        {
            RegistryKey cloudTargets = SetupTargetsKey();
            cloudTargets.DeleteValue(valueName);
        }

        private static Dictionary<string, string[]> GetValuesFromCloudTargets()
        {
            RegistryKey cloudTargets = SetupTargetsKey();
            string[] allValues = cloudTargets.GetValueNames();
            Dictionary<string, string[]> result = new Dictionary<string, string[]>(allValues.Length);
            foreach (string value in allValues)
            {
                string[] strings = (string[])cloudTargets.GetValue(value, null);
                if (strings != null)
                {
                    result.Add(value, strings);
                }
            }

            return result;
        }
    }
}
