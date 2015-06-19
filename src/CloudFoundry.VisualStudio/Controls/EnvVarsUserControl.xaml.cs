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
            //var profile = (this.DataContext as PublishProfile);

            //if (!profile.CFAppManifest.EnvironmentVars.ContainsKey(tbEnvVarKey.Text))
            //{
            //    profile.CFAppManifest.EnvironmentVars.Add(tbEnvVarKey.Text, tbEnvVarValue.Text);

            //    tbEnvVarValue.Clear();
            //    tbEnvVarKey.Clear();
            //    dgEnvVars.Items.Refresh();
            //}
            //else
            //{
            //    MessageBoxHelper.DisplayError("An environment variable with the same key has already been added!");
            //}
        }


        private void btnRemoveEnvVar_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in lvEnvVars.SelectedItems)
            {
                //var envVar = item as KeyValuePair<string, string>;
            }
            //try
            //{
            //    KeyValuePair<string, string> selectedEnvVar = (KeyValuePair<string, string>)dgEnvVars.SelectedItem;
            //    var dataContext = (this.DataContext as PublishProfileEditorResources);
            //    if (dataContext != null)
            //    {
            //        dataContext.PublishProfile.Application.EnvironmentVariables.Remove(selectedEnvVar.Key);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBoxHelper.DisplayError(ex);
            //}
            //lvEnvVars.Items.Refresh();
        }

    }
}
