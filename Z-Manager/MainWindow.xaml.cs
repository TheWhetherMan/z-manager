using System.Windows.Threading;
using System.Windows.Input;
using Z_Manager.Managers;
using System.Windows;
using System;

namespace Z_Manager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoggingManager.LogMessage("Z-Manager starting...");
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoggingManager.LogMessage("MainWindow.MainWindow_Loaded");
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (WindowStyle != WindowStyle.None)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (DispatcherOperationCallback)delegate (object obj)
                {
                    WindowStyle = WindowStyle.None;
                    return null;
                }, null);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void FileMenuButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            LoggingManager.LogMessage("MainWindow.MinimizeButton_Click");
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Minimized;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoggingManager.LogMessage("MainWindow.ExitButton_Click");
            Application.Current.Shutdown();
        }
    }
}
