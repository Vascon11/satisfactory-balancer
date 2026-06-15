using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Entities;

public class Splitter : FlowNode
{
    private readonly int _outputs;

    public Splitter(NodeId id, int outputs) : base(id)
    {
        if (outputs is not 2 and not 3)
            throw new ArgumentException("Splitter só suporta 2 ou 3 saídas.");
        _outputs = outputs;
    }

    public override int InputCount => 1;
    public override int OutputCount => _outputs;
    public override string Label => $"Split1→{_outputs}";

    public override IReadOnlyList<FlowFraction> Process(IReadOnlyList<FlowFraction> inputs)
    {
        if (inputs.Count != 1)
            throw new InvalidOperationException("Splitter requer exatamente 1 entrada.");

        var part = inputs[0].SplitBy(_outputs);
        return Enumerable.Repeat(part, _outputs).ToList();
    }
}
