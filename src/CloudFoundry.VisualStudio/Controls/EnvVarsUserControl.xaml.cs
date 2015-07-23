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
    using CloudFoundry.VisualStudio.Forms;
    using CloudFoundry.VisualStudio.ProjectPush;

    /// <summary>
    /// Interaction logic for EnvVarsUserControl.xaml
    /// </summary>
    public partial class EnvironmentVariablesUserControl : UserControl
    {
        public EnvironmentVariablesUserControl()
        {
            this.InitializeComponent();
        }

        private void BtnAddEnvVar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbEnvVarKey.Text) == false)
            {
                var dataContext = this.DataContext as PublishProfileEditorResources;

                if (dataContext == null)
                {
                    throw new InvalidOperationException("Data Context is not a valid Publish Profile Editor Resources");
                }

                dataContext.SelectedPublishProfile.Application.EnvironmentVariables[tbEnvVarKey.Text] = tbEnvVarValue.Text;

                tbEnvVarValue.Clear();
                tbEnvVarKey.Clear();
                lvEnvVars.Items.Refresh();
            }
        }

        private void BtnRemoveEnvVar_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as PublishProfileEditorResources;

            if (dataContext == null)
            {
                throw new InvalidOperationException("Data Context is not a valid Publish Profile Editor Resources");
            }

            foreach (var item in lvEnvVars.SelectedItems)
            {
                var envVar = (KeyValuePair<string, string>)item;
                dataContext.SelectedPublishProfile.Application.EnvironmentVariables.Remove(envVar.Key);
            }

            lvEnvVars.Items.Refresh();
        }

        private void BtnEditEnvVar_Click(object sender, RoutedEventArgs e)
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
