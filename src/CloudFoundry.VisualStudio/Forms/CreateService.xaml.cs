using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
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
    public partial class CreateService : Window
    {
        private CloudFoundryClient client;
        private Guid spaceGuid;
        public CreateService(CloudFoundryClient cfclient, Guid workingSpaceGuid)
        {
            this.client = cfclient;
            this.spaceGuid = workingSpaceGuid;
            InitializeComponent();
            FillServiceTypes();
        }

        private async void FillServiceTypes()
        {
            this.IsEnabled = false;
            try
            {
                var services = await client.Services.ListAllServices();

                ServiceType.ItemsSource = services;
                ServiceType.SelectedValuePath = "EntityMetadata.Guid";
                ServiceType.DisplayMemberPath = "Label";
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayError("Error retrieving service types ", ex);
                Logger.Error("Error retrieving service types ", ex);
            }

            this.IsEnabled = true;
        }



        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();
                request.Name = ServiceName.Text;
                request.ServicePlanGuid = new Guid(ServicePlan.SelectedValue.ToString());
                request.SpaceGuid = this.spaceGuid;

                await client.ServiceInstances.CreateServiceInstance(request);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayError("Error creating service instance ", ex);
                Logger.Error("Error creating service instance ", ex);
            }

            this.IsEnabled = true;
        }

        private async void ServiceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                Guid serviceGuid = new Guid(ServiceType.SelectedValue.ToString());

                var servicePlanList = await client.ServicePlans.ListAllServicePlans();

                var servicePlans = servicePlanList.Where(o => o.ServiceGuid == serviceGuid);

                ServicePlan.ItemsSource = servicePlans;

                ServicePlan.DisplayMemberPath = "Name";
                ServicePlan.SelectedValuePath = "EntityMetadata.Guid";
            }
            catch (Exception ex)
            {
                MessageBoxHelper.DisplayError("Error retrieving plans for selected service type ", ex);
                Logger.Error("Error retrieving plans for selected service type ", ex);
            }

            this.IsEnabled = true;
        }
    }
}
