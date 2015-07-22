namespace CloudFoundry.VisualStudio.Controls
{
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

    /// <summary>
    /// Interaction logic for AppSettingsUserControl.xaml
    /// </summary>
    public partial class AppSettingsUserControl : UserControl
    {
        public AppSettingsUserControl()
        {
            this.InitializeComponent();
        }

        private void CbBuildpack_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(cbBuildpack.Text))
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
