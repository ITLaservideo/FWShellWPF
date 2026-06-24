using DotNet.Utility;
using FWITD;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FWShellWPF {

    public partial class App : Application {
        public static StartApp RequestedStartApp { get; private set; } = AppSettings.Get<StartApp>("App.RequestedStartApp", StartApp.Dashboard);

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var args = e.Args;
            for (int i = 0; i < args.Length - 1; i++) {
                if (args[i] == "--start-app" && int.TryParse(args[i + 1], out int value) && Enum.IsDefined(typeof(StartApp), value)) {
                    RequestedStartApp = (StartApp)value;
                    break;
                }
            }
            SQL.Init();
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
    }
}
