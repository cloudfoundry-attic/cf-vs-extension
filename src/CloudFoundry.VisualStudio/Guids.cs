// Guids.cs
// MUST match guids.h

namespace CloudFoundry.VisualStudio
{
    using System;

    internal static class GuidList
    {
        public const string GuidCloudFoundryVisualStudioPkgString = "0e96d39c-9e00-4758-ad87-aeb5a9bf8f7c";
        public const string GuidCloudFoundryVisualStudioCmdSetString = "0771f788-56bf-431d-ac6f-ae8057b022a0";
        public const string GuidToolWindowPersistanceString = "df775e32-5655-486d-a7c3-a2c69ca39c1f";

        public static readonly Guid GuidCloudFoundryVisualStudioCmdSet = new Guid(GuidCloudFoundryVisualStudioCmdSetString);
    }
}