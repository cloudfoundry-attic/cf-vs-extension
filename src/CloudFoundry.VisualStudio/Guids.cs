// Guids.cs
// MUST match guids.h
using System;

namespace CloudFoundry.VisualStudio
{
    static class GuidList
    {
        public const string guidHP_CloudFoundry_UI_VisualStudioPkgString = "0e96d39c-9e00-4758-ad87-aeb5a9bf8f7c";
        public const string guidHP_CloudFoundry_UI_VisualStudioCmdSetString = "0771f788-56bf-431d-ac6f-ae8057b022a0";
        public const string guidToolWindowPersistanceString = "df775e32-5655-486d-a7c3-a2c69ca39c1f";

        public static readonly Guid guidHP_CloudFoundry_UI_VisualStudioCmdSet = new Guid(guidHP_CloudFoundry_UI_VisualStudioCmdSetString);
    };
}