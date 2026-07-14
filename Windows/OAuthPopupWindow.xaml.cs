using System.Windows;

namespace FWShellWPF {

    /// <summary>
    /// Transient WebView2 window used to run an OAuth provider's hosted login page.
    /// Not registered with <see cref="FWITD.RequestDispatcher"/> — it only ever
    /// navigates to external provider URLs, never to our own JS bridge pages.
    /// </summary>
    public partial class OAuthPopupWindow : Window {
        private readonly TaskCompletionSource<string> _redirectTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenSource _closedCts = new();

        /// <summary>Cancelled the moment the user closes the popup (e.g. to abandon a device-code poll loop).</summary>
        public CancellationToken ClosedToken => _closedCts.Token;

        public OAuthPopupWindow(string title) {
            InitializeComponent();
            Title = title;
            Topmost = true;
            Closed += (_, _) => {
                _redirectTcs.TrySetCanceled();
                _closedCts.Cancel();
            };
        }

        /// <summary>
        /// Navigates the popup to <paramref name="url"/>. When <paramref name="redirectUriPrefix"/> is
        /// supplied, any navigation whose URI starts with it is intercepted (before the network request
        /// fires) and surfaced via <see cref="WaitForRedirectAsync"/> instead of actually being loaded —
        /// the prefix never needs to resolve to a real page.
        /// </summary>
        public async Task NavigateAsync(string url, string? redirectUriPrefix = null) {
            await PopupWebView.EnsureCoreWebView2Async();
            if (redirectUriPrefix != null) {
                PopupWebView.CoreWebView2.NavigationStarting += (_, args) => {
                    if (args.Uri.StartsWith(redirectUriPrefix, StringComparison.OrdinalIgnoreCase)) {
                        args.Cancel = true;
                        _redirectTcs.TrySetResult(args.Uri);
                    }
                };
            }
            PopupWebView.CoreWebView2.Navigate(url);
        }

        /// <summary>Resolves with the intercepted redirect URL. Throws <see cref="TaskCanceledException"/> if the user closes the popup first.</summary>
        public Task<string> WaitForRedirectAsync() => _redirectTcs.Task;
    }
}
