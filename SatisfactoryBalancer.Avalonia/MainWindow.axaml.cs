using Avalonia.Controls;
using Avalonia.Interactivity;
using SatisfactoryBalancer.Domain.Algorithms;
using SatisfactoryBalancer.Domain.Services;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Generate();
    }

    private void OnGerar(object? sender, RoutedEventArgs e) => Generate();

    private void Generate()
    {
        ErrorBorder.IsVisible = false;
        StatsText.Text = "";

        int n = (int)(InputSpinner.Value ?? 1);
        int m = (int)(OutputSpinner.Value ?? 1);

        try
        {
            var network = BalancerGenerator.Generate(n, m);
            GraphCanvas.SetNetwork(network);

            var frac = new FlowFraction(n, m);
            StatsText.Text =
                $"IN {network.Inputs}  OUT {network.Outputs}  " +
                $"Split {network.SplitterCount}  Merge {network.MergerCount}  " +
                $"Fluxo/saída {frac} ({frac.ToDouble():F3})";
        }
        catch (InvalidOperationException ex)
        {
            ErrorText.Text = ex.Message;
            ErrorBorder.IsVisible = true;
            GraphCanvas.SetNetwork(null);
        }
    }
}
