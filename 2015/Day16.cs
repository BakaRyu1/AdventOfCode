using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day16 : DayRunner<Day16.Data>
{
    public struct Data
    {
        public Dictionary<string, int> Clues;
        public Dictionary<string, int>[] Aunts;
    }
    public override Data Parse(FileReference file)
    {
        var clues = new Dictionary<string, int>();
        var aunts = new List<Dictionary<string, int>>();
        var parsingClues = true;
        var currentAunt = 1;
        foreach (var line in file.GetLines())
        {
            if (parsingClues)
            {
                if (line == "---")
                {
                    parsingClues = false;
                    continue;
                }
                var pos = line.IndexOf(':');
                var name = line.AsSpan(0, pos).Trim().ToString();
                var count = line.AsSpan(pos + 1).ToInt();
                clues[name] = count;
            }
            else
            {
                if (!line.StartsWith("Sue "))
                    throw new InvalidOperationException($"Aunt Sue is not detected: {line}");
                var pos = line.IndexOf(':');
                var number = line.AsSpan(4, pos - 4).ToInt();
                if (number != currentAunt)
                    throw new InvalidOperationException($"Unexpected aunt number: {line}");
                ++currentAunt;
                var data = new Dictionary<string, int>();
                var itemsStr = line.AsSpan(pos + 1);
                foreach (var range in itemsStr.Split(','))
                {
                    var itemStr = itemsStr[range].Trim();
                    var pos2 = itemStr.IndexOf(':');
                    var name = itemStr.Slice(0, pos2).ToString();
                    var count = itemStr.Slice(pos2 + 1).ToInt();
                    data[name] = count;
                }
                aunts.Add(data);
            }
        }
        return new()
        {
            Clues = clues,
            Aunts = [.. aunts]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        foreach (var (i, aunt) in data.Aunts.Index())
        {
            var match = true;
            foreach (var clue in data.Clues)
            {
                if (aunt.TryGetValue(clue.Key, out var count) && clue.Value != count)
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                Console.WriteLine($"Found matching aunt {i + 1}");
            }
        }
    }

    public override void Part2(Data data, RunSettings settings)
    {
        foreach (var (i, aunt) in data.Aunts.Index())
        {
            var match = true;
            foreach (var clue in data.Clues)
            {
                if (aunt.TryGetValue(clue.Key, out var count))
                {
                    if (GREATER_AMOUNTS.Contains(clue.Key))
                    {
                        if (count <= clue.Value)
                            match = false;
                    }
                    else if (FEWER_AMOUNTS.Contains(clue.Key))
                    {
                        if (count >= clue.Value)
                            match = false;
                    }
                    else if (clue.Value != count)
                        match = false;
                    if (match == false)
                        break;
                }
            }
            if (match)
            {
                Console.WriteLine($"Found matching aunt {i + 1}");
            }
        }
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day16), "day16-input.txt");
    }

    private static readonly HashSet<string> GREATER_AMOUNTS = ["cats", "trees"];
    private static readonly HashSet<string> FEWER_AMOUNTS = ["pomeranians", "goldfish"];
}
