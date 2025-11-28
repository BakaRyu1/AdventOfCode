using AdventOfCode.Utils;

namespace AdventOfCode._2015;

internal class Day02 : DayRunner<Day02.BoxDimensions[]>
{
    public struct BoxDimensions
    {
        public int Length;
        public int Width;
        public int Height;
    }
    public override BoxDimensions[] Parse(FileReference file)
    {
        var boxes = new List<BoxDimensions>();
        foreach (var line in file.GetLines())
        {
            var ints = line.AsSpan().SplitInts('x').ToList();
            if (ints.Count != 3)
                throw new InvalidOperationException($"Invalid dimensions for box {line}");
            boxes.Add(new()
            {
                Length = ints[0],
                Width = ints[1],
                Height = ints[2]
            });
        }
        return [.. boxes];
    }

    public override void Part1(BoxDimensions[] data, RunSettings settings)
    {
        var totalWrappingPaper = 0;
        foreach (var box in data)
        {
            var side1 = box.Length * box.Width;
            var side2 = box.Width * box.Height;
            var side3 = box.Height * box.Length;
            var smallestSide = Math.Min(Math.Min(side1, side2), side3);
            var wrappingPaper = 2 * side1 + 2 * side2 + 2 * side3 + smallestSide;
            if (settings.Verbose)
                Console.WriteLine($"Wrapping paper for box {box.Length}x{box.Width}x{box.Height} is {wrappingPaper}");
            totalWrappingPaper += wrappingPaper;
        }
        Console.WriteLine($"Total wrapping paper is {totalWrappingPaper}");
    }

    public override void Part2(BoxDimensions[] data, RunSettings settings)
    {
        var totalRibbon = 0;
        foreach (var box in data)
        {
            var side1 = box.Length + box.Width;
            var side2 = box.Width + box.Height;
            var side3 = box.Height + box.Length;
            var smallestSide = Math.Min(Math.Min(side1, side2), side3);
            var bow = box.Length * box.Width * box.Height;
            var ribbon = 2 * smallestSide + bow;
            if (settings.Verbose)
                Console.WriteLine($"Ribbon for box {box.Length}x{box.Width}x{box.Height} is {ribbon}");
            totalRibbon += ribbon;
        }
        Console.WriteLine($"Total ribbon is {totalRibbon}");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day02), settings.Example ? "day02-example.txt" : "day02-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day02), "day02-example2.txt") : null);
    }
}
