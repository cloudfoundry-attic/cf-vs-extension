namespace CloudFoundry.VisualStudio.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio.Shell;

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

        public string Parameters
        {
            get;
            set;
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

        public void Initialize(IEventSource eventSource)
        {
            if (eventSource != null)
            {
                eventSource.MessageRaised += this.EventSource_MessageRaised;
                eventSource.ErrorRaised += this.EventSource_ErrorRaised;
            }
        }

        public void Shutdown()
        {
        }

        private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            this.errorPane.Tasks.Add(new ErrorTask()
            {
                Category = TaskCategory.BuildCompile,
                ErrorCategory = TaskErrorCategory.Error,
                Column = e.ColumnNumber,
                Line = e.LineNumber,
                Text = e.Message,
                Document = e.ProjectFile
            });
        }

        private void EventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            this.outputPane.OutputString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", e.Message, Environment.NewLine));
        }
    }
}
