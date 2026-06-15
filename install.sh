#!/usr/bin/env bash
set -e

# ── Satisfactory Balancer – Instalador para Fedora/Linux ──────────────────────

APP_DIR="$HOME/.local/share/satisfactory-balancer"
ICON_DIR="$HOME/.local/share/icons/hicolor/scalable/apps"
DESKTOP_DIR="$HOME/.local/share/applications"

echo "==> Verificando dependência: .NET SDK 8..."
if ! command -v dotnet &>/dev/null; then
    echo ""
    echo "  .NET SDK não encontrado. Instale com:"
    echo "    sudo dnf install dotnet-sdk-8.0"
    echo ""
    exit 1
fi

echo "==> Publicando binário (self-contained)..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet publish "$SCRIPT_DIR/SatisfactoryBalancer.Avalonia/SatisfactoryBalancer.Avalonia.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$APP_DIR" \
    --nologo -v quiet

chmod +x "$APP_DIR/SatisfactoryBalancer.Avalonia"
echo "   Binário instalado em: $APP_DIR"

echo "==> Instalando ícone..."
mkdir -p "$ICON_DIR"
cp "$SCRIPT_DIR/assets/satisfactory-balancer.svg" "$ICON_DIR/satisfactory-balancer.svg"

echo "==> Criando entrada no menu GNOME..."
mkdir -p "$DESKTOP_DIR"
cat > "$DESKTOP_DIR/satisfactory-balancer.desktop" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=Satisfactory Balancer
GenericName=Belt Balancer Generator
Comment=Gerador de redes de splitters e mergers para Satisfactory
Exec=$APP_DIR/SatisfactoryBalancer.Avalonia
Icon=satisfactory-balancer
Terminal=false
Categories=Game;Utility;
Keywords=satisfactory;belt;balancer;splitter;merger;
StartupWMClass=SatisfactoryBalancer.Avalonia
EOF

echo "==> Atualizando cache do GNOME..."
update-desktop-database "$DESKTOP_DIR" 2>/dev/null || true
gtk-update-icon-cache -f -t "$HOME/.local/share/icons/hicolor/" 2>/dev/null || true

echo ""
echo "✓ Instalado com sucesso!"
echo "  Procure por 'Satisfactory Balancer' no menu de aplicativos."
echo "  Para executar direto: $APP_DIR/SatisfactoryBalancer.Avalonia"
