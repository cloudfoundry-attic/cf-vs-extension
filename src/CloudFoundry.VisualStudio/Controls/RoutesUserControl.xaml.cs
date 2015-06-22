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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloudFoundry.VisualStudio.Controls
{
    /// <summary>
    /// Interaction logic for RoutesUserControl.xaml
    /// </summary>
    public partial class RoutesUserControl : UserControl
    {
        public RoutesUserControl()
        {
            InitializeComponent();
        }

        private void btnAddHost_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (this.DataContext as PublishProfileEditorResources);

            if (dataContext == null)
            {
                throw new InvalidOperationException("DataContext is not a valid PublishProfileEditorResources");
            }
            dataContext.PublishProfile.Application.Hosts.Add(tbName.Text);

            lvRoutes.Items.Refresh();
            tbName.Clear();

        }

        private void btnDeleteHost_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (this.DataContext as PublishProfileEditorResources);

            if (dataContext == null)
            {
                throw new InvalidOperationException("DataContext is not a valid PublishProfileEditorResources");
            }

            foreach (var item in lvRoutes.SelectedItems)
            {
                string host = item as string;
                if (dataContext.PublishProfile.Application.Hosts.Contains(host))
                {
                    dataContext.PublishProfile.Application.Hosts.Remove(host);
                }
            }
            lvRoutes.Items.Refresh();
        }
    }
}
