using HP.CloudFoundry.UI.VisualStudio.Forms;
using HP.CloudFoundry.UI.VisualStudio.Model;
using HP.CloudFoundry.UI.VisualStudio.TargetStore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            ReloadTargets();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void treeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem == null)
            {
                return;
            }

            var cloudItemAction = menuItem.Tag as CloudItemAction;

            if (cloudItemAction != null)
            {
                cloudItemAction.Click.Invoke().ContinueWith((antecedent) =>
                {
                    if (antecedent.IsFaulted)
                    {
                        var errorMessages = new List<string>();
                        ErrorFormatter.FormatExceptionMessage(antecedent.Exception, errorMessages);
                        MessageBoxHelper.DisplayError(string.Join(Environment.NewLine, errorMessages));
                    }
                    else
                    {

                    }
                });
            }
        }

        private void ExplorerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CloudItem item = e.NewValue as CloudItem;
            if (item != null)
            {
                PropertyGrid.SelectedObject = item;
            }
        }

        private void AddTargetButton_Click(object sender, RoutedEventArgs e)
        {
            using (var loginForm = new LoginWizardForm())
            {
                var result = loginForm.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var target = loginForm.CloudTarget;

                    if (target != null)
                    {
                        CloudTargetManager.SaveTarget(target);
                        ReloadTargets();
                    }
                }
            }
        }


        private void ReloadTargets()
        {
            this.ExplorerTree.Items.Clear();

            var targets = CloudTargetManager.GetTargets();

            foreach (var target in targets)
            {
                ExplorerTree.Items.Add(new CloudFoundryTarget(target));
            }
        }
    }
}