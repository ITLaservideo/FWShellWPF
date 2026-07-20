# FWShellWPF

WPF host shell for [FWITD](FWITD/readme.md)-based Web-In-The-Desktop apps: a WebView2 window that loads one of several JS/HTML "apps" depending on how the process is launched.

## How it works

- Which app loads is a `StartApp` value (`FWITD/AppConfig.cs`), picked by the `--start-app <id>` command-line argument, or `App.RequestedStartApp`'s `AppSettings` default (`appsettings.json`) when launched with no arguments.
- `App.OnStartup` (`App.xaml.cs`) resolves the requested `StartApp` against `AppConfig._scripts` and shows `MainWindow` and, for apps that define one, a topmost `ExtraWindow` alongside it.
- Frameless dashboard apps (`ServerStatus`, `DashboardLettoreBarcode`) are borderless/non-resizable and persist their position and size per `StartApp` via `AppSettings` (`Windows/MainWindow.xaml.cs`).
- The `CreateStartAppShortcut` build target (`FWShellWPF.csproj`) generates a `.lnk` per app id into the output folder after every build, each launching the exe with its own `--start-app` argument.

## Layout

| Path | Purpose |
|---|---|
| `Windows/` | `MainWindow`, `ExtraWindow`, `OAuthPopupWindow`, `IDWebviews` |
| `Assets/DBUpdate/` | This host's own numbered SQL migrations, embedded and applied on startup |
| `FWITD/` | Shared git submodule — frontend, native/JS bridge, shared services (see its own [readme](FWITD/readme.md)) |
| `appsettings.json` | Per-app configuration (URLs, feature settings, etc.) |

## Building & running

Open `FWShellWPF.slnx` in Visual Studio, or:

```powershell
dotnet build
dotnet run --start-app 12   # e.g. StartApp.ServerStatus
```

With no arguments, the app defaults to whatever `App.RequestedStartApp` / `AppSettings` resolve to.

## Notes on the FWITD integration

This project follows FWITD's own documented WPF setup (see [FWITD/readme.md § Project setup](FWITD/readme.md#project-setup)) as-is — `ClientApp` assets embedded rather than copied as loose `Content`, and SQL migrations kept in this project's own `Assets/DBUpdate/` rather than inside the submodule. Both of those started out as FWShellWPF-specific deviations before being upstreamed into FWITD's own docs as the standard WPF convention, so `FWShellWPF.csproj`'s comments now mirror FWITD's readme rather than calling anything out as unusual.
