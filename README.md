# Satisfactory Belt Balancer

Gerador de redes de **splitters e mergers** para o jogo [Satisfactory](https://www.satisfactorygame.com/).

Dado N entradas e M saídas, calcula automaticamente a topologia mínima de divisores e mescladores para distribuir o fluxo de forma perfeitamente igual entre todas as saídas.

![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows&logoColor=white)
![Linux](https://img.shields.io/badge/Linux-Fedora%20%7C%20Ubuntu%20%7C%20Arch-FCC624?logo=linux&logoColor=black)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![License MIT](https://img.shields.io/badge/license-MIT-green)

![screenshot](assets/screenshot.png)

## Como funciona

O algoritmo usa **Expand-and-Merge**:

1. Encontra o menor K = múltiplo de LCM(N, M) tal que K/N seja da forma 2ᵃ×3ᵇ
2. Constrói uma **árvore de split** recursiva (apenas divisores 1→2 e 1→3)
3. Constrói uma **árvore de merge** que agrupa os K fluxos em M saídas

> Alguns pares (N, M) são impossíveis — por exemplo, 17→5 —  
> quando LCM/N tem fatores primos diferentes de 2 e 3. O programa avisa.

## Instalação

### Windows 10 / 11

**Dependência:** [.NET SDK 8](https://dotnet.microsoft.com/download)

**Instalar** (PowerShell):
```powershell
git clone https://github.com/Vascon11/satisfactory-balancer.git
cd satisfactory-balancer
.\install.ps1
```

Procure por **"Satisfactory Balancer"** no Menu Iniciar.

---

### Linux (Fedora / Ubuntu / Arch)

**Dependência:**
```bash
# Fedora
sudo dnf install dotnet-sdk-8.0

# Ubuntu / Debian
sudo apt install dotnet-sdk-8.0

# Arch
sudo pacman -S dotnet-sdk
```

**Instalar:**
```bash
git clone https://github.com/Vascon11/satisfactory-balancer.git
cd satisfactory-balancer
./install.sh
```

Procure por **"Satisfactory Balancer"** no menu de aplicativos do GNOME.

---

### Executar sem instalar (qualquer OS)

```bash
dotnet run --project SatisfactoryBalancer.Avalonia/SatisfactoryBalancer.Avalonia.csproj
```

## Tecnologias

- **C# 12 / .NET 8** — 100% C#, sem HTML ou JavaScript
- **Avalonia UI 12** — UI desktop nativa (Linux, Windows, macOS)
- **DDD** — domínio isolado em `SatisfactoryBalancer/Domain/`

## Estrutura

```
SatisfactoryBalancer/          ← Domain library (algoritmos, value objects)
  Domain/
    Algorithms/                ← LcmExpander, SplitTreeBuilder, MergeTreeBuilder
    Services/                  ← BalancerGenerator
    ValueObjects/              ← FlowFraction (aritmética racional exata)
    Aggregates/                ← BalancerNetwork

SatisfactoryBalancer.Avalonia/ ← App desktop (Avalonia UI)
  Controls/BalancerCanvas.cs   ← Canvas customizado com pan e zoom
  Services/NetworkLayout.cs    ← Layout BFS + heurística baricêntrica
  MainWindow.axaml             ← Janela principal
```
