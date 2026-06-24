using System.IO;
using System.Windows;
using FWITD;
using FWShellWPF.Windows;

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

            if (!AppConfig._scripts.TryGetValue(App.RequestedStartApp, out var entry)) {
                MessageBox.Show($"No configuration found for app: {App.RequestedStartApp}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (entry.main.script is JSProvider.JS.pages page) {
                var basePath = await JSProvider.getPathJSHTMLApp(page, id_webview);
                WebView.CoreWebView2.Navigate(new Uri(basePath + ".html").AbsoluteUri);
                return;
            }

            if (entry.main.script is JSProvider.JS.injectable_apps injectableApp) {
                WebView.CoreWebView2.NavigationCompleted += async (_, _) => {
                    await WebView.CoreWebView2.ExecuteScriptAsync(await JSProvider.getScriptApp(injectableApp, id_webview));
                };
            }

            if (entry.main.url is not null) {
                WebView.CoreWebView2.Navigate(entry.main.url);
            } else {
                MessageBox.Show($"No URL configured for app: {App.RequestedStartApp}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}