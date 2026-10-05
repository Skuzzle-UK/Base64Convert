# Builds Base64 Convert and installs it for the current Windows user.
# Run again to update. Uninstall from Settings > Apps > Installed apps.
#
#   powershell -ExecutionPolicy Bypass -File install.ps1

$ErrorActionPreference = 'Stop'

$project    = Join-Path $PSScriptRoot 'Base64Convert\Base64Convert.csproj'
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Base64Convert'
$exe        = Join-Path $installDir 'Base64Convert.exe'
$shortcut   = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\Base64 Convert.lnk'
$regKey     = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Base64Convert'
$tfm        = 'net10.0-windows10.0.19041.0'

# Close the app if it's running, so its files can be replaced.
Get-Process Base64Convert -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -eq $exe } |
    Stop-Process -Force

Write-Host 'Building...'
dotnet publish $project -f $tfm "-p:TargetFrameworks=$tfm" -c Release -r win-x64 --self-contained `
    -p:WindowsAppSDKSelfContained=true -o $installDir -v q -nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

# Start menu shortcut.
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
$link.TargetPath = $exe
$link.WorkingDirectory = $installDir
$link.IconLocation = "$exe,0"
$link.Save()

# Uninstaller, run from Settings > Apps.
$uninstaller = Join-Path $installDir 'uninstall.ps1'
@"
Set-Location `$env:TEMP
Get-Process Base64Convert -ErrorAction SilentlyContinue | Where-Object { `$_.Path -eq '$exe' } | Stop-Process -Force
Remove-Item -LiteralPath '$shortcut' -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath '$regKey' -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath '$installDir' -Recurse -Force
"@ | Set-Content $uninstaller -Encoding UTF8

# Register in Installed apps.
[xml]$csproj = Get-Content $project
$version = ($csproj.Project.PropertyGroup.ApplicationDisplayVersion | Where-Object { $_ }) | Select-Object -First 1
$sizeKb = [int]((Get-ChildItem $installDir -Recurse | Measure-Object Length -Sum).Sum / 1KB)

New-Item -Path $regKey -Force | Out-Null
$values = @{
    DisplayName     = 'Base64 Convert'
    DisplayVersion  = "$version"
    Publisher       = 'Skuzzle'
    DisplayIcon     = $exe
    InstallLocation = $installDir
    UninstallString = "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$uninstaller`""
}
foreach ($name in $values.Keys) { Set-ItemProperty -Path $regKey -Name $name -Value $values[$name] }
Set-ItemProperty -Path $regKey -Name EstimatedSize -Value $sizeKb -Type DWord
Set-ItemProperty -Path $regKey -Name NoModify -Value 1 -Type DWord
Set-ItemProperty -Path $regKey -Name NoRepair -Value 1 -Type DWord

Write-Host "Installed Base64 Convert $version to $installDir"
