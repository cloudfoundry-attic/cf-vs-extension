namespace CloudFoundry.VisualStudio.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.VisualStudio.MSBuild;
    using CloudFoundry.VisualStudio.ProjectPush;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Threading;
    
    /// <summary>
    /// Interaction logic for PushDialog.xaml
    /// </summary>
    public partial class PushDialog : DialogWindow
    {
        private CancellationToken cancellationToken;
        private PublishProfileEditorResources publishProfileResources;

        public PushDialog(PublishProfile package)
        {
            this.cancellationToken = new CancellationToken();
            this.publishProfileResources = new PublishProfileEditorResources(package, this.cancellationToken);
            this.DataContext = this.publishProfileResources;

            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.publishProfileResources.Refresh(PublishProfileRefreshTarget.Client);
        }

        private void WizardPush_Finish(object sender, RoutedEventArgs e)
        {
            this.publishProfileResources.CleanManifest();
            this.publishProfileResources.SelectedPublishProfile.Save();

            var project = VsUtils.GetSelectedProject();

            if (project != null)
            {
                project.ProjectItems.AddFromFile(this.publishProfileResources.SelectedPublishProfile.Path);
                project.Save();
            }

            Dictionary<string, string> buildProperties = new Dictionary<string, string>();
            buildProperties.Add("DeployOnBuild", "true");
            buildProperties.Add("PublishProfile", this.publishProfileResources.SelectedPublishProfile.Path);

            MSBuildProcess.Publish(project, buildProperties);
        }

        private void WizardPush_Cancel(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBoxHelper.WarningQuestion("Do you really want to cancel ?");
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
