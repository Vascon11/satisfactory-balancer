namespace SatisfactoryBalancer.Domain.ValueObjects;

public record FlowFraction
{
    public int Numerator { get; }
    public int Denominator { get; }

    public FlowFraction(int numerator, int denominator)
    {
        if (denominator == 0) throw new ArgumentException("Denominador não pode ser zero.");
        if (numerator < 0 || denominator < 0) throw new ArgumentException("Fluxo não pode ser negativo.");

        var gcd = Gcd(numerator, denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    public static FlowFraction Zero => new(0, 1);
    public static FlowFraction One => new(1, 1);

    public FlowFraction SplitBy(int parts) => new(Numerator, Denominator * parts);

    public static FlowFraction operator +(FlowFraction a, FlowFraction b)
    {
        var lcm = Lcm(a.Denominator, b.Denominator);
        return new FlowFraction(a.Numerator * (lcm / a.Denominator) + b.Numerator * (lcm / b.Denominator), lcm);
    }

    public static FlowFraction operator *(FlowFraction a, int scalar) => new(a.Numerator * scalar, a.Denominator);

    public double ToDouble() => (double)Numerator / Denominator;

    public override string ToString() => Denominator == 1 ? $"{Numerator}" : $"{Numerator}/{Denominator}";

    private static int Gcd(int a, int b)
    {
        while (b != 0) { (a, b) = (b, a % b); }
        return a == 0 ? 1 : a;
    }

    internal static int Lcm(int a, int b) => a / Gcd(a, b) * b;
}
