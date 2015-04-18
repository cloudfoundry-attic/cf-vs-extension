using System;
using System.Collections.Generic;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;

namespace HP.CloudFoundry.UI.VisualStudio.TargetStore
{
    public static class CloudTargetManager
    {
        private const string hpApi = "isHPApi";
        private const string notHPApi = "notHPApi";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static CloudTarget[] GetTargets()
        {
            Dictionary<string, string[]> targets = GetValuesFromCloudTargets();

            List<CloudTarget> result = new List<CloudTarget>(targets.Count);
            
            foreach (KeyValuePair<string, string[]> target in targets)
            {
                Guid targetId = Guid.Empty;

                try
                {
                    targetId = new Guid(target.Key);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("There is an invalid key in the registry: {0}", target.Key), ex);
                    continue;
                }

                try
                {
                    var cloudTarget = CloudTarget.FromRegistryText(target);
                    result.Add(cloudTarget);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while loading targets: {0}", ex);
                    RemoveTarget(targetId);
                }
            }

            return result.ToArray();
        }

        public static void SaveTarget(CloudTarget target)
        {
            if (target == null)
                return;

            if (GetValuesFromCloudTargets().ContainsKey(target.TargetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID already exists in the collection!");
            }

            CloudTargetManager.AddValueToCloudTargets(
                    target.TargetId.ToString(),
                    target.ToRegistryText()
                );
        }

        public static void RemoveTarget(CloudTarget target)
        {
            RemoveTarget(target.TargetId);
        }

        public static void RemoveTarget(Guid targetId)
        {
            if (!GetValuesFromCloudTargets().ContainsKey(targetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID does not exist!");
            }

            RemoveValueFromCloudTargets(targetId.ToString());
        }

        private static RegistryKey SetupHPKey()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey hpSubKey = softwareKey.OpenSubKey("HP", true);
            if (hpSubKey == null)
            {
                softwareKey.CreateSubKey("HP");
                hpSubKey = softwareKey.OpenSubKey("HP", true);
            }

            RegistryKey hpCloudTargetsSubKey = hpSubKey.OpenSubKey("CloudTargets", true);
            if (hpCloudTargetsSubKey == null)
            {
                hpSubKey.CreateSubKey("CloudTargets");
                hpCloudTargetsSubKey = hpSubKey.OpenSubKey("CloudTargets", true);
            }
            return hpCloudTargetsSubKey;
        }

        private static void AddValueToCloudTargets(string valueName, string[] value)
        {
            RegistryKey cloudTargets = SetupHPKey();
            cloudTargets.SetValue(valueName, value, RegistryValueKind.MultiString);
        }

        private static void RemoveValueFromCloudTargets(string valueName)
        {
            RegistryKey cloudTargets = SetupHPKey();
            cloudTargets.DeleteValue(valueName);
        }

        private static Dictionary<string, string[]> GetValuesFromCloudTargets()
        {
            RegistryKey cloudTargets = SetupHPKey();
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
