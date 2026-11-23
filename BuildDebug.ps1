# Builds FWShellWPF in Debug configuration (output: bin\Debug\net10.0-windows\)
#
# Uses the default `dotnet` SDK, same as Visual Studio. Restore walks every TargetFramework of the referenced
# QStorage project (incl. net10.0-android/ios), so that SDK must have the MAUI workloads installed, otherwise
# it fails with NETSDK1147 (fix: `dotnet workload restore`, or install them via the Visual Studio installer).
$ErrorActionPreference = 'Stop'

Write-Host "Using .NET SDK $(dotnet --version)" -ForegroundColor Cyan

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

# Clean build: delete the whole bin folder (after closing instances above, so its files aren't locked)
$binDir = "$PSScriptRoot\bin"
if (Test-Path $binDir) {
    try {
        Remove-Item -LiteralPath $binDir -Recurse -Force -ErrorAction Stop
        Write-Host "Deleted $binDir" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Can't delete ${binDir}: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

dotnet build "$PSScriptRoot\FWShellWPF.csproj" -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build succeeded: $PSScriptRoot\bin\Debug\net10.0-windows\" -ForegroundColor Green
