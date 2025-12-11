using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day23 : DayRunner<(string, string)[]>
{
    public override (string, string)[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var pairs = new List<(string, string)>();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var parts = line.Split('-');
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Couldn't parse line: " + line);
                throw new InvalidOperationException();
            }
            pairs.Add((parts[0], parts[1]));
        }
        return [.. pairs];
    }

    public override void Part1((string, string)[] data, RunSettings settings)
    {
        var allLinks = GetLinks(data);
        var triplets = new HashSet<string[]>(ArrayComparer<string>.Instance);
        foreach (var pc in allLinks.Keys)
        {
            var links = allLinks[pc];
            if (links.Count >= 2)
            {
                var linksArray = links.ToArray();
                foreach (var (i, pc2) in links.Index())
                {
                    foreach (var pc3 in links.Skip(i))
                    {
                        if (!allLinks[pc2].Contains(pc3))
                            continue;
                        string[] strs = [pc, pc2, pc3];
                        Array.Sort(strs);
                        triplets.Add(strs);
                    }
                }       
            }
        }
        var masterPcs = triplets.Count(triplets => triplets.Any(pc => pc.StartsWith("t")));
        Console.WriteLine("There are " + triplets.Count + " sets.");
        Console.WriteLine("Potential chief historian sets: " + masterPcs);
    }

    public override void Part2((string, string)[] data, RunSettings settings)
    {
        var allLinks = GetLinks(data);
        var bestSet = new List<string>();
        var visited = new HashSet<string>();
        foreach (var pc in allLinks.Keys)
        {
            if (visited.Contains(pc))
                continue;
            visited.Add(pc);
            var links = allLinks[pc];
            foreach (var pc2 in links)
                visited.Add(pc);
            var subSet = FindBestSet(allLinks, [pc], links);
            if (subSet.Count > bestSet.Count)
                bestSet = subSet;
        }
        bestSet.Sort();
        Console.WriteLine("Best set is " + string.Join(',', bestSet));
    }

    private static List<string> FindBestSet(Dictionary<string, HashSet<string>> allLinks, List<string> currentSet, IEnumerable<string> potentials)
    {
        if (!potentials.Any())
            return currentSet;
        var bestSet = new List<string>();
        var visited = new HashSet<string>();
        foreach (var (i, pc) in potentials.Index())
        {
            if (visited.Contains(pc))
                continue;
            var links = allLinks[pc];
            if (!currentSet.All(pc2 => links.Contains(pc2)))
                continue;
            var subSet = FindBestSet(allLinks, [.. currentSet, pc], potentials.Skip(i + 1));
            if (subSet.Count > bestSet.Count)
                bestSet = subSet;
            foreach (var pc2 in subSet)
                visited.Add(pc2);
        }
        return bestSet;
    }

    private static Dictionary<string, HashSet<string>> GetLinks(IEnumerable<(string, string)> data)
    {
        var allLinks = new Dictionary<string, HashSet<string>>();
        foreach (var (pc1, pc2) in data)
        {
            if (!allLinks.TryGetValue(pc1, out var links1))
                allLinks[pc1] = links1 = [];
            links1.Add(pc2);
            if (!allLinks.TryGetValue(pc2, out var links2))
                allLinks[pc2] = links2 = [];
            links2.Add(pc1);
        }
        return allLinks;
    }
}
