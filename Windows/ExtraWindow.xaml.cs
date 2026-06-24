using FWITD;
using System.Windows;
using System.Windows.Input;
using FWShellWPF.Windows;

namespace FWShellWPF {

    public partial class ExtraWindow : Window {
        private const int id_webview = (int)IDWebviews.ExtraWindow;
        public ExtraWindow() {
            InitializeComponent();
            Width = AppSettings.Get($"GlobalPreferences.GoogleDocs.Width", 250);
            Height = AppSettings.Get($"GlobalPreferences.GoogleDocs.Height", 350);
            Top = SystemParameters.PrimaryScreenHeight - Height - 100;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) {
            await WebView.EnsureCoreWebView2Async();
            RequestDispatcher.Register(WebView, id_webview);

            if (!AppConfig._scripts.TryGetValue(App.RequestedStartApp, out var entry)) {
                MessageBox.Show($"No configuration found for app: {App.RequestedStartApp}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (entry.extra.script is JSProvider.JS.pages page) {
                var basePath = await JSProvider.getPathJSHTMLApp(page, id_webview);
                WebView.CoreWebView2.Navigate(new Uri(basePath + ".html").AbsoluteUri);
                return;
            }

            if (entry.extra.script is JSProvider.JS.injectable_apps injectableApp) {
                WebView.CoreWebView2.NavigationCompleted += async (_, _) => {
                    await WebView.CoreWebView2.ExecuteScriptAsync(await JSProvider.getScriptApp(injectableApp, id_webview));
                };
            }

            if (entry.extra.url is not null) {
                WebView.CoreWebView2.Navigate(entry.extra.url);
            } else {
                MessageBox.Show($"No URL configured for app: {App.RequestedStartApp}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
