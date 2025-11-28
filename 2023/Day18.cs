using AdventOfCode.Utils;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day18 : DayRunner<Day18.Instruction[]>
{
    public struct Instruction
    {
        public Direction Direction;
        public int Count;
        public uint Color;
    }

    public override Instruction[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var instructions = new List<Instruction>();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var match = InstructionPattern().Match(line);
            if (!match.Success)
            {
                Console.Error.WriteLine("Couldn't parse line: " + line);
                throw new InvalidOperationException();
            }
            instructions.Add(new()
            {
                Direction = match.Groups["dir"].ValueSpan switch
                {
                    "U" => Direction.North,
                    "R" => Direction.East,
                    "D" => Direction.South,
                    "L" => Direction.West,
                    _ => throw new InvalidOperationException()
                },
                Count = match.Groups["count"].ValueSpan.ToInt(),
                Color = uint.Parse(match.Groups["color"].ValueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            });
        }
        return [.. instructions];
    }

    public override void Part1(Instruction[] data, RunSettings settings)
    {
        var (min, max) = GetBounds(data);
        var size = max - min + (1, 1);
        var map = new Array2D<bool>(size.X, size.Y);
        Paint(data, map, min);
        Fill(data, map, min);
        PrintMap(map);
        var count = map.Data.Count(val => val);
        Console.WriteLine("Total cubic meters is " + count);
        Console.WriteLine("Other count is " + CountSpaces(GetScanlineInfos(data, min, size.Y)));
    }

    public override void Part2(Instruction[] data, RunSettings settings)
    {
        var instructions = GetRealInstructions(data);
        var (min, max) = GetBounds(instructions);
        var size = max - min + (1, 1);
        Console.WriteLine($"Size is {size.X}x{size.Y} ({min.X}..{min.Y} to {max.X}..{max.Y})");
        var info = GetScanlineInfos(instructions, min, size.Y);
        Console.WriteLine("Other count is " + CountSpaces(info));

    }

    [GeneratedRegex(@"^\s*(?<dir>[URDL])\s+(?<count>\d+)\s+\(\s*#(?<color>[a-f0-9]{6})\s*\)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex InstructionPattern();

    private long CountSpaces(ScanlineInfo[] scanlines)
    {
        var count = 0L;
        var activeLines = new HashSet<int>();
        foreach (var line in scanlines)
        {
            var lineCount = 0L;
            var info = line.VerticalSegments;
            foreach (var pair in info.Where(x => activeLines.Contains(x)).Chunk(2))
            {
                if (pair.Length != 2)
                    continue;
                lineCount += pair[1] - pair[0] + 1;
            }
            foreach (var segment in line.HorizontalSegments)
            {
                var active1 = activeLines.Contains(segment.x1);
                var active2 = activeLines.Contains(segment.x2);
                if (active1)
                    continue;
                lineCount += (segment.x2 - segment.x1) + (active2 ? 0 : 1);
            }
            count += lineCount;

            activeLines.Clear();
            foreach (var x in line.VerticalSegments)
                activeLines.Add(x);
        }
        return count;
    }

    private static (Position, Position) GetBounds(Instruction[] instructions)
    {
        var min = new Position(0, 0);
        var max = new Position(0, 0);
        var cur = new Position(0, 0);
        foreach (var instruction in instructions)
        {
            cur += (Position)instruction.Direction * instruction.Count;
            min = min.Min(cur);
            max = max.Max(cur);
        }
        return (min, max);
    }

    private static void Paint(Instruction[] instructions, Array2D<bool> map, Position shift)
    {
        var cur = new Position(0, 0);
        foreach (var instruction in instructions)
        {
            var delta = (Position)instruction.Direction;
            for (var i = 0; i < instruction.Count; ++i)
            {
                map[cur - shift] = true;
                cur += delta;
            }
        }
    }

    private struct ScanlineInfo
    {
        public int[] VerticalSegments;
        public (int x1, int x2)[] HorizontalSegments;
    }

    private static Direction[] RealDirections = [Direction.East, Direction.South, Direction.West, Direction.North];

    private static Instruction[] GetRealInstructions(Instruction[] instructions)
    {
        return instructions.Select(i => new Instruction()
        {
            Direction = RealDirections[i.Color & 0xF],
            Count = (int)(i.Color >> 8),
            Color = i.Color
        }).ToArray();
    }

    private static ScanlineInfo[] GetScanlineInfos(Instruction[] instructions, Position shift, int height)
    {
        var segments = new (SortedSet<int> v, SortedSet<(int, int)> h)[height];
        for (var i = 0; i < segments.Length; ++i)
            segments[i] = ([], []);
        var cur = -shift;
        foreach (var instruction in instructions)
        {
            var delta = (Position)instruction.Direction;
            if (instruction.Direction.IsVertical())
            {
                for (var i = 0; i < instruction.Count; ++i)
                {
                    segments[cur.Y].v.Add(cur.X);
                    cur += delta;
                }
                segments[cur.Y].v.Add(cur.X);
            }
            else
            {
                segments[cur.Y].h.Add(delta.X > 0 ? (cur.X, cur.X + instruction.Count) : (cur.X - instruction.Count, cur.X));
                cur += delta * instruction.Count;
            }
        }
        return  segments.Select(scanline => new ScanlineInfo() {
            VerticalSegments = [.. scanline.v],
            HorizontalSegments = [.. scanline.h]
        }).ToArray();
    }

    private static void Fill(Array2D<bool> map, ScanlineInfo[] scanlines)
    {
        var activeLines = new SortedSet<int>();
        for (var y = 0; y < map.Height; ++y)
        {
            var info = scanlines[y].VerticalSegments;
            foreach (var pair in info.Where(x => activeLines.Contains(x)).Chunk(2))
            {
                if (pair.Length != 2)
                    continue;
                for (var x = pair[0]; x <= pair[1]; ++x)
                    map[x, y] = true;
            }
            activeLines.Clear();
            foreach (var x in info)
                activeLines.Add(x);
        }
    }

    private static void Fill(Instruction[] instructions, Array2D<bool> map, Position shift)
    {
        var verticalSegments = GetScanlineInfos(instructions, shift, map.Height);
        Fill(map, verticalSegments);
    }

    private static void PrintMap(Array2D<bool> map)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                Console.Write(map[x, y] ? '#' : '.');
            }
            Console.WriteLine();
        }
    }
}
