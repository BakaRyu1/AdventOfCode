using AdventOfCode.Utils;
using System.Buffers;

namespace AdventOfCode._2024;

internal class Day06 : DayRunner<Day06.Data>
{
    private static readonly Dictionary<char, Direction> GuardDirections = new()
    {
        { '^', Direction.North },
        { 'v', Direction.South },
        { '>', Direction.East },
        { '<', Direction.West }
    };
    public struct Data
    {
        public Array2D<bool> Map;
        public Position Start;
        public Direction StartDirection;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var map = new List<bool[]>();
        var startX = -1;
        var startY = -1;
        Direction startDirection = Direction.North;
        foreach (var (y, line) in lines.Index())
        {
            var row = line.Select((ch, x) =>
            {
                switch (ch)
                {
                    case '.':
                        return false;
                    case '#':
                        return true;
                    case '^':
                    case '>':
                    case 'v':
                    case '<':
                        if (startX >= 0)
                        {
                            Console.Error.WriteLine("Found multiple start position: " + line);
                            throw new InvalidOperationException();
                        }
                        startX = x;
                        startY = y;
                        startDirection = GuardDirections[ch];
                        return false;
                    default:
                        Console.Error.WriteLine("Unknown map element: " + line);
                        throw new InvalidOperationException();
                }
            }).ToArray();
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
        return new()
        {
            Map = Array2D<bool>.From(map),
            Start = new(startX, startY),
            StartDirection = startDirection
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var visited = Array2D<bool>.FromSize(data.Map, false);
        TraceGuardWalk(data, data.Start, data.StartDirection, null, (position, direction, _) =>
        {
            visited[position] = true;
            return true;
        });
        var totalDistinctPositions = visited.Data.Count(pos => pos);
        if (settings.Verbose)
        {
            PrintMap(data, (x, y) =>
            {
                var fgColor = ConsoleColor.White;
                var bgColor = ConsoleColor.Black;
                var ch = (char)0;
                if (visited[x, y])
                {
                    bgColor = ConsoleColor.Yellow;
                    fgColor = ConsoleColor.Black;
                }
                return (ch, fgColor, bgColor);
            });
            Console.WriteLine();
        }
        Console.WriteLine("Total distinct positions: " + totalDistinctPositions);
    }
    public override void Part2(Data data, RunSettings settings)
    {
        
        var knownObstacles = new HashSet<(Position, Direction)>();
        TraceGuardWalk(data, data.Start, data.StartDirection, obstacle: (position, direction) =>
        {
            knownObstacles.Add((position, direction));
            return true;
        });
        /*
        var obstaclesTest = Array2D<bool>.From(data.Map, false);
        var directions = Array2D<Direction>.From(data.Map, Direction.None);
        using (TimeUtils.MeasureTime())
        {
            MarkDirectionsBackward(data, directions, data.Start, data.StartDirection);
            TraceGuardWalk(data, data.Start, data.StartDirection, step: (position, direction, rotated) =>
            {
                if (rotated)
                {
                    var prevPosition = position + direction.Opposite();
                    directions[prevPosition] |= direction;
                    MarkDirectionsBackward(data, directions, position, direction);
                }
                var leftDirection = direction.RotateLeft();
                var leftPosition = position + leftDirection;
                if (leftPosition.IsInside(data.Map) && data.Map[leftPosition])
                    MarkDirectionsBackward(data, directions, position, leftDirection);
                directions[position] |= direction;
                return true;
            });
            TraceGuardWalk(data, data.Start, data.StartDirection, step: (position, direction, _) =>
            {
                var localDirections = directions[position];
                if (localDirections == Direction.None)
                    return true;
                var directionRight = direction.RotateRight();
                if (localDirections.HasFlag(directionRight))
                {
                    var obstaclePos = position + direction;
                    if (obstaclePos.IsInside(data.Map)
                        && !data.Map[obstaclePos]
                        && IsGuardLooping(data, position, direction, obstaclePos, knownObstacles)
                        && IsGuardLooping(data, data.Start, data.StartDirection, obstaclePos))
                    {
                        obstaclesTest[obstaclePos] = true;
                    }
                }
                return true;
            });
            var totalDistinctPositions = obstaclesTest.Data.Count(pos => pos);
            Console.WriteLine("Total distinct obstacles: " + totalDistinctPositions);
        }
        */
        var obstacles = Array2D<bool>.FromSize(data.Map, false);
        TraceGuardWalk(data, data.Start, data.StartDirection, step: (position, direction, _) =>
        {
            var obstaclePos = position;
            if (obstaclePos.IsInside(data.Map)
                && !data.Map[obstaclePos]
                && IsGuardLooping(data, position + direction.Opposite(), direction, obstaclePos, knownObstacles)
                && IsGuardLooping(data, data.Start, data.StartDirection, obstaclePos))
            {
                obstacles[obstaclePos] = true;
            }
            return true;
        });
        if (settings.Verbose)
        {
            PrintMap(data, (x, y) =>
            {
                var fgColor = ConsoleColor.White;
                var bgColor = ConsoleColor.Black;
                var ch = (char)0;
                /*var direction = directions[x, y];
                if (direction != Direction.None)
                {
                    ch = direction.AsPrintableChar();
                    var angles = Directions.All.Where(dir => direction.HasFlag(dir) && direction.HasFlag(dir.RotateRight()));
                    fgColor = angles.Any() ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                }
                if (!obstacles[x, y] && obstacles2[x, y])
                {
                    bgColor = ConsoleColor.Yellow;
                    fgColor = ConsoleColor.Black;
                }*/
                if (obstacles[x, y])
                {
                    bgColor = ConsoleColor.Yellow;
                    fgColor = ConsoleColor.Black;
                }
                return (ch, fgColor, bgColor);
            });
        }
        Console.WriteLine("Total distinct obstacles: " + obstacles.Data.Count(pos => pos));
    }

    /*
    private static void MarkDirectionsBackward(Data data, Array2D<Direction> directions, Position position, Direction direction)
    {
        var traces = new Queue<(Position, Direction)>();
        traces.Enqueue((position, direction));
        while (traces.Count > 0)
        {
            (position, direction) = traces.Dequeue();
            var opposite = direction.Opposite();
            var leftDirection = direction.RotateLeft();
            while (position.IsInside(data.Map) && !data.Map[position])
            {
                directions[position] |= direction;
                if (!directions[position].HasFlag(leftDirection))
                {
                    var leftPosition = position + leftDirection;
                    if (leftPosition.IsInside(data.Map) && data.Map[leftPosition])
                        traces.Enqueue((position, leftDirection));
                }
                position = position + opposite;
            }
        }
    }
    */
    private static void PrintMap(Data data, Func<int, int, (char, ConsoleColor, ConsoleColor)> obstacles)
    {
        for (int y = 0; y < data.Map.Height; ++y)
        {
            for (int x = 0; x < data.Map.Width; ++x)
            {
                var och = obstacles(x, y);
                if (och.Item1 != 0)
                {
                    Console.ForegroundColor = och.Item2;
                    Console.BackgroundColor = och.Item3;
                    Console.Write(och.Item1);
                }
                else
                {
                    Console.ResetColor();
                    Console.Write(data.Map[x, y] ? '#' : '.');
                }
            }
            Console.WriteLine();
        }
    }
    private static bool TraceGuardWalk(Data data, Position position, Direction direction,
        Position? extraObstacle = null,
        Func<Position, Direction, bool, bool>? step = null,
        Func<Position, Direction, bool>? obstacle = null)
    {
        var rotated = false;
        while (position.IsInside(data.Map))
        {
            if (step is not null && !(step!(position, direction, rotated)))
                return false;
            rotated = false;
            var nextDirection = direction;
            var nextPosition = position + direction;
            while (nextPosition.IsInside(data.Map) && (data.Map[nextPosition] || nextPosition.Equals(extraObstacle)))
            {
                if (obstacle is not null && !(obstacle!(nextPosition, nextDirection)))
                    return false;
                nextDirection = nextDirection.RotateRight();
                nextPosition = position + nextDirection;
                rotated = true;
            }
            direction = nextDirection;
            position = nextPosition;
        }
        return true;
    }
    private static bool IsGuardLooping(Data data, Position position, Direction direction, Position? extraObstacle, HashSet<(Position, Direction)>? knownObstacles = null)
    {
        var hits = new HashSet<(Position, Direction)>();
        return !TraceGuardWalk(data, position, direction, extraObstacle, (position, direction, rotated) =>
        {
            if (rotated)
            {
                var tuple = (position, direction);
                if (hits.Contains(tuple) || (knownObstacles?.Contains(tuple) ?? false))
                    return false;
                hits.Add(tuple);
            }
            return true;
        });
    }
}
