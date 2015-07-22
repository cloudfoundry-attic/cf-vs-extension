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
    using CloudFoundry.VisualStudio.TargetStore;

    /// <summary>
    /// Interaction logic for TargetUserControl.xaml
    /// </summary>
    public partial class TargetUserControl : UserControl
    {
        public TargetUserControl()
        {
            this.InitializeComponent();
        }

        private void ButtonSetTarget_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as PublishProfileEditorResources;

            if (dataContext == null)
            {
                throw new InvalidOperationException("DataContext is not a valid PublishProfileEditorResources");
            }

            var loginForm = new LogOnForm();

            var result = loginForm.ShowDialog();

            if (result == true)
            {
                var target = loginForm.CloudTarget;
                this.cbTarget.SelectedValue = target;
            }
        }
    }
}
