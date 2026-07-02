using FWITD;
using FWShellWPF.Windows;
using System.Windows;

namespace FWShellWPF {

    public partial class MainWindow : Window {
        private const int id_webview = (int)IDWebviews.MainWindow;
        public MainWindow() {
            InitializeComponent();
            if (App.RequestedStartApp.ToString().ToLower().Contains("android")) {
                Width = 340;
                Height = 600;
            }
            Loaded += OnLoaded;
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