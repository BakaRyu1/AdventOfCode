using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day22 : DayRunner<long[]>
{
    public override long[] Parse(FileReference file)
    {
        return file.GetLines()
            .Where(line => !line.AsSpan().Trim().IsEmpty)
            .Select(line => line.AsSpan().ToLong())
            .ToArray();
    }

    public override void Part1(long[] data, RunSettings settings)
    {
        var sum = 0Lu;
        foreach (var secret in data)
        {
            var newSecret = (ulong)secret;
            for (var i = 0; i < 2000; ++i)
                newSecret = NextSecret(newSecret);
            if (settings.Verbose)
                Console.WriteLine(secret + ": " + newSecret);
            sum += newSecret;
        }
        Console.WriteLine("Sum of secrets is " + sum);
    }

    public override void Part2(long[] data, RunSettings settings)
    {
        var newSecrets = new ulong[data.Length];
        int[] bestSequence = [];
        int bestTotal = 0;
        if (settings.Verbose)
            Console.WriteLine("Calculating prices...");
        var allPrices = new Dictionary<uint, int>[data.Length];
        for (var i = 0; i < data.Length; ++i)
        {
            var newSecret = (ulong)data[i];
            var prevPrice = (int)(newSecret % 10);
            var prices = allPrices[i] = [];
            var deque = new LinkedList<int>();
            for (var j = 0; j < 2000; ++j)
            {
                newSecret = NextSecret(newSecret);
                var price = (int)(newSecret % 10);
                deque.AddLast(price - prevPrice);
                if (deque.Count > 4)
                    deque.RemoveFirst();
                if (deque.Count == 4)
                {
                    var key = MakeKey(deque);
                    if (!prices.ContainsKey(key))
                        prices[key] = price;
                }
                prevPrice = price;
            }
        }
        if (settings.Verbose)
            Console.WriteLine("Searching for best total...");
        var visited = new HashSet<uint>();
        for (var i = 0; i < data.Length; ++i)
        {
            var newSecret = (ulong)data[i];
            var prevPrice = (int)(newSecret % 10);
            var deque = new LinkedList<int>();
            for (var j = 0; j < 2000; ++j)
            {
                newSecret = NextSecret(newSecret);
                var price = (int)(newSecret % 10);
                deque.AddLast(price - prevPrice);
                if (deque.Count > 4)
                    deque.RemoveFirst();
                if (deque.Count == 4 && price == 9)
                {
                    var key = MakeKey(deque);
                    if (!visited.Contains(key))
                    {
                        visited.Add(key);
                        var sequence = deque.ToArray();
                        var total = GetTotalPrice(allPrices, key);
                        if (total > bestTotal)
                        {
                            bestTotal = total;
                            bestSequence = sequence;
                            if (settings.Verbose)
                            {
                                Console.WriteLine("Found better total: " + bestTotal);
                                Console.WriteLine("\t=> Sequence: " + string.Join(",", bestSequence));
                            }
                        }
                    }
                }
                prevPrice = price;
            }
        }
        Console.WriteLine("Best total is " + bestTotal);
        Console.WriteLine("\t=> Sequence: " + string.Join(",", bestSequence));
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day22), settings.Example ? "day22-example.txt" : "day22-input.txt");
    }

    private static ulong Mix(ulong secret, ulong value) => secret ^ value;
    private static ulong Prune(ulong secret) => secret % 0x1000000;
    private static ulong NextSecret(ulong secret)
    {
        secret = Prune(Mix(secret, secret << 6));
        secret = Prune(Mix(secret, secret >> 5));
        secret = Prune(Mix(secret, secret << 11));
        return secret;
    }

    private static int GetTotalPrice(Dictionary<uint, int>[] allPrices, uint key)
    {
        var sum = 0;
        foreach (var prices in allPrices)
        {
            if (prices.TryGetValue(key, out var price))
                sum += price;
        }
        return sum;
    }
    /*
    private static uint MakeKey(int[] sequence) => MakeKey(sequence[0], sequence[1], sequence[2], sequence[3]);
    private static uint MakeKey(int i, int j, int k, int l)
    {
        
        return ((uint)(i + 9) << 24)
            | ((uint)(j + 9) << 16)
            | ((uint)(k + 9) << 8)
            | ((uint)(l + 9));
    }*/
    private static uint MakeKey(IEnumerable<int> sequence) => sequence.Aggregate(0u, (acc, val) => (uint)(acc << 8) | (uint)(val + 9));
}
