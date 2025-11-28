using AdventOfCode.Utils;

namespace AdventOfCode._2015;

internal class Day01 : DayRunner<bool[]>
{
    public override bool[] Parse(FileReference file)
    {
        return [.. file.GetText().Select(ch => ch switch
        {
            '(' => true,
            ')' => false,
            _ => throw new InvalidOperationException($"Invalid character '{ch}'")
        })];
    }

    public override void Part1(bool[] data, RunSettings settings)
    {
        int floor = 0;
        foreach (var command in data)
        {
            if (command)
                ++floor;
            else
                --floor;
        }
        Console.WriteLine($"Santa ends up on floor {floor}.");
    }

    public override void Part2(bool[] data, RunSettings settings)
    {
        int floor = 0;
        foreach (var (i, command) in data.Index())
        {
            if (command)
                ++floor;
            else
                --floor;
            if (floor == -1)
            {
                Console.WriteLine($"Santa enters the basement at position {i+1}.");
                return;
            }
        }
        Console.WriteLine("Failed to find basement position.");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day01), settings.Example ? "day01-example.txt" : "day01-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day01), "day01-example2.txt") : null);
    }
}
