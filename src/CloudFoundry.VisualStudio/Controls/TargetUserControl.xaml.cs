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
    /// Interaction logic for TargetUserControl.xaml
    /// </summary>
    public partial class TargetUserControl : UserControl
    {
        public TargetUserControl()
        {
            InitializeComponent();
        }

        private async void cbOrganization_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await (this.DataContext as PublishProfile).RefreshSpaces();
        }
    }
}
