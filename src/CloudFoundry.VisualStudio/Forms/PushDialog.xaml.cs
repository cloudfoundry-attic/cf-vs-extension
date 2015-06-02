using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.VisualStudio.ProjectPush;
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
    /// Interaction logic for PushDialog.xaml
    /// </summary>
    public partial class PushDialog : Window
    {

        public PushDialog(PublishProfile package, EnvDTE.Project currentProject)
        {
            //package.CFAppManifest.NoRoute
            this.DataContext = package;
            InitializeComponent();


        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await (this.DataContext as PublishProfile).InitiCFClient();
        }




    }
}
