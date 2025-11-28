using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day16 : DayRunner<Day16.Data>
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
        var mapData = GenerateData(data);
        var bestScore = mapData.GetDatas(data.End).MinBy(pair => pair.data.Score);
        if (settings.Verbose)
        {
            var positions = GetPositions(data, mapData);
            PrintMap(data, positions);
        }
        Console.WriteLine("Best score is " + bestScore.data.Score);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var mapData = GenerateData(data);
        var positions = GetPositions(data, mapData);
        if (settings.Verbose && !settings.Part1)
            PrintMap(data, positions);
        Console.WriteLine("Count of tiles of best paths is " + positions.Count);
    }

    private static DirectionalArray2D<NodeData> GenerateData(Data data)
    {
        var mapData = new DirectionalArray2D<NodeData>(data.Map.Width, data.Map.Height);
        var queue = new Queue<(Position, Direction, int, Direction)>();
        queue.Enqueue((data.Start, Direction.East, 0, Direction.East));
        while (queue.Count > 0)
        {
            var (position, direction, score, prevDirection) = queue.Dequeue();
            var data_ = mapData[position, direction];
            if (data_.Score <= score)
            {
                if (data_.Score == score)
                    data_.Sources |= prevDirection;
                continue;
            }
            data_.Score = score;
            data_.Sources = prevDirection;
            if (position == data.End)
            {
                continue;
            }
            var forwardPos = position + direction;
            if (forwardPos.IsInside(data.Map) && !data.Map[forwardPos])
                queue.Enqueue((forwardPos, direction, score + 1, direction));
            var leftDir = direction.RotateLeft();
            queue.Enqueue((position, leftDir, score + 1000, prevDirection));
            var rightDir = direction.RotateRight();
            queue.Enqueue((position, rightDir, score + 1000, prevDirection));
        }
        return mapData;
    }

    private static HashSet<Position> GetPositions(Data data, DirectionalArray2D<NodeData> mapData)
    {
        var queue = new Queue<(Position, Direction)>();
        var bestScore = mapData.GetDatas(data.End).Min(pair => pair.data.Score);
        var lastDirs = mapData.GetDatas(data.End).Where(pair => pair.data.Score == bestScore).Select(pair => pair.direction);
        foreach (var dir in lastDirs)
            queue.Enqueue((data.End, dir));
        var positions = new HashSet<Position>();
        while (queue.Count > 0)
        {
            var (position, direction) = queue.Dequeue();
            positions.Add(position);
            if (position == data.Start)
                continue;
            var nodeData = mapData[position, direction];
            foreach (var dir in nodeData.Sources.Separate())
            {
                var next = position + dir.Opposite();
                queue.Enqueue(new(next, dir));
            }
        }
        return positions;
    }

    private static void PrintMap(Data data, HashSet<Position> path)
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

    private class NodeData()
    {
        public int Score = int.MaxValue;
        public Direction Sources = Direction.None;
    }
}
