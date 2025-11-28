using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day06 : DayRunner<Day06.Data[]>
{
    public enum Command
    {
        TurnOn,
        Toggle,
        TurnOff
    }
    public struct Data
    {
        public Command Command;
        public Position TopLeft;
        public Position BottomRight;
    }
    public override Data[] Parse(FileReference file)
    {
        var data = new List<Data>();
        foreach (var line in file.GetLines())
        {
            var matcher = InstructionPattern().Match(line);
            if (!matcher.Success)
                throw new InvalidOperationException($"Failed to parse instruction: {line}");
            var command = matcher.Groups["command"].ValueSpan switch
            {
                "turn on" => Command.TurnOn,
                "toggle" => Command.Toggle,
                "turn off" => Command.TurnOff,
                _ => throw new InvalidOperationException($"Unknown command: {matcher.Groups["command"].ValueSpan}")
            };
            var x1 = matcher.Groups["x1"].ValueSpan.ToInt();
            var y1 = matcher.Groups["y1"].ValueSpan.ToInt();
            var x2 = matcher.Groups["x2"].ValueSpan.ToInt();
            var y2 = matcher.Groups["y2"].ValueSpan.ToInt();
            data.Add(new()
            {
                Command = command,
                TopLeft = new(x1, y1),
                BottomRight = new(x2, y2)
            });
        }
        return [.. data];
    }

    public override void Part1(Data[] data, RunSettings settings)
    {
        var map = new Array2D<bool>(1000, 1000);
        foreach (var instruction in data)
        {
            switch (instruction.Command)
            {
                case Command.TurnOn:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            map[x, y] = true;
                    }
                    break;
                case Command.Toggle:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            map[x, y] = !map[x, y];
                    }
                    break;
                case Command.TurnOff:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            map[x, y] = false;
                    }
                    break;
            }
        }
        var count = map.Data.Count(b => b);
        Console.WriteLine($"There are {count} lights turned on.");
    }

    public override void Part2(Data[] data, RunSettings settings)
    {
        var map = new Array2D<int>(1000, 1000);
        foreach (var instruction in data)
        {
            switch (instruction.Command)
            {
                case Command.TurnOn:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            ++map[x, y];
                    }
                    break;
                case Command.Toggle:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            map[x, y] += 2;
                    }
                    break;
                case Command.TurnOff:
                    for (var y = instruction.TopLeft.Y; y <= instruction.BottomRight.Y; ++y)
                    {
                        for (var x = instruction.TopLeft.X; x <= instruction.BottomRight.X; ++x)
                            map[x, y] = Math.Max(map[x, y] - 1, 0);
                    }
                    break;
            }
        }
        var count = map.Data.Sum();
        Console.WriteLine($"The total brightness is {count}.");
    }

    [GeneratedRegex(@"^(?<command>turn on|toggle|turn off) (?<x1>\d+),(?<y1>\d+) through (?<x2>\d+),(?<y2>\d+)$")]
    private partial Regex InstructionPattern();
}
