// -----------------------------------------------------------------------
// <copyright file="MessageBoxHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace CloudFoundry.VisualStudio.Forms
{
    using System;
    using System.Windows;
    using Microsoft.VisualStudio.Shell;

    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class MessageBoxHelper
    {
        public static MessageBoxResult ErrorWithMessageBox(string message)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult ErrorWithMessageBox(Exception ex)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(ex.Message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult ErrorWithMessageBox(string message, Exception ex)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(message + "\r\n\r\n" + ex.Message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult DisplayError(string message)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult DisplayError(string message, Exception ex)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(message + "\r\n\r\n" + ex.ToString(), Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult DisplayError(Exception ex)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(ex.ToString(), Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult DisplayInfo(string message)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
            {
                return Task.FromResult(MessageBox.Show(message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None));
            });
        }

        public static MessageBoxResult DisplayWarning(string message)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
                {
                    return Task.FromResult(MessageBox.Show(message, Logger.EventSource, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None));
                });
        }

        public static MessageBoxResult WarningQuestion(string message)
        {
            return ThreadHelper.JoinableTaskFactory.Run<MessageBoxResult>(() =>
           {
               return Task.FromResult(MessageBox.Show(message, Logger.EventSource, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None));
           });
        }
    }
}
