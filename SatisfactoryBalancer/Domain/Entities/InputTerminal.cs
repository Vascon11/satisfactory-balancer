using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Entities;

public class InputTerminal : FlowNode
{
    public InputTerminal(NodeId id) : base(id) { }

    public override int InputCount => 0;
    public override int OutputCount => 1;
    public override string Label => "INPUT";

    public override IReadOnlyList<FlowFraction> Process(IReadOnlyList<FlowFraction> inputs) =>
        [FlowFraction.One];
}
