# Builds FWShellWPF in Debug configuration (output: bin\Debug\net10.0-windows\)
#
# Runs on the newest installed .NET 10 SDK rather than whatever `dotnet` resolves by default: restore walks
# every TargetFramework of the referenced QStorage project (incl. net10.0-android/ios), so it needs an SDK
# that has the MAUI workloads installed — a newer preview/RC SDK without them fails with NETSDK1147.
$ErrorActionPreference = 'Stop'

$sdk = dotnet --list-sdks |
    ForEach-Object { if ($_ -match '^(10\.\d+\.\d+)\s+\[(.+)\]$') { [pscustomobject]@{ Version = [version]$Matches[1]; Path = Join-Path $Matches[2] $Matches[1] } } } |
    Sort-Object Version -Descending |
    Select-Object -First 1
if (-not $sdk) {
    Write-Host "No .NET 10 SDK found (dotnet --list-sdks)" -ForegroundColor Red
    exit 1
}
Write-Host "Using .NET SDK $($sdk.Version)" -ForegroundColor Cyan

# Force close running instances of the Debug build, otherwise the copy to the output folder fails on locked dlls.
# An instance whose path can't be read (e.g. started elevated) is assumed to be ours.
$outDir = "$PSScriptRoot\bin\Debug\"
$running = Get-Process -Name 'DevFWShellWPF' -ErrorAction SilentlyContinue |
    Where-Object { -not $_.Path -or $_.Path.StartsWith($outDir, [StringComparison]::OrdinalIgnoreCase) }
foreach ($p in $running) {
    try {
        Stop-Process -Id $p.Id -Force -ErrorAction Stop
        $p.WaitForExit(5000) | Out-Null
        Write-Host "Closed running instance (PID $($p.Id))" -ForegroundColor Yellow
    }
    catch {
        $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
        if (-not $isAdmin) {
            Write-Host "## not administrator can't kill running instances" -ForegroundColor Red
        }
        else {
            Write-Host "Can't close running instance (PID $($p.Id)): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

dotnet "$($sdk.Path)\dotnet.dll" build "$PSScriptRoot\FWShellWPF.csproj" -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build succeeded: $PSScriptRoot\bin\Debug\net10.0-windows\" -ForegroundColor Green
