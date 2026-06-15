namespace SatisfactoryBalancer.Domain.Algorithms;

public static class LcmExpander
{
    /// <summary>
    /// Calcula o menor K múltiplo de LCM(N,M) tal que K/N seja da forma 2^a×3^b.
    ///
    /// A constraint real do algoritmo é apenas que K/N seja fatorável em 2s e 3s
    /// (árvore de splitters). K/M pode ser qualquer inteiro — qualquer merge count
    /// é construível com árvore de mergers 2→1 e 3→1.
    /// </summary>
    public static int FindMinimalK(int n, int m)
    {
        var lcm = Lcm(n, m);
        for (int t = 1; t <= 10_000; t++)
        {
            var k = lcm * t;
            if (IsPowerOf2And3(k / n)) return k;
        }
        throw new InvalidOperationException($"Não foi possível encontrar K válido para {n}→{m}");
    }

    /// <summary>
    /// Decompõe um número N em uma sequência de fatores 2 e 3.
    /// Ex: 12 → [2, 2, 3] (pois 12 = 2×2×3)
    /// Retorna null se N não for da forma 2^a × 3^b.
    /// </summary>
    public static List<int>? Factorize(int n)
    {
        var factors = new List<int>();
        while (n % 2 == 0) { factors.Add(2); n /= 2; }
        while (n % 3 == 0) { factors.Add(3); n /= 3; }
        return n == 1 ? factors : null;
    }

    /// <summary>
    /// Verifica se um número é decomponível exclusivamente em fatores 2 e 3.
    /// </summary>
    public static bool IsPowerOf2And3(int n)
    {
        if (n <= 0) return false;
        while (n % 2 == 0) n /= 2;
        while (n % 3 == 0) n /= 3;
        return n == 1;
    }

    /// <summary>
    /// Encontra o próximo número >= target que seja da forma 2^a × 3^b e divisível por divisor.
    /// </summary>
    public static int NextPowerOf2And3(int target, int? mustDivide = null)
    {
        // BFS sobre candidatos 2^a × 3^b em ordem crescente
        var candidates = new SortedSet<int>();
        for (int a = 0; (int)Math.Pow(2, a) <= target * 8; a++)
        for (int b = 0; (int)(Math.Pow(2, a) * Math.Pow(3, b)) <= target * 8; b++)
        {
            var candidate = (int)(Math.Pow(2, a) * Math.Pow(3, b));
            if (candidate >= target)
                candidates.Add(candidate);
        }

        foreach (var c in candidates)
        {
            if (mustDivide.HasValue && c % mustDivide.Value != 0) continue;
            return c;
        }

        throw new InvalidOperationException($"Não foi possível encontrar K válido para target={target}");
    }

    public static int Gcd(int a, int b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return a;
    }

    public static int Lcm(int a, int b) => a / Gcd(a, b) * b;
}
