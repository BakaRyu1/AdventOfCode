using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day19 : DayRunner<Day19.Data>
{
    public struct Data
    {
        public (string From, string To)[] Substitutions;
        public string Molecule;
    }
    public override Data Parse(FileReference file)
    {
        var subs = new List<(string, string)>();
        string? molecule = null;
        var isSubs = true;
        foreach (var line in file.GetLines())
        {
            if (isSubs)
            {
                if (line.Length == 0)
                    isSubs = false;
                else
                {
                    var pos = line.IndexOf("=>");
                    var from = line.AsSpan(0, pos).Trim().ToString();
                    var to = line.AsSpan(pos + 2).Trim().ToString();
                    subs.Add((from, to));
                }
            }
            else
            {
                if (molecule != null)
                    throw new InvalidOperationException($"Duplicate molecule: {line}");
                molecule = line;
            }
        }
        return new()
        {
            Molecule = molecule ?? throw new InvalidOperationException("No molecule defined"),
            Substitutions = [.. subs]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var molecules = new HashSet<string>();
        if (settings.Verbose)
            Console.WriteLine(data.Molecule);
        foreach (var newMolecule in GetSubstitutions(data.Molecule, data.Substitutions))
        {
            if (settings.Verbose)
                Console.WriteLine($"=> {newMolecule}");
            molecules.Add(newMolecule);
        }
        Console.WriteLine($"There are {molecules.Count} possible molecules.");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var queue = new Queue<(string, int)>();
        var totalCount = 0;
        var molecule = data.Molecule;
        while (molecule != "e")
        {
            var hasSubstitued = false;
            foreach (var (from, to) in data.Substitutions)
            {
                var pos = molecule.IndexOf(to);
                if (pos >= 0)
                {
                    molecule = string.Concat(molecule.AsSpan(0, pos), from, molecule.AsSpan(pos + to.Length));
                    ++totalCount;
                    hasSubstitued = true;
                }
            }
            if (!hasSubstitued)
                throw new InvalidOperationException("Couldn't substitute anything.");
        }
        Console.WriteLine($"There are {totalCount} steps before reaching {data.Molecule}");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day19), settings.Example ? "day19-example.txt" : "day19-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day19), "day19-example2.txt") : null);
    }

    private static IEnumerable<string> GetSubstitutions(string molecule, (string from, string to)[] substitutions)
    {
        foreach (var (from, to) in substitutions)
        {
            foreach (var newMolecule in GetSubstitutions(molecule, from, to))
                yield return newMolecule;
        }
    }

    private static IEnumerable<string> GetSubstitutions(string molecule, string from, string to)
    {
        var pos = molecule.IndexOf(from);
        while (pos >= 0)
        {
            var newMolecule = string.Concat(molecule.AsSpan(0, pos), to, molecule.AsSpan(pos + from.Length));
            yield return newMolecule;
            pos = molecule.IndexOf(from, pos + 1);
        }
    }
}
