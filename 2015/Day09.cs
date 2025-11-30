using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day09 : DayRunner<Dictionary<(string, string), int>>
{
    public override Dictionary<(string, string), int> Parse(FileReference file)
    {
        var distances = new Dictionary<(string, string), int>();
        foreach (var line in file.GetLines())
        {
            var match = DistancePattern().Match(line);
            if (!match.Success)
                throw new InvalidOperationException($"Couldn't parse line: {line}");
            var from = match.Groups["from"].Value;
            var to = match.Groups["to"].Value;
            var distance = match.Groups["distance"].ValueSpan.ToInt();
            distances[(from, to)] = distance;
            distances[(to, from)] = distance;
        }
        return distances;
    }

    public override void Part1(Dictionary<(string, string), int> data, RunSettings settings)
    {
        var bestDistance = int.MaxValue;
        string[] bestPath = [];
        ForEachPath(data, (path, distance) =>
        {
            if (distance < bestDistance)
            {
                bestPath = path;
                bestDistance = distance;
            }
        });
        Console.WriteLine($"Shortest distance is {bestDistance}");
        Console.WriteLine(string.Join(" -> ", bestPath));
    }

    public override void Part2(Dictionary<(string, string), int> data, RunSettings settings)
    {
        var bestDistance = -1;
        string[] bestPath = [];
        ForEachPath(data, (path, distance) =>
        {
            if (distance > bestDistance)
            {
                bestPath = path;
                bestDistance = distance;
            }
        });
        Console.WriteLine($"Longest distance is {bestDistance}");
        Console.WriteLine(string.Join(" -> ", bestPath));
    }

    [GeneratedRegex(@"^\s*(?<from>\w+)\s+to\s+(?<to>\w+)\s*=\s*(?<distance>\d+)\s*$")]
    private static partial Regex DistancePattern();

    private static void ForEachPath(Dictionary<(string, string), int> distances, Action<string[], int> callback)
    {
        var uniqueNames = distances.Keys.Select(key => key.Item1).ToHashSet();
        if (uniqueNames.Count == 0)
            throw new InvalidOperationException();
        if (uniqueNames.Count == 1)
        {
            callback([uniqueNames.First()], 0);
            return;
        }
        var queue = new Queue<(string[], int, string[])>();
        foreach (var name in uniqueNames)
            queue.Enqueue(([name], 0, [.. uniqueNames.Where(n => n != name)]));
        while (queue.Count > 0)
        {
            var (path, distance, unusedNames) = queue.Dequeue();
            if (unusedNames.Length == 1)
            {
                callback([.. path, unusedNames[0]], distance + distances[(path[^1], unusedNames[0])]);
            }
            else
            {
                foreach (var name in unusedNames)
                    queue.Enqueue(([.. path, name], distance + distances[(path[^1], name)], [.. unusedNames.Where(n => n != name)]));
            }
        }
    }
}
