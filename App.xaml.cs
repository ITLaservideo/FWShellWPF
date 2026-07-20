using FWITD;
using FWITD.Controllers;
using System.IO;
using System.Windows;

namespace FWShellWPF {

    public partial class App : Application {
        public static StartApp RequestedStartApp { get; private set; } = StartApp.Cloudflared;
        // public static StartApp RequestedStartApp { get; private set; } = AppSettings.Get<StartApp>("App.RequestedStartApp", StartApp.DashboardLayout1);

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var args = e.Args;
            for (int i = 0; i < args.Length - 1; i++) {
                if (args[i] == "--start-app" && int.TryParse(args[i + 1], out int value)) {
                    if (Enum.IsDefined(typeof(StartApp), value)) {
                        RequestedStartApp = (StartApp)value;
                    } else {
                        MessageBox.Show($"The requested app id '{value}' does not exist.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Shutdown();
                        return;
                    }
                    break;
                }
            }

            // "Open with" / double-clicking an associated file launches the exe with the file's path as an argument.
            foreach (var arg in args) {
                if (!arg.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || !File.Exists(arg))
                    continue;
                OpenPdf(arg);
                break;
            }
#if DEBUG
            Log.log($"APP:{RequestedStartApp.ToString()} Started {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", LogType.Info);
#endif
#if HASDATABASE
            SQL.Init();
#endif
            var main = new MainWindow();

            if (AppConfig._scripts.TryGetValue(RequestedStartApp, out var appConfig)) {
                if (appConfig.extra.script != null || appConfig.extra.url != null) {
                    var extra = new ExtraWindow { Topmost = true };
                    main.Closed += (_, _) => extra.Close();
                    extra.Closed += (_, _) => main.Close();
                    extra.Show();
                }
                if (appConfig.main.script != null || appConfig.main.url != null) {
                    main.Show();
                }
            } else {
                main.Show();
            }
        }

        /// <summary>
        /// Hands a PDF the OS asked this app to open over to the PDFEditor, which picks it up
        /// on startup via <see cref="OpenedFileController.TakePending"/>.
        /// </summary>
        private static void OpenPdf(string path) {
            // Opening a PDF always lands in the PDFEditor, overriding the default and --start-app,
            // even when reading the file fails (the editor then just starts empty).
            RequestedStartApp = StartApp.PDFEditor;
            try {
                OpenedFileController.SetPending(Path.GetFileName(path), File.ReadAllBytes(path));
            } catch (Exception ex) {
                Log.log($"Unable to open '{path}': {ex.Message}", LogType.Error);
            }
        }
    }
}
