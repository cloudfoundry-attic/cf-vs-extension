using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace HP.CloudFoundry.UI.VisualStudio
{
    class Logger
    {
        /// <summary>
        /// The process that generates the log entry message
        /// </summary>
        public const string EventSource = "Cloud Foundry Helion Visual Studio Extension";

        private static void WriteToLog(string message, EventLogEntryType type, params object[] args)
        {
            Logger.WriteToLog(string.Format(
                CultureInfo.InvariantCulture,
                message,
                args
                ), type);
        }

        private static void WriteToLog(string message, EventLogEntryType type)
        {
            try
            {
                EventLog.WriteEntry(
                    EventSource,
                    message,
                    type);
            }
            catch // If we can't write to log, there's really nothing we can do, and we can't let Visual Studio crash
            { }
        }

        /// <summary>
        /// Logs an error message.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Error(string message)
        {
            Logger.WriteToLog(message, EventLogEntryType.Error);
        }

        /// <summary>
        /// /// Logs an error message and the source that produced the error.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="source">Enum item of the object that generates the log entry message.</param>
        public static void Error(string message, Exception ex)
        {
            Logger.WriteToLog("{0} - {1}", EventLogEntryType.Error, message, ex);
        }

        /// <summary>
        /// Logs a warning message.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Warning(string message)
        {
            Logger.WriteToLog(message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// Logs an information message.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Info(string message)
        {
            Logger.WriteToLog(message, EventLogEntryType.Information);
        }
    }
}
