using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day24 : DayRunner<int[]>
{
    public override int[] Parse(FileReference file)
    {
        var packages = new List<int>();
        foreach (var line in file.GetLines())
            packages.Add(line.AsSpan().ToInt());
        packages.Sort();
        packages.Reverse();
        return [.. packages];
    }

    public override void Part1(int[] data, RunSettings settings)
    {
        var totalWeight = data.Aggregate((a, b) => a + b);
        var groupWeight = totalWeight / 3;
        if (settings.Verbose)
            Console.WriteLine($"Group weight is {groupWeight}");
        var bestGroup = data;
        var bestQe = long.MaxValue;
        foreach (var group1 in GetAllDispositions(data, groupWeight, []))
        {
            long? qe = null;
            if (group1.Length > bestGroup.Length)
                continue;
            if (group1.Length == bestGroup.Length && (qe = QuantumEntanglement(group1)) > bestQe)
                continue;
            var remainingBoxes = data.Except(group1).ToList();
            var group2 = GetAllDispositions(remainingBoxes, groupWeight, []).FirstOrDefault();
            if (group2 == null)
                continue;
            bestGroup = group1;
            bestQe = qe ??= QuantumEntanglement(group1);
            if (settings.Verbose)
            {
                var group3 = remainingBoxes.Except(group2).ToArray();
                Console.WriteLine($"{string.Join(" ", group1)} (QE={qe}); {string.Join(" ", group2)}; {string.Join(" ", group3)}");
            }
        }
        Console.WriteLine($"Best group 1 is {string.Join(" ", bestGroup)}");
        Console.WriteLine($"Quantum entanglement is {bestQe}");
    }

    public override void Part2(int[] data, RunSettings settings)
    {
        var totalWeight = data.Aggregate((a, b) => a + b);
        var groupWeight = totalWeight / 4;
        if (settings.Verbose)
            Console.WriteLine($"Group weight is {groupWeight}");
        var bestGroup = data;
        var bestQe = long.MaxValue;
        foreach (var group1 in GetAllDispositions(data, groupWeight, []))
        {
            long? qe = null;
            if (group1.Length > bestGroup.Length)
                continue;
            if (group1.Length == bestGroup.Length && (qe = QuantumEntanglement(group1)) > bestQe)
                continue;
            var remainingBoxes = data.Except(group1).ToList();
            int[]? group2 = null;
            int[]? group3 = null;
            int[]? group4 = null;
            foreach (var possibleGroup2 in GetAllDispositions(remainingBoxes, groupWeight, []))
            {
                var remainingBoxes2 = remainingBoxes.Except(possibleGroup2).ToList();
                group3 = GetAllDispositions(remainingBoxes2, groupWeight, []).FirstOrDefault();
                if (group3 != null)
                {
                    group2 = possibleGroup2;
                    if (settings.Verbose)
                        group4 = [.. remainingBoxes2.Except(group3)];
                    break;
                }
            }
            if (group2 == null || group3 == null)
                continue;
            bestGroup = group1;
            bestQe = qe ??= QuantumEntanglement(group1);
            if (settings.Verbose)
                Console.WriteLine($"{string.Join(" ", group1)} (QE={qe}); {string.Join(" ", group2)}; {string.Join(" ", group3)}; {string.Join(" ", group4!)}");
        }
        Console.WriteLine($"Best group 1 is {string.Join(" ", bestGroup)}");
        Console.WriteLine($"Quantum entanglement is {bestQe}");
    }

    private static long QuantumEntanglement(IEnumerable<int> packages) => packages.Aggregate(1L, (a, b) => a * b);

    private static IEnumerable<int[]> GetAllDispositions(IEnumerable<int> packages, int maxWeight, Dictionary<(int, int), int[][]> cache)
    {
        if (maxWeight == 0)
        {
            yield return [];
            yield break;
        }
        if (cache.TryGetValue((packages.Count(), maxWeight), out var cachedSequences))
        {
            foreach (var sequence in cachedSequences)
                yield return sequence;
            yield break;
        }
        var sequences = new List<int[]>();
        foreach (var (i, package) in packages.Index())
        {
            foreach (var subSequence in GetAllDispositions(packages.Skip(i + 1), maxWeight - package, cache))
            {
                int[] newSequence = [package, .. subSequence];
                yield return newSequence;
                sequences.Add(newSequence);
            }
        }
        cache[(packages.Count(), maxWeight)] = [.. sequences];
    }
}
