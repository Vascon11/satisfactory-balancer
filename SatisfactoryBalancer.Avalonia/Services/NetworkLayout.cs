using SatisfactoryBalancer.Domain.Aggregates;
using SatisfactoryBalancer.Domain.Entities;
using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Avalonia.Services;

public record NodePosition(
    string Id, string Label, string Color,
    double X, double Y, double Width, double Height);

public record EdgePath(
    string FromId, string ToId,
    double X1, double Y1, double X2, double Y2,
    string FlowLabel);

public class NetworkLayoutResult
{
    public List<NodePosition> Nodes { get; init; } = [];
    public List<EdgePath> Edges { get; init; } = [];
    public double SvgWidth { get; init; }
    public double SvgHeight { get; init; }
    public bool IsTooLarge { get; init; }
}

public static class NetworkLayoutService
{
    private const double NodeW = 72;
    private const double NodeH = 30;
    private const double LevelGap = 150;
    private const double NodeGap = 52;
    private const double PaddingX = 20;
    private const double PaddingY = 30;
    private const int MaxNodes = 120;

    public static NetworkLayoutResult Compute(BalancerNetwork network)
    {
        if (network.TotalNodes > MaxNodes)
            return new NetworkLayoutResult { IsTooLarge = true };

        // 1. Assign topological levels (BFS from inputs)
        var levels = AssignLevels(network);

        // 2. Group nodes by level
        var byLevel = network.Nodes
            .GroupBy(n => levels[n.Id.Value])
            .OrderBy(g => g.Key)
            .ToList();

        int maxLevel = byLevel.Max(g => g.Key);

        // 3. Assign y positions within each level using predecessor barycenter
        var positions = new Dictionary<string, (double x, double y)>();

        int maxNodesInAnyLevel = byLevel.Max(g => g.Count());
        double totalGridHeight = maxNodesInAnyLevel * NodeGap;

        foreach (var group in byLevel)
        {
            var nodesInLevel = group.ToList();
            var level = group.Key;
            double x = PaddingX + level * LevelGap;

            // Sort by average y of predecessors (barycenter heuristic)
            var withBary = nodesInLevel.Select(n =>
            {
                var preds = network.Edges
                    .Where(e => e.To == n.Id && positions.ContainsKey(e.From.Value))
                    .Select(e => positions[e.From.Value].y)
                    .ToList();
                double bary = preds.Count > 0 ? preds.Average() : double.MaxValue;
                return (node: n, bary);
            })
            .OrderBy(t => t.bary)
            .ToList();

            int count = withBary.Count;
            double columnHeight = count * NodeGap;
            // Center this column vertically relative to the tallest column
            double startY = PaddingY + (totalGridHeight - columnHeight) / 2.0;

            for (int i = 0; i < count; i++)
            {
                var n = withBary[i].node;
                positions[n.Id.Value] = (x, startY + i * NodeGap);
            }
        }

        // 4. Build NodePosition list
        var nodeList = network.Nodes.Select(n =>
        {
            var (x, y) = positions[n.Id.Value];
            var (label, color) = NodeStyle(n);
            return new NodePosition(n.Id.Value, label, color, x, y, NodeW, NodeH);
        }).ToList();

        // 5. Build EdgePath list (bezier curves)
        var edgeList = network.Edges.Select(e =>
        {
            if (!positions.ContainsKey(e.From.Value) || !positions.ContainsKey(e.To.Value))
                return null;

            var (fx, fy) = positions[e.From.Value];
            var (tx, ty) = positions[e.To.Value];

            double x1 = fx + NodeW;
            double y1 = fy + NodeH / 2;
            double x2 = tx;
            double y2 = ty + NodeH / 2;

            return new EdgePath(e.From.Value, e.To.Value, x1, y1, x2, y2, e.Flow.ToString());
        })
        .Where(e => e != null)
        .Select(e => e!)
        .ToList();

        double svgW = PaddingX * 2 + (maxLevel + 1) * LevelGap + NodeW;
        double svgH = PaddingY * 2 + byLevel.Max(g => g.Count()) * NodeGap;

        return new NetworkLayoutResult
        {
            Nodes = nodeList,
            Edges = edgeList,
            SvgWidth = Math.Max(svgW, 400),
            SvgHeight = Math.Max(svgH, 200)
        };
    }

    private static Dictionary<string, int> AssignLevels(BalancerNetwork network)
    {
        var levels = new Dictionary<string, int>();
        var inDegree = network.Nodes.ToDictionary(n => n.Id.Value, _ => 0);

        foreach (var e in network.Edges)
            if (inDegree.ContainsKey(e.To.Value))
                inDegree[e.To.Value]++;

        var queue = new Queue<string>(
            network.Nodes.Where(n => inDegree[n.Id.Value] == 0).Select(n => n.Id.Value));

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            foreach (var e in network.Edges.Where(e => e.From.Value == id))
            {
                int newLevel = (levels.TryGetValue(id, out var l) ? l : 0) + 1;
                if (!levels.TryGetValue(e.To.Value, out var cur) || newLevel > cur)
                    levels[e.To.Value] = newLevel;

                inDegree[e.To.Value]--;
                if (inDegree[e.To.Value] == 0)
                    queue.Enqueue(e.To.Value);
            }
        }

        // Ensure all nodes have a level
        foreach (var n in network.Nodes)
            levels.TryAdd(n.Id.Value, 0);

        return levels;
    }

    private static (string label, string color) NodeStyle(FlowNode node) => node switch
    {
        InputTerminal t  => (t.Id.Value.Split('_')[0], "#22c55e"),  // green
        OutputTerminal t => (t.Id.Value.Split('_')[0], "#ef4444"),  // red
        Splitter s       => (s.Label, "#3b82f6"),                    // blue
        Merger m         => (m.Label, "#f97316"),                    // orange
        _                => (node.Label, "#6b7280")
    };
}
