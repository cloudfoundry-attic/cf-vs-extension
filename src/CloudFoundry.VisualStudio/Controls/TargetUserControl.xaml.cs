using CloudFoundry.VisualStudio.Forms;
using CloudFoundry.VisualStudio.ProjectPush;
using CloudFoundry.VisualStudio.TargetStore;
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

        private void ButtonSetTarget_Click(object sender, RoutedEventArgs e)
        {
            using (var loginForm = new LoginWizardForm())
            {
                var result = loginForm.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var target = loginForm.CloudTarget;
                    this.cbTarget.SelectedValue = target;
                }
            }
        }
    }
}
