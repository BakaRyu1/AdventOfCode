using AdventOfCode.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day08 : DayRunner<Day08.Data>
{
    [DebuggerDisplay("{ID} (Left={Left}, Right={Right}")]
    public struct Node
    {
        public string ID;
        public string Left;
        public string Right;
    }
    public struct Data
    {
        public string Instructions;
        public Dictionary<string, Node> Nodes;
    }

    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var data = new Data()
        {
            Nodes = []
        };

        foreach (var line in lines)
        {
            var lineSpan = line.AsSpan().Trim();
            if (lineSpan.IsEmpty)
                continue;
            if (data.Instructions == null)
            {
                if (lineSpan.IndexOfAnyExcept("LR") >= 0)
                {
                    Console.Error.WriteLine("Found invalid instructions: " + line);
                    throw new InvalidOperationException();
                }
                data.Instructions = lineSpan.ToString();
                continue;
            }
            var nodeMatch = NodePattern().Match(line);
            if (!nodeMatch.Success)
            {
                Console.Error.WriteLine("Invalid node: " + line);
                throw new InvalidOperationException();
            }
            var id = nodeMatch.Groups["node"].Value;
            data.Nodes.Add(id, new Node()
            {
                ID = id,
                Left = nodeMatch.Groups["left"].Value,
                Right = nodeMatch.Groups["right"].Value
            });
        }
        return data;
    }
    public override void Part1(Data data, RunSettings settings)
    {
        var steps = CountSteps(data, "AAA", node => node.ID == "ZZZ");
        Console.WriteLine("Steps required: " + steps);
    }
    public override void Part2(Data data, RunSettings settings)
    {
        var steps = data.Nodes.Keys
            .Where(id => id.EndsWith('A'))
            .Select(id => CountSteps(data, id, node => node.ID.EndsWith('Z')))
            .ToArray();
        Console.WriteLine("Steps required for ghosts: " + steps.Aggregate(MathUtils.LowestCommonMultiplicator));
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day08), settings.Example ? "day08-example1.txt" : "day08-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day08), "day08-example2.txt") : null);
    }

    [GeneratedRegex(@"^\s*(?<node>[A-Z0-9]+)\s*=\s*\(\s*(?<left>[A-Z0-9]+)\s*,\s*(?<right>[A-Z0-9]+)\s*\)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex NodePattern();

    private static long CountSteps(Data data, string initialNode, Func<Node, bool> predicate)
    {
        var node = data.Nodes[initialNode];
        var found = false;
        var count = 0L;
        while (!found)
        {
            foreach (var dir in data.Instructions)
            {
                if (dir == 'L')
                    node = data.Nodes[node.Left];
                else // dir == 'R'
                    node = data.Nodes[node.Right];
                ++count;
                if (predicate(node))
                {
                    found = true;
                    break;
                }
            }
        }
        return count;
    }
}
