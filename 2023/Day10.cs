using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day10 : DayRunner<Day10.Data>
{
    public struct Data
    {
        public Array2D<Direction> Map;
        public Position Start;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var map = new List<Direction[]>();
        var startX = -1;
        var startY = -1;
        foreach (var (y, line) in lines.Index())
        {
            var row = line.Select(ch => PipeDatas[ch]).ToArray();
            var pos = line.IndexOf('S');
            if (pos >= 0)
            {
                startX = pos;
                startY = y;
            }
            map.Add(row);
        }
        if (!map.Skip(1).All(row => row.Length == map[0].Length))
        {
            Console.Error.WriteLine("Map is not rectangular!");
            throw new InvalidOperationException();
        }
        if (startX < 0)
        {
            Console.Error.WriteLine("Couldn't locate starting position!");
            throw new InvalidOperationException();
        }
        var startPos = new Position(startX, startY);
        var startPipe = Direction.None;
        var resultMap = Array2D<Direction>.From(map);
        foreach (var direction in Directions.All)
        {
            var neighborPos = startPos + direction;
            if (!neighborPos.IsInside(resultMap))
                continue;
            var neighbor = resultMap[neighborPos];
            if (neighbor.HasFlag(direction.Opposite()))
                startPipe |= direction;
        }
        resultMap[startX, startY] = startPipe;
        return new()
        {
            Map = resultMap,
            Start = startPos
        };
    }
    public override void Part1(Data data, RunSettings settings)
    {
        var distances = Array2D<int>.FromSize(data.Map, -1);

        var queue = new Queue<Position>();
        queue.Enqueue(data.Start);
        distances[data.Start] = 0;
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            var pipe = data.Map[position];
            var distance = distances[position];
            foreach (var direction in pipe.Separate())
            {
                var neighbor = position + direction;
                if (!neighbor.IsInside(data.Map))
                    continue;
                if (distances[neighbor] >= 0)
                    continue;
                distances[neighbor] = distance + 1;
                queue.Enqueue(neighbor);
            }
        }
        var farthest = distances.Data.Max();
        Console.WriteLine("The farthest distance is " + farthest);
    }
    public override void Part2(Data data, RunSettings settings)
    {
        var mainLoop = Array2D<bool>.FromSize(data.Map, false);

        var queue = new Queue<Position>();
        queue.Enqueue(data.Start);
        mainLoop[data.Start] = true;
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            var pipe = data.Map[position];
            foreach (var direction in pipe.Separate())
            {
                var neighbor = position + direction;
                if (!neighbor.IsInside(data.Map))
                    continue;
                if (mainLoop[neighbor])
                    continue;
                mainLoop[neighbor] = true;
                queue.Enqueue(neighbor);
            }
        }
        var count = 0;
        var insideMap = Array2D<bool>.FromSize(data.Map, false);
        for (var y = 0; y < data.Map.Height; ++y)
        {
            var insideNorth = false;
            var insideSouth = false;
            for (var x = 0; x < data.Map.Width; ++x)
            {
                var pipe = data.Map[x, y];
                if (mainLoop[x, y])
                {
                    if (pipe.HasFlag(Direction.North))
                        insideNorth = !insideNorth;
                    if (pipe.HasFlag(Direction.South))
                        insideSouth = !insideSouth;
                }
                if (insideNorth && insideSouth && !mainLoop[x, y])
                {
                    insideMap[x, y] = true;
                    ++count;
                }
            }
        }
        if (settings.Verbose)
        {
            for (var y = 0; y < data.Map.Height; ++y)
            {
                for (var x = 0; x < data.Map.Width; ++x)
                {
                    var pipe = data.Map[x, y];
                    if (insideMap[x, y])
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else if (mainLoop[x, y])
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                        Console.ResetColor();
                    Console.Write(pipe.AsPrintableChar());
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        Console.WriteLine("Total enclosed tiles is " + count);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day10), settings.Example ? "day10-example1.txt" : "day10-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day01), "day10-example2.txt") : null);
    }

    private static readonly Dictionary<char, Direction> PipeDatas = new()
    {
        { '|', Direction.North | Direction.South },
        { '-', Direction.West | Direction.East },
        { 'L', Direction.North | Direction.East },
        { 'J', Direction.North | Direction.West },
        { '7', Direction.South | Direction.West },
        { 'F', Direction.South | Direction.East },
        { '.', Direction.None },
        { 'S', Direction.None }
    };
}
