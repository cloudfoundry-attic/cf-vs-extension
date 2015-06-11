namespace CloudFoundry.VisualStudio.MSBuild
{
    using EnvDTE;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MSBuildLogger : ILogger
    {
        private LoggerVerbosity verbosity = LoggerVerbosity.Normal;
        private OutputWindowPane outputPane;
        private ErrorListProvider errorPane;

        public MSBuildLogger(OutputWindowPane outputPane, ErrorListProvider errorPane)
        {
            this.outputPane = outputPane;
            this.errorPane = errorPane;
        }

        public void Initialize(IEventSource eventSource)
        {
            if (eventSource != null)
            {
                eventSource.MessageRaised += eventSource_MessageRaised;
                eventSource.ErrorRaised += eventSource_ErrorRaised;
            }
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            errorPane.Tasks.Add(new ErrorTask()
            {
                Category = TaskCategory.BuildCompile,
                ErrorCategory = TaskErrorCategory.Error,
                Column = e.ColumnNumber,
                Line = e.LineNumber,
                Text = e.Message,
                Document = e.ProjectFile
            });
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            outputPane.OutputString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", e.Message, Environment.NewLine));
        }

        public string Parameters
        {
            get;
            set;
        }

        public void Shutdown()
        {
        }

        public LoggerVerbosity Verbosity
        {
            get
            {
                return this.verbosity;
            }
            set
            {
                this.verbosity = value;
            }
        }
    }
}
