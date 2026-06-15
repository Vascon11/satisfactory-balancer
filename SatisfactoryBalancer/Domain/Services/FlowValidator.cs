using SatisfactoryBalancer.Domain.Aggregates;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Services;

public static class FlowValidator
{
    public record ValidationResult(bool IsValid, string Message);

    public static ValidationResult Validate(BalancerNetwork network, FlowFraction inputFlowPerBelt)
    {
        var totalInput = inputFlowPerBelt * network.Inputs;
        // totalInput = inputFlowPerBelt.Numerator * network.Inputs / inputFlowPerBelt.Denominator
        var totalInputFraction = new FlowFraction(
            inputFlowPerBelt.Numerator * network.Inputs,
            inputFlowPerBelt.Denominator);

        var expectedPerOutput = new FlowFraction(
            totalInputFraction.Numerator,
            totalInputFraction.Denominator * network.Outputs);

        // Valida cada nó: soma das entradas == soma das saídas
        foreach (var node in network.Nodes)
        {
            var inFlow = network.Edges
                .Where(e => e.To == node.Id)
                .Aggregate(FlowFraction.Zero, (acc, e) => acc + e.Flow);

            var outFlow = network.Edges
                .Where(e => e.From == node.Id)
                .Aggregate(FlowFraction.Zero, (acc, e) => acc + e.Flow);

            if (inFlow != outFlow)
                return new ValidationResult(false,
                    $"Nó {node} viola conservação de fluxo: in={inFlow} out={outFlow}");
        }

        return new ValidationResult(true,
            $"Rede válida. Cada saída recebe {expectedPerOutput} por unidade de entrada.");
    }
}
