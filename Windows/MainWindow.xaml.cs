using FWITD;
using FWShellWPF.Windows;
using Microsoft.Web.WebView2.Core;
using System.IO;
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
                    string script = await JSProvider.getScriptApp(injectableApp, id_webview);
                    await InjectScriptAsync(WebView.CoreWebView2, script);
                };
            }

            if (entry.main.url is not null) {
                WebView.CoreWebView2.Navigate(entry.main.url);
            } else {
                MessageBox.Show($"No URL configured for app: {App.RequestedStartApp}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static async Task InjectScriptAsync(CoreWebView2 webView, string script) {
            string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(script));
            const int chunkSize = 4_500_000;//4.5 MB

            await webView.ExecuteScriptAsync("window.__fw_chunks=[];");
            for (int i = 0; i < b64.Length; i += chunkSize) {
                string chunk = b64.Substring(i, Math.Min(chunkSize, b64.Length - i));
                await webView.ExecuteScriptAsync($"window.__fw_chunks.push('{chunk}');");
            }
            await webView.ExecuteScriptAsync(
                "try{eval(new TextDecoder().decode(Uint8Array.from(atob(window.__fw_chunks.join('')),c=>c.charCodeAt(0))))}finally{delete window.__fw_chunks}");
        }
    }
}