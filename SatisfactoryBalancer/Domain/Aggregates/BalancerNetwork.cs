using SatisfactoryBalancer.Domain.Entities;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Aggregates;

public class BalancerNetwork
{
    private readonly List<FlowNode> _nodes = [];
    private readonly List<FlowEdge> _edges = [];

    public int Inputs { get; }
    public int Outputs { get; }
    public IReadOnlyList<FlowNode> Nodes => _nodes;
    public IReadOnlyList<FlowEdge> Edges => _edges;

    public int SplitterCount => _nodes.Count(n => n is Splitter);
    public int MergerCount => _nodes.Count(n => n is Merger);
    public int TotalNodes => _nodes.Count;

    public BalancerNetwork(int inputs, int outputs)
    {
        if (inputs <= 0 || outputs <= 0)
            throw new ArgumentException("Entradas e saídas devem ser maiores que zero.");
        Inputs = inputs;
        Outputs = outputs;
    }

    public void AddNode(FlowNode node) => _nodes.Add(node);

    public void AddEdge(FlowEdge edge) => _edges.Add(edge);

    public FlowFraction ExpectedOutputFlow(FlowFraction inputFlow) =>
        new FlowFraction(inputFlow.Numerator * Inputs, inputFlow.Denominator * Outputs);

    public override string ToString() =>
        $"Balancer {Inputs}→{Outputs} | {SplitterCount} splitters, {MergerCount} mergers, {_edges.Count} arestas";
}
