using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day17 : DayRunner<Array2D<int>>
{
    public override Array2D<int> Parse(FileReference file)
    {
        var lines = file.GetRectangle();
        var rows = new List<int[]>();
        foreach (var line in lines)
        {
            rows.Add(line.Select(ch =>
            {
                if (ch < '0' || ch > '9')
                    throw new InvalidOperationException("Invalid character found: " + ch);
                return (int)(ch - '0');
            }).ToArray());
        }
        return Array2D<int>.From(rows);
    }

    public override void Part1(Array2D<int> data, RunSettings settings)
    {
        var mapData = GenerateData(data, 0, 3);
        var lastPos = new Position(data.Width - 1, data.Height - 1);
        var lastDir = mapData.GetDatas(lastPos)
            .MinBy(pair => pair.data.Heat)
            .direction;
        if (settings.Verbose)
        {
            var path = GetPath(mapData, lastPos, lastDir);
            PrintMap(data, path);
        }
        Console.WriteLine("Best (with crucible) heat is " + mapData[lastPos, lastDir].Heat);
    }

    public override void Part2(Array2D<int> data, RunSettings settings)
    {
        var mapData = GenerateData(data, 4, 10);
        var lastPos = new Position(data.Width - 1, data.Height - 1);
        var lastDir = mapData.GetDatas(lastPos)
            .MinBy(pair => pair.data.Heat)
            .direction;
        if (settings.Verbose)
        {
            var path = GetPath(mapData, lastPos, lastDir);
            PrintMap(data, path);
        }
        Console.WriteLine("Best heat (with ultra crucible) is " + mapData[lastPos, lastDir].Heat);
    }

    private class NodeData()
    {
        public bool Queued = false;
        public int Heat = int.MaxValue;
        public (Position pos, Direction dir) Source = ((0, 0), Direction.None);
        public int Steps = 0;
    }

    private static DirectionalArray2D<NodeData> GenerateData(Array2D<int> map, int minTurn, int maxDistance)
    {
        var mapData = new DirectionalArray2D<NodeData>(map.Width, map.Height);
        var queue = new Queue<(Position, Direction)>();
        var lastPos = new Position(map.Width - 1, map.Height - 1);
        foreach (var dir in new Direction[] { Direction.East, Direction.South })
        {
            var node = (pos: (0, 0), dir);
            var data = mapData[node.pos, node.dir];
            data.Heat = 0;
            data.Queued = true;
            queue.Enqueue(node);
        }
        while (queue.Count > 0)
        {
            var start = queue.Dequeue();
            var (position, direction) = start;
            var startData = mapData[position, direction];
            startData.Queued = false;
            var delta = (Position)direction;
            var leftDir = direction.RotateLeft();
            var rightDir = direction.RotateRight();
            var heat = startData.Heat;
            for (var distance = 1; distance <= maxDistance; ++distance)
            {
                position += delta;
                if (!position.IsInside(map))
                    break;
                heat += map[position];
                if (distance < minTurn)
                {
                    if (position == lastPos)
                    {
                        var forwardData = mapData[position, direction];
                        if (forwardData.Heat > heat)
                        {
                            forwardData.Heat = heat;
                            forwardData.Source = start;
                            forwardData.Steps = distance;
                        }
                    }
                }
                else
                {
                    var leftData = mapData[position, leftDir];
                    if (leftData.Heat > heat)
                    {
                        leftData.Heat = heat;
                        leftData.Source = start;
                        leftData.Steps = distance;
                        if (!leftData.Queued)
                        {
                            leftData.Queued = true;
                            queue.Enqueue((position, leftDir));
                        }
                    }
                    var rightData = mapData[position, rightDir];
                    if (rightData.Heat > heat)
                    {
                        rightData.Heat = heat;
                        rightData.Source = start;
                        rightData.Steps = distance;
                        if (!rightData.Queued)
                        {
                            rightData.Queued = true;
                            queue.Enqueue((position, rightDir));
                        }
                    }
                }
            }
        }
        return mapData;
    }

    private static List<Position> GetPath(DirectionalArray2D<NodeData> mapData, Position startPos, Direction startDir)
    {
        var path = new List<Position>();
        var prevNode = (pos: startPos, dir: startDir);
        while (prevNode.pos != (0, 0))
        {
            var info = mapData[prevNode.pos, prevNode.dir];
            path.Add(prevNode.pos);
            var position = info.Source.pos;
            var delta = (Position)info.Source.dir;
            for (var i = 1; i < info.Steps; ++i)
            {
                position += delta;
                path.Insert(path.Count - i + 1, position);
            }
            prevNode = info.Source;
        }
        path.Reverse();
        return path;
    }

    private static void PrintMap(Array2D<int> map, List<Position> path)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (path.Contains((x, y)))
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                    Console.ResetColor();
                Console.Write(map[x, y]);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
