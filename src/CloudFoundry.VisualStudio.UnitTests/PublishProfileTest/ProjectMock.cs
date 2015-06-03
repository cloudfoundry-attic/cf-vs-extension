using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.UnitTests.PublishProfileTest
{
    class ProjectMock : Project
    {
        private string projectDir;

        public ProjectMock(string projectDir)
        {
            this.projectDir = projectDir;
        }

        public CodeModel CodeModel
        {
            get { throw new NotImplementedException(); }
        }

        public Projects Collection
        {
            get { throw new NotImplementedException(); }
        }

        public ConfigurationManager ConfigurationManager
        {
            get { throw new NotImplementedException(); }
        }

        public DTE DTE
        {
            get { throw new NotImplementedException(); }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public string ExtenderCATID
        {
            get { throw new NotImplementedException(); }
        }

        public dynamic ExtenderNames
        {
            get { throw new NotImplementedException(); }
        }

        public string FileName
        {
            get { return "test.csproj"; }
        }

        public string FullName
        {
            get { return Path.Combine(this.projectDir, this.FileName); }
        }

        public Globals Globals
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDirty
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Kind
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                return "foobar-project";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public new dynamic Object
        {
            get { throw new NotImplementedException(); }
        }

        public ProjectItem ParentProjectItem
        {
            get { throw new NotImplementedException(); }
        }

        public ProjectItems ProjectItems
        {
            get { throw new NotImplementedException(); }
        }

        public Properties Properties
        {
            get { throw new NotImplementedException(); }
        }

        public void Save(string FileName = "")
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string NewFileName)
        {
            throw new NotImplementedException();
        }

        public bool Saved
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string UniqueName
        {
            get { throw new NotImplementedException(); }
        }

        public dynamic get_Extender(string ExtenderName)
        {
            throw new NotImplementedException();
        }
    }
}
