using FWITD;
using FWShellWPF.Windows;
using System.Windows;
using System.Windows.Input;

namespace FWShellWPF {

    public partial class MainWindow : Window {
        private const int id_webview = (int)IDWebviews.MainWindow;
        public MainWindow() {
            InitializeComponent();
            if (App.RequestedStartApp.ToString().ToLower().Contains("android")) {
                Width = 340;
                Height = 600;
            }
            if (App.RequestedStartApp == StartApp.ServerStatus) {
                //remove top bar, the app has it's own Application.exit
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Width = 500;
                Height = 500;
                Top = 200;
                Left = 200;
                DragHandle.Visibility = Visibility.Visible;
            }
            Loaded += OnLoaded;
        }
        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) {
                DragMove();
            }
        }
        private async void OnLoaded(object sender, RoutedEventArgs e) {
            await WebView.EnsureCoreWebView2Async();
            RequestDispatcher.Register(WebView, id_webview);

            try {
                await AppLoader.LoadAsync(id_webview, App.RequestedStartApp);
            } catch (InvalidOperationException ex) {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}