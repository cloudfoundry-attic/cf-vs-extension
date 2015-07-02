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
    /// Interaction logic for AppSettingsUserControl.xaml
    /// </summary>
    public partial class AppSettingsUserControl : UserControl
    {
        public AppSettingsUserControl()
        {
            InitializeComponent();
        }

        private void cbBuildpack_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbBuildpack.Text.Length > 0)
            {
                tbBuildpackWatermark.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                tbBuildpackWatermark.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
