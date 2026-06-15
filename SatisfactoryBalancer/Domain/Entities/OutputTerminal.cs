using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Entities;

public class OutputTerminal : FlowNode
{
    public OutputTerminal(NodeId id) : base(id) { }

    public override int InputCount => 1;
    public override int OutputCount => 0;
    public override string Label => "OUTPUT";

    public override IReadOnlyList<FlowFraction> Process(IReadOnlyList<FlowFraction> inputs) => inputs;
}
