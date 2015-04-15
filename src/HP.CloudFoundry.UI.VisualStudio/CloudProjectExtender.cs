using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EnvDTE;
using HP.CloudFoundry.UI.VisualStudio.Model;
//using HP.CloudFoundry.UI.Packaging;

namespace HP.CloudFoundry.UI.VisualStudio
{
    [CLSCompliant(false)]
    public class CloudProjectExtenderProvider : EnvDTE.IExtenderProvider
    {

        public static readonly string[] ProjectTypesToExtend = new string[]
        {
            "{4EF9F003-DE95-4d60-96B0-212979F2A857}",    //VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpProjectBrowseObject
            "{E0FDC879-C32A-4751-A3D3-0B3824BD575F}",    //VSLangProj.PrjBrowseObjectCATID.prjCATIDVBProjectBrowseObject
            "{EEF81A81-D390-4725-B16D-E103E0F967B4}",    //VsWebSite.PrjBrowseObjectCATID.prjCATIDWebSiteProjectBrowseObject
        };

        private static string dynamicExtenderName = "HPCloudProjectPropertiesExtender";


        public static string DynamicExtenderName
        {
            get
            {
                return dynamicExtenderName;
            }
        }

        public bool CanExtend(string ExtenderCATID, string ExtenderName, object ExtendeeObject)
        {
            
            System.ComponentModel.PropertyDescriptor extendeeCATIDProp = TypeDescriptor.GetProperties(ExtendeeObject)["ExtenderCATID"];

            bool IfCanExtend = ExtenderName == dynamicExtenderName &&
                 ProjectTypesToExtend.Any(row => row.ToLower() == ExtenderCATID.ToLower()) &&
                 extendeeCATIDProp != null &&
                 ProjectTypesToExtend.Any(row => row.ToLower() == extendeeCATIDProp.GetValue(ExtendeeObject).ToString().ToLower());

            return IfCanExtend;
        }

        Dictionary<Project, Object> dic = new Dictionary<Project, object>();

        public object GetExtender(string ExtenderCATID, string ExtenderName, object ExtendeeObject, EnvDTE.IExtenderSite ExtenderSite, int Cookie)
        {
            
            AppPackage dynamicExtender = null;

            EnvDTE.Project proj = null;

            proj = ExtendeeObject as EnvDTE.Project;
            if (proj == null)
            {
                object[] selectedProjects = (object[])((EnvDTE.DTE)ExtenderSite.GetObject("")).ActiveSolutionProjects;

                if (selectedProjects.Length == 1)
                {
                    proj = (Project)selectedProjects[0];
                }
            }

            if (proj != null && dic.ContainsKey(proj))
            {
                return dic[proj];
            }

            if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject) && proj != null)
            {
                dynamicExtender = new AppPackage();
                dynamicExtender.Initialize(proj);
                dic[proj] = dynamicExtender;
            }

            return dynamicExtender;
        }
    }
}