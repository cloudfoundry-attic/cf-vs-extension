using CloudFoundry.VisualStudio.Forms;
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
    /// Interaction logic for EnvVarsUserControl.xaml
    /// </summary>
    public partial class EnvVarsUserControl : UserControl
    {
        public EnvVarsUserControl()
        {
            InitializeComponent();
        }

        private void btnAddEnvVar_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (this.DataContext as PublishProfileEditorResources);

            if (dataContext == null)
            {
                throw new InvalidOperationException("DataContext is not a valid PublishProfileEditorResources");
            }

            dataContext.PublishProfile.Application.EnvironmentVariables[tbEnvVarKey.Text] = tbEnvVarValue.Text;

            tbEnvVarValue.Clear();
            tbEnvVarKey.Clear();
            lvEnvVars.Items.Refresh();
        }


        private void btnRemoveEnvVar_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (this.DataContext as PublishProfileEditorResources);

            if (dataContext == null)
            {
                throw new InvalidOperationException("DataContext is not a valid PublishProfileEditorResources");
            }

            foreach (var item in lvEnvVars.SelectedItems)
            {
                var envVar = (KeyValuePair<string, string>)item;
                dataContext.PublishProfile.Application.EnvironmentVariables.Remove(envVar.Key);
            }
            lvEnvVars.Items.Refresh();
        }

        private void btnEditEnvVar_Click(object sender, RoutedEventArgs e)
        {
            if (lvEnvVars.SelectedItem != null)
            {
                var envVar = (KeyValuePair<string, string>)lvEnvVars.SelectedItem;
                tbEnvVarKey.Text = envVar.Key;
                tbEnvVarValue.Text = envVar.Value;
            }
        }

    }
}
