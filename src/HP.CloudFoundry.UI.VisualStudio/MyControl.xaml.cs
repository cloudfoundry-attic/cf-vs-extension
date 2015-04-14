using HP.CloudFoundry.UI.VisualStudio.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HP.CloudFoundry.UI.VisualStudio
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        public MyControl()
        {
            InitializeComponent();

            SSLErrorsIgnorer.Ignore = true;

            ExplorerTree.Items.Add(new CloudFoundryTarget(
                "Private Tenant",
                new Uri("https://cloud.hopto.me"),
                "admin",
                "password1234!",
                false));

            ExplorerTree.Items.Add(new CloudFoundryTarget(
              "Pelerinul",
              new Uri("https://pelerinul.servebeer.com"),
              "admin",
              "password1234!",
              false));

            ExplorerTree.Items.Add(new CloudFoundryTarget(
                "Bad Credentials",
                new Uri("https://cloud.hopto.me"),
                "admin",
                "password1234",
                false));

            ExplorerTree.Items.Add(new CloudFoundryTarget(
                "Hack For Europe",
                new Uri("https://h4e.cf.helion-dev.com"),
                "gert.drapers@hp.com",
                "",
                false));


            ExplorerTree.Items.Add(new CloudFoundryTarget(
                "Hack For India",
                new Uri("https://gids.cf.helion-dev.com"),
                "gert.drapers@hp.com",
                "",
                false));
        }

        private void treeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cloudItemAction = ExplorerTree.SelectedItem as CloudItemAction;
            if (cloudItemAction != null)
            {
                Task clickTask = new Task(cloudItemAction.Click);
                clickTask.Start();
            }
        }

        private void ExplorerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CloudItem item = e.NewValue as CloudItem;
            if (item != null)
            {
                propertyGrid.SelectedObject = item;
                propertyGrid2.SelectedObject = item;
            }
        }
    }
}