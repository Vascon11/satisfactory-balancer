namespace SatisfactoryBalancer.Domain.ValueObjects;

public record NodeId(string Value)
{
    public static NodeId New(string prefix) => new($"{prefix}_{Guid.NewGuid().ToString("N")[..6]}");
    public override string ToString() => Value;
}
