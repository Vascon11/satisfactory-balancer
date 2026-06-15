using SatisfactoryBalancer.Domain.ValueObjects;

namespace SatisfactoryBalancer.Domain.Aggregates;

public record FlowEdge(NodeId From, int FromPort, NodeId To, int ToPort, FlowFraction Flow);
