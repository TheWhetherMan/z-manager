using System.Windows.Controls;

namespace Z_Manager.Controls
{
    public partial class MinecraftServerControl : UserControl
    {
        public MinecraftServerControl()
        {
            InitializeComponent();

            Loaded += MinecraftServerControl_Loaded;
        }

        private void MinecraftServerControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void StartStopServerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
