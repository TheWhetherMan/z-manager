using System.Windows.Controls;
using Z_Manager.Managers;

namespace Z_Manager.Controls
{
    public partial class NetworkControl : UserControl
    {
        public NetworkControl()
        {
            InitializeComponent();

            Loaded += NetworkControl_Loaded;
            Unloaded += NetworkControl_Unloaded;
        }

        private void NetworkControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO
        }

        private void NetworkControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }
    }
}
