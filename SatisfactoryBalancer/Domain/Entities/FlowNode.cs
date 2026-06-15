using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Entities;

public abstract class FlowNode
{
    public NodeId Id { get; }
    public abstract int InputCount { get; }
    public abstract int OutputCount { get; }
    public abstract string Label { get; }

    protected FlowNode(NodeId id) => Id = id;

    public abstract IReadOnlyList<FlowFraction> Process(IReadOnlyList<FlowFraction> inputs);

    public override string ToString() => $"{Label}({Id})";
}
