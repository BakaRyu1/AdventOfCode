using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day14 : DayRunner<Day14.RobotInfo[]>
{
    public struct RobotInfo
    {
        public Position Start;
        public Position Velocity;
    }
    public override RobotInfo[] Parse(FileReference file)
    {
        var robots = new List<RobotInfo>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var match = LinePattern().Match(line);
            if (!match.Success)
            {
                Console.Error.WriteLine("Could't parse line: " + line);
                throw new InvalidOperationException();
            }
            robots.Add(new()
            {
                Start = new(
                    match.Groups["sx"].ValueSpan.ToInt(),
                    match.Groups["sy"].ValueSpan.ToInt()
                ),
                Velocity = new(
                    match.Groups["vx"].ValueSpan.ToInt(),
                    match.Groups["vy"].ValueSpan.ToInt()
                )
            });
        }
        return [.. robots];
    }

    public override void Part1(RobotInfo[] data, RunSettings settings)
    {
        var width = settings.Example ? 11 : 101;
        var height = settings.Example ? 7 : 103;
        var middleX = width / 2;
        var middleY = height / 2;
        var quadrants = new int[4];
        foreach (var robot in data)
        {
            var position = robot.Start + robot.Velocity * 100;
            ClampPosition(ref position, width, height);
            if (position.X < middleX)
            {
                if (position.Y < middleY)
                    ++quadrants[0];
                else if (position.Y > middleY)
                    ++quadrants[1];
            }
            else if (position.X > middleX)
            {
                if (position.Y < middleY)
                    ++quadrants[2];
                else if (position.Y > middleY)
                    ++quadrants[3];
            }
        }
        if (settings.Verbose)
            Console.WriteLine("Quadrants: " + string.Join(' ', quadrants));
        var product = quadrants.Aggregate((a, b) => a * b);
        Console.WriteLine("The product of quadrants is " + product);
    }

    public override void Part2(RobotInfo[] data, RunSettings settings)
    {
        var width = settings.Example ? 11 : 101;
        var height = settings.Example ? 7 : 103;
        var bathroom = new Array2D<int>(width, height);
        var positions = new Position[data.Length];
        foreach (var (i, robot) in data.Index())
        {
            ++bathroom[robot.Start];
            positions[i] = robot.Start;
        }
        var seconds = 0;
        do
        {
            foreach (var (i, robot) in data.Index())
            {
                --bathroom[positions[i]];
                positions[i] += robot.Velocity;
                ClampPosition(ref positions[i], width, height);
                ++bathroom[positions[i]];
            }
            ++seconds;
        } while (bathroom.Data.Any(count => count >= 2));
        if (settings.Verbose)
        {
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    Console.Write(bathroom[x, y] > 0 ? 'X' : ' ');
                }
                Console.WriteLine();
            }
        }
        Console.WriteLine("Time required for drawing: " + seconds + "s");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day14), settings.Example ? "day14-example.txt" : "day14-input.txt");
    }

    [GeneratedRegex(@"^\s*p\s*=\s*(?<sx>-?\d+)\s*,\s*(?<sy>-?\d+)\s+v\s*=\s*(?<vx>-?\d+)\s*,\s*(?<vy>-?\d+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex LinePattern();

    private static void ClampPosition(ref Position position, int width, int height)
    {
        if (position.X < 0 || position.X >= width)
        {
            position.X %= width;
            if (position.X < 0)
                position.X += width;
        }
        if (position.Y < 0 || position.Y >= height)
        {
            position.Y %= height;
            if (position.Y < 0)
                position.Y += height;
        }
    }
}
