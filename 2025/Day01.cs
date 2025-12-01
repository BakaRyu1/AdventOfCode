using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day01 : DayRunner<int[]>
{
    public override int[] Parse(FileReference file)
    {
        var rotations = new List<int>();
        foreach (var line in file.GetLines()) {
            if (line.Length < 2)
                throw new InvalidOperationException($"Invalid line: {line}");
            var direction = line[0] switch
            {
                'L' => -1,
                'R' => 1,
                _ => throw new InvalidOperationException($"Invalid direction: {line}")
            };
            var length = line.AsSpan(1).ToInt();
            rotations.Add(direction * length);
        }
        return [.. rotations];
    }

    public override void Part1(int[] data, RunSettings settings)
    {
        int position = 50;
        int count = 0;
        foreach (var rotation in data)
        {
            position += rotation;
            while (position < 0)
                position += 100;
            while (position > 99)
                position -= 100;
            if (position == 0)
                ++count;
        }
        Console.WriteLine($"The dial points at 0 a total of {count} times.");
    }

    public override void Part2(int[] data, RunSettings settings)
    {
        int position = 50;
        int count = 0;
        foreach (var (num, rotation) in data.Index())
        {
            var newPosition = position + rotation;
            if (newPosition == 0)
            {
                if (settings.Verbose)
                    Console.WriteLine($"Arrived at 0 from {position} to {newPosition}");
                ++count;
            }
            else if (newPosition < 0)
            {
                if (settings.Verbose)
                    Console.WriteLine($"Rollover negative {position} to {newPosition}");
                count += (newPosition / -100) + (position != 0 ? 1 : 0);
                newPosition = (100 + (newPosition % 100)) % 100;
            }
            else if (newPosition > 99)
            {
                if (settings.Verbose)
                    Console.WriteLine($"Rollover positive {position} to {newPosition}");
                count += newPosition / 100;
                newPosition %= 100;
            }
            position = newPosition;
        }
        Console.WriteLine($"The dial passed 0 a total of {count} times.");
    }
}
