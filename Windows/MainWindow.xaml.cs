using FWITD;
using FWShellWPF.Windows;
using QStorage;
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
            if (App.RequestedStartApp == StartApp.ServerStatus || App.RequestedStartApp == StartApp.DashboardLettoreBarcode) {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Width = AppSettings.Get($"{App.RequestedStartApp}.Width", 500.0);
                Height = AppSettings.Get($"{App.RequestedStartApp}.Height", 500.0);
                Top = AppSettings.Get($"{App.RequestedStartApp}.Top", 200.0);
                Left = AppSettings.Get($"{App.RequestedStartApp}.Left", 200.0);
                DragHandle.Visibility = Visibility.Visible;
            }
            Loaded += OnLoaded;
        }
        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) {
                DragMove();
                AppSettings.Set($"{App.RequestedStartApp}.Left", Left);
                AppSettings.Set($"{App.RequestedStartApp}.Top", Top);
            }
        }

        private const double MinResizeWidth = 100;
        private const double MinResizeHeight = 100;
        private Point _resizeStartPoint;
        private double _resizeStartWidth;
        private double _resizeStartLeft;
        private double _resizeStartHeight;
        private double _resizeStartTop;
        private bool _resizingFromLeft;

        private void ResizeLeftHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            StartResize(sender, e, resizingFromLeft: true);
        }

        private void ResizeRightHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            StartResize(sender, e, resizingFromLeft: false);
        }

        private void StartResize(object sender, MouseButtonEventArgs e, bool resizingFromLeft) {
            _resizingFromLeft = resizingFromLeft;
            _resizeStartPoint = PointToScreen(e.GetPosition(this));
            _resizeStartWidth = Width;
            _resizeStartLeft = Left;
            _resizeStartHeight = Height;
            _resizeStartTop = Top;

            var handle = (UIElement)sender;
            handle.MouseMove += ResizeHandle_MouseMove;
            handle.MouseLeftButtonUp += ResizeHandle_MouseLeftButtonUp;
            handle.CaptureMouse();
        }
        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var current = PointToScreen(e.GetPosition(this));
            double deltaX = current.X - _resizeStartPoint.X;
            double deltaY = current.Y - _resizeStartPoint.Y;

            if (_resizingFromLeft) {
                double newWidth = _resizeStartWidth - deltaX;
                if (newWidth >= MinResizeWidth) {
                    Width = newWidth;
                    Left = _resizeStartLeft + deltaX;
                }
            } else {
                double newWidth = _resizeStartWidth + deltaX;
                if (newWidth >= MinResizeWidth) {
                    Width = newWidth;
                }
            }

            // The handle sits on the top edge, so dragging up grows the window,
            // anchoring the resize at the bottom edge.
            double newHeight = _resizeStartHeight - deltaY;
            if (newHeight >= MinResizeHeight) {
                Height = newHeight;
                Top = _resizeStartTop + deltaY;
            }
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var handle = (UIElement)sender;
            handle.ReleaseMouseCapture();
            handle.MouseMove -= ResizeHandle_MouseMove;
            handle.MouseLeftButtonUp -= ResizeHandle_MouseLeftButtonUp;

            AppSettings.Set($"{App.RequestedStartApp}.Width", Width);
            AppSettings.Set($"{App.RequestedStartApp}.Height", Height);
            AppSettings.Set($"{App.RequestedStartApp}.Left", Left);
            AppSettings.Set($"{App.RequestedStartApp}.Top", Top);
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