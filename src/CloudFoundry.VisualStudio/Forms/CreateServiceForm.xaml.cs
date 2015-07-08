using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.VisualStudio.ProjectPush;
using Microsoft.VisualStudio.PlatformUI;
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

namespace CloudFoundry.VisualStudio.Forms
{
    /// <summary>
    /// Interaction logic for CreateService.xaml
    /// </summary>
    public partial class CreateServiceForm : DialogWindow
    {
        private CloudFoundryClient client;
        private Guid spaceGuid;
        public CreateServiceForm(CloudFoundryClient cfclient, Guid workingSpaceGuid)
        {
            this.client = cfclient;
            this.spaceGuid = workingSpaceGuid;
            InitializeComponent();
            this.DataContext = new ServiceInstanceEditorResource(client);

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
                viewModel.Refreshing = true;
                viewModel.RefreshMessage = string.Format(CultureInfo.InvariantCulture, "Creating service {0} ...", tbServiceName.Text);
                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();
                request.Name = this.tbServiceName.Text;
                request.ServicePlanGuid = viewModel.SelectedServicePlan.ToGuid();
                request.SpaceGuid = this.spaceGuid;

                await client.ServiceInstances.CreateServiceInstance(request);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                viewModel.Refreshing = false;
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
