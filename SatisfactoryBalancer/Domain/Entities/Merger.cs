using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Entities;

public class Merger : FlowNode
{
    private readonly int _inputs;

    public Merger(NodeId id, int inputs) : base(id)
    {
        if (inputs is not 2 and not 3)
            throw new ArgumentException("Merger só suporta 2 ou 3 entradas.");
        _inputs = inputs;
    }

    public override int InputCount => _inputs;
    public override int OutputCount => 1;
    public override string Label => $"Merge{_inputs}→1";

    public override IReadOnlyList<FlowFraction> Process(IReadOnlyList<FlowFraction> inputs)
    {
        if (inputs.Count != _inputs)
            throw new InvalidOperationException($"Merger requer exatamente {_inputs} entradas.");

        var sum = inputs.Aggregate(FlowFraction.Zero, (acc, f) => acc + f);
        return [sum];
    }
}
