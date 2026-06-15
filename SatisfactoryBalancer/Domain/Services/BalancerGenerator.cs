using SatisfactoryBalancer.Domain.Aggregates;
using SatisfactoryBalancer.Domain.Algorithms;
using SatisfactoryBalancer.Domain.Entities;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Services;

public static class BalancerGenerator
{
    /// <summary>
    /// Gera uma rede de balanceamento para N entradas → M saídas.
    ///
    /// Algoritmo Expand-and-Merge:
    ///   1. K = menor número 2^a×3^b divisível por LCM(N, M)
    ///   2. Cada entrada é dividida em K/N partes (árvore de splitters)
    ///   3. K partes totais são redistribuídas: K/M partes por saída (árvore de mergers)
    /// </summary>
    public static BalancerNetwork Generate(int inputs, int outputs)
    {
        var network = new BalancerNetwork(inputs, outputs);

        var k = LcmExpander.FindMinimalK(inputs, outputs);
        var splitFactor = k / inputs;   // cada entrada vira splitFactor partes
        var mergeFactor = k / outputs;  // cada saída recebe mergeFactor partes

        var inputFlow = FlowFraction.One;
        var partFlow = inputFlow.SplitBy(splitFactor);

        // Cria K slots intermediários: uma lista de (nodeId, port, flow)
        var allParts = new List<(NodeId nodeId, int port, FlowFraction flow)>();

        for (int i = 0; i < inputs; i++)
        {
            var inputNode = new InputTerminal(NodeId.New($"IN{i + 1}"));
            network.AddNode(inputNode);

            var leaves = SplitTreeBuilder.Build(network, inputNode.Id, 0, inputFlow, splitFactor);

            foreach (var leaf in leaves)
                allParts.Add((leaf.nodeId, leaf.port, partFlow));
        }

        // Cada saída recebe mergeFactor partes consecutivas (round-robin garante uniformidade)
        // Redistribuição: parte i vai para saída (i % outputs)
        var outputBuckets = new List<List<(NodeId, int, FlowFraction)>>(outputs);
        for (int o = 0; o < outputs; o++)
            outputBuckets.Add([]);

        for (int i = 0; i < allParts.Count; i++)
            outputBuckets[i % outputs].Add(allParts[i]);

        for (int o = 0; o < outputs; o++)
        {
            var (mergeOut, mergePort) = MergeTreeBuilder.Build(network, outputBuckets[o]);
            var outputNode = new OutputTerminal(NodeId.New($"OUT{o + 1}"));
            network.AddNode(outputNode);

            var mergedFlow = outputBuckets[o].Aggregate(FlowFraction.Zero, (acc, s) => acc + s.Item3);
            network.AddEdge(new FlowEdge(mergeOut, mergePort, outputNode.Id, 0, mergedFlow));
        }

        return network;
    }
}
