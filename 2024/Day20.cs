using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day20 : DayRunner<Day20.Data>
{
    public struct Data
    {
        public Array2D<bool> Map;
        public Position Start;
        public Position End;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetRectangle();
        var start = new Position(-1, -1);
        var end = new Position(-1, -1);
        var rows = new List<bool[]>();
        foreach (var line in lines)
        {
            rows.Add(line.Select((ch, x) =>
            {
                switch (ch)
                {
                    case '#': return true;
                    case '.': return false;
                    case 'S':
                        if (start.X >= 0)
                        {
                            Console.Error.WriteLine("Found multiple starts: " + line);
                            throw new InvalidOperationException();
                        }
                        start = (x, rows.Count);
                        return false;
                    case 'E':
                        if (end.X >= 0)
                        {
                            Console.Error.WriteLine("Found multiple ends: " + line);
                            throw new InvalidOperationException();
                        }
                        end = (x, rows.Count);
                        return false;
                    default:
                        Console.Error.WriteLine("Unknown map character: " + ch);
                        throw new InvalidOperationException();
                }
            }).ToArray());
        }
        if (start.X < 0)
        {
            Console.Error.WriteLine("Couldn't find start position.");
            throw new InvalidOperationException();
        }
        if (end.X < 0)
        {
            Console.Error.WriteLine("Couldn't find end position.");
            throw new InvalidOperationException();
        }
        return new()
        {
            Map = Array2D<bool>.From(rows),
            Start = start,
            End = end
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var mapData = GenerateData(data, data.Start);
        var positions = GetPositions(data, mapData);
        var minDelta = settings.Example ? 0 : 100;
        var shortcuts = FindCheatShortcuts(data, positions, mapData, 2, minDelta);
        Console.WriteLine("Cheats that saves 100 picoseconds are " + shortcuts.Count);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var mapData = GenerateData(data, data.Start);
        var positions = GetPositions(data, mapData);
        var minDelta = settings.Example ? 0 : 100;
        var shortcuts = FindCheatShortcuts(data, positions, mapData, 20, minDelta);
        Console.WriteLine("Cheats that saves 100 picoseconds (with 20 picoseconds cheats) are " + shortcuts.Count);
    }

    private static Array2D<NodeData> GenerateData(Data data, Position start)
    {
        var mapData = new Array2D<NodeData>(data.Map.Width, data.Map.Height);
        var queue = new Queue<(Position, Direction, int)>();
        queue.Enqueue((start, Direction.None, 0));
        while (queue.Count > 0)
        {
            var (position, direction, score) = queue.Dequeue();
            var data_ = mapData[position];
            if (data_ is null)
                mapData[position] = data_ = new();
            if (data_.Score <= score)
            {
                if (data_.Score == score)
                    data_.Sources |= direction;
                continue;
            }
            data_.Score = score;
            data_.Sources = direction;
            if (position == data.End)
                continue;
            foreach (var nextDirection in Directions.All)
            {
                var neighbor = position + nextDirection;
                if (neighbor.IsInside(data.Map) && !data.Map[neighbor])
                    queue.Enqueue((neighbor, nextDirection, score + 1));
            }
        }
        return mapData;
    }

    private static Dictionary<Position, int> GetPositions(Data data, Array2D<NodeData> mapData)
    {
        var queue = new Queue<Position>();
        queue.Enqueue(data.End);
        var positions = new Dictionary<Position, int>();
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            var nodeData = mapData[position];
            positions[position] = nodeData.Score;
            if (position == data.Start)
                continue;
            
            foreach (var dir in nodeData.Sources.Separate())
            {
                var next = position + dir.Opposite();
                queue.Enqueue(next);
            }
        }
        return positions;
    }

    private static List<(Position, Position)> FindCheatShortcuts(Data data, Dictionary<Position, int> positions, Array2D<NodeData> mapData, int range, int minDelta)
    {
        var shortcuts = new List<(Position, Position)>();
        foreach (var (position, score) in positions)
        {
            for (var dy = -range; dy <= range; ++dy)
            {
                for (var dx = -range; dx <= range; ++dx)
                {
                    var distance = Math.Abs(dx) + Math.Abs(dy);
                    if (distance == 0 || distance > range)
                        continue;
                    var cheatEnd = position + (dx, dy);
                    if (!cheatEnd.IsInside(data.Map) || data.Map[cheatEnd])
                        continue;
                    var cheatScore = score + distance;
                    var localScore = mapData[cheatEnd]?.Score ?? -1;
                    if (cheatScore < localScore)
                    {
                        if ((localScore - cheatScore) >= minDelta)
                            shortcuts.Add((position, cheatEnd));
                    }
                }
            }
        }
        return shortcuts;
    }

    private static void PrintMap(Data data, ICollection<Position> path, ICollection<Position> cheatPositions)
    {
        for (var y = 0; y < data.Map.Height; ++y)
        {
            for (var x = 0; x < data.Map.Width; ++x)
            {
                if (path.Contains((x, y)))
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (cheatPositions.Contains((x, y)))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                    Console.ResetColor();
                if (data.Start == (x, y))
                    Console.Write('S');
                else if (data.End == (x, y))
                    Console.Write('E');
                else
                    Console.Write(data.Map[x, y] ? '#' : '.');
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private class NodeData(int score = int.MaxValue, Direction sources = Direction.None)
    {
        public int Score = score;
        public Direction Sources = sources;
    }
}
