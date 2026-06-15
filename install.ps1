# Satisfactory Balancer – Instalador para Windows
param()
$ErrorActionPreference = "Stop"

$AppDir     = "$env:LOCALAPPDATA\SatisfactoryBalancer"
$ExePath    = "$AppDir\SatisfactoryBalancer.Avalonia.exe"
$ShortcutDir = [Environment]::GetFolderPath("StartMenu") + "\Programs"
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "==> Verificando dependência: .NET SDK 8..."
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "  .NET SDK não encontrado. Baixe em: https://dotnet.microsoft.com/download"
    Write-Host ""
    exit 1
}

Write-Host "==> Publicando binário (self-contained)..."
dotnet publish "$ScriptDir\SatisfactoryBalancer.Avalonia\SatisfactoryBalancer.Avalonia.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "$AppDir" `
    --nologo -v quiet

Write-Host "   Binário instalado em: $AppDir"

Write-Host "==> Criando atalho no Menu Iniciar..."
$Shell    = New-Object -ComObject WScript.Shell
$Shortcut = $Shell.CreateShortcut("$ShortcutDir\Satisfactory Balancer.lnk")
$Shortcut.TargetPath       = $ExePath
$Shortcut.WorkingDirectory = $AppDir
$Shortcut.Description      = "Gerador de redes de splitters e mergers para Satisfactory"
$Shortcut.Save()

Write-Host ""
Write-Host "Instalado com sucesso!"
Write-Host "  Procure por 'Satisfactory Balancer' no Menu Iniciar."
Write-Host "  Para executar direto: $ExePath"
