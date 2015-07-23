namespace CloudFoundry.VisualStudio.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
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
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.VisualStudio.ProjectPush;
    using Microsoft.VisualStudio.PlatformUI;

    /// <summary>
    /// Interaction logic for CreateService.xaml
    /// </summary>
    public partial class CreateServiceForm : DialogWindow
    {
        private CloudFoundryClient client;
        private Guid spaceGuid;

        public CreateServiceForm(CloudFoundryClient client, Guid workingSpace)
        {
            this.client = client;
            this.spaceGuid = workingSpace;
            this.InitializeComponent();
            this.DataContext = new ServiceInstanceEditorResource(client);
            this.InfoMessage.Text = string.Empty;
            this.InfoSpinner.Visibility = System.Windows.Visibility.Hidden;
        }

        private async void Wizard_Finish(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ServiceInstanceEditorResource;

            if (viewModel == null)
            {
                throw new InvalidOperationException("Invalid DataContext");
            }

            try
            {
                viewModel.AllowFinish = false;
                this.InfoMessage.Text = "Please wait while creating service...";
                this.InfoSpinner.Visibility = System.Windows.Visibility.Visible;
                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();
                request.Name = this.tbServiceName.Text;
                request.ServicePlanGuid = viewModel.SelectedServicePlan.ToGuid();
                request.SpaceGuid = this.spaceGuid;

                await this.client.ServiceInstances.CreateServiceInstance(request);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                viewModel.AllowFinish = true;
                this.InfoSpinner.Visibility = System.Windows.Visibility.Hidden;
                this.InfoMessage.Text = string.Empty;
                var errorMessages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(ex, errorMessages);
                viewModel.Error.HasErrors = true;
                viewModel.Error.ErrorMessage = string.Join(Environment.NewLine, errorMessages.ToArray());
                Logger.Error("Error creating service instance ", ex);
            }
        }

        private void Wizard_Cancel(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBoxHelper.WarningQuestion("Do you really want to cancel ?");
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
