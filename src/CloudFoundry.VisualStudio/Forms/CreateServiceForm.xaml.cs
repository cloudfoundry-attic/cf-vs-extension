using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.VisualStudio.ProjectPush;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
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

        private void ServiceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsEnabled = false;

            var viewModel = this.DataContext as ServiceInstanceEditorResource;
          
            if (viewModel == null)
            {
                throw new InvalidOperationException("Invalid DataContext");
            }

            try
            {
                Guid serviceGuid = new Guid(cbServiceType.SelectedValue.ToString());

                var servicePlans = viewModel.ServicePlans.Where(o => o.ServiceGuid == serviceGuid);

                this.cbServicePlan.ItemsSource = servicePlans;
            }
            catch (Exception ex)
            {
                var errorMessages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(ex, errorMessages);
                viewModel.Error.ErrorMessage = string.Join(Environment.NewLine, errorMessages.ToArray());
                viewModel.Error.HasErrors = true;
                Logger.Error("Error retrieving plans for selected service type ", ex);
            }

            this.IsEnabled = true;
        }

        private async void Wizard_Finish(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            var viewModel = this.DataContext as ServiceInstanceEditorResource;

            if (viewModel == null)
            {
                throw new InvalidOperationException("Invalid DataContext");
            }

            try
            {
                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();
                request.Name = this.tbServiceName.Text;
                request.ServicePlanGuid = new Guid(this.cbServicePlan.SelectedValue.ToString());
                request.SpaceGuid = this.spaceGuid;

                await client.ServiceInstances.CreateServiceInstance(request);

                this.DialogResult = true;

                this.Close();
            }
            catch (Exception ex)
            {
                var errorMessages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(ex, errorMessages);
                viewModel.Error.ErrorMessage = string.Join(Environment.NewLine, errorMessages.ToArray());
                viewModel.Error.HasErrors = true;
                Logger.Error("Error creating service instance ", ex);
            }

            this.IsEnabled = true;
        }
    }
}
