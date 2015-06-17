using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.VisualStudio.ProjectPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.PlatformUI;

namespace CloudFoundry.VisualStudio.Forms
{
    /// <summary>
    /// Interaction logic for PushDialog.xaml
    /// </summary>
    public partial class PushDialog : DialogWindow
    {
        private CancellationToken cancellationToken;

        public PushDialog(PublishProfile package)
        {
            this.cancellationToken = new CancellationToken();
            var publishProfileResources = new PublishProfileEditorResources(package, this.cancellationToken);

            this.DataContext = publishProfileResources;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var publishResources = this.DataContext as PublishProfileEditorResources;

            if (publishResources == null)
            {
                return;
            }

            publishResources.Refresh(PublishProfileRefreshTarget.Client);
        }
    }
}
