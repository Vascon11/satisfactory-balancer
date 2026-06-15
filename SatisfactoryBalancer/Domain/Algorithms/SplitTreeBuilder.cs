using SatisfactoryBalancer.Domain.Aggregates;
using SatisfactoryBalancer.Domain.Entities;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Algorithms;

/// <summary>
/// Constrói uma árvore de splitters que divide 1 entrada em K partes iguais.
/// K deve ser da forma 2^a × 3^b.
/// Retorna os NodeIds das K folhas (saídas da árvore).
/// </summary>
public static class SplitTreeBuilder
{
    public static List<(NodeId nodeId, int port)> Build(
        BalancerNetwork network,
        NodeId sourceId,
        int sourcePort,
        FlowFraction sourceFlow,
        int parts)
    {
        if (!LcmExpander.IsPowerOf2And3(parts))
            throw new ArgumentException($"splitFactor={parts} não é decomponível em 2^a × 3^b. O algoritmo FindMinimalK deve garantir isso antes de chamar Build.");

        if (parts == 1)
            return [(sourceId, sourcePort)];

        var factors = LcmExpander.Factorize(parts)!;
        // Agrupa fatores para minimizar profundidade: usa 3s primeiro
        factors.Sort((a, b) => b.CompareTo(a));

        return BuildRecursive(network, sourceId, sourcePort, sourceFlow, factors, 0);
    }

    private static List<(NodeId, int)> BuildRecursive(
        BalancerNetwork network,
        NodeId inputId,
        int inputPort,
        FlowFraction currentFlow,
        List<int> factors,
        int depth)
    {
        if (depth >= factors.Count)
            return [(inputId, inputPort)];

        var fanout = factors[depth];
        var splitter = new Splitter(NodeId.New("SPL"), fanout);
        network.AddNode(splitter);

        var outputFlow = currentFlow.SplitBy(fanout);
        var leaves = new List<(NodeId, int)>();

        // Conecta entrada ao splitter
        network.AddEdge(new FlowEdge(inputId, inputPort, splitter.Id, 0, currentFlow));

        for (int port = 0; port < fanout; port++)
        {
            var subLeaves = BuildRecursive(network, splitter.Id, port, outputFlow, factors, depth + 1);
            leaves.AddRange(subLeaves);
        }

        return leaves;
    }
}
