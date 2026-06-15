using SatisfactoryBalancer.Domain.Aggregates;
using SatisfactoryBalancer.Domain.Entities;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Algorithms;

/// <summary>
/// Constrói uma árvore de mergers que combina N fontes em 1 saída.
/// Aceita qualquer N >= 1, pois todo inteiro >= 2 pode ser decomposto em somas de 2 e 3.
/// </summary>
public static class MergeTreeBuilder
{
    public static (NodeId nodeId, int port) Build(
        BalancerNetwork network,
        List<(NodeId nodeId, int port, FlowFraction flow)> sources)
    {
        if (sources.Count == 0)
            throw new ArgumentException("Nenhuma fonte para merger.");

        if (sources.Count == 1)
            return (sources[0].nodeId, sources[0].port);

        return BuildRecursive(network, sources);
    }

    private static (NodeId, int) BuildRecursive(
        BalancerNetwork network,
        List<(NodeId nodeId, int port, FlowFraction flow)> sources)
    {
        if (sources.Count == 1)
            return (sources[0].nodeId, sources[0].port);

        var nextLevel = new List<(NodeId, int, FlowFraction)>();
        int i = 0;

        // Se a contagem tem resto 1 ao dividir por 2 (ímpar), usa um grupo de 3 primeiro
        // para eliminar o resto sem quebrar a uniformidade da árvore.
        if (sources.Count % 2 == 1 && sources.Count >= 3)
        {
            var group = sources.GetRange(0, 3);
            nextLevel.Add(MergeGroup(network, group));
            i = 3;
        }

        while (i < sources.Count)
        {
            var group = sources.GetRange(i, 2);
            nextLevel.Add(MergeGroup(network, group));
            i += 2;
        }

        return BuildRecursive(network, nextLevel);
    }

    private static (NodeId, int, FlowFraction) MergeGroup(
        BalancerNetwork network,
        List<(NodeId nodeId, int port, FlowFraction flow)> group)
    {
        var merger = new Merger(NodeId.New("MRG"), group.Count);
        network.AddNode(merger);

        var totalFlow = group.Aggregate(FlowFraction.Zero, (acc, s) => acc + s.flow);

        for (int port = 0; port < group.Count; port++)
            network.AddEdge(new FlowEdge(group[port].nodeId, group[port].port, merger.Id, port, group[port].flow));

        return (merger.Id, 0, totalFlow);
    }
}
