using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day15 : DayRunner<Day15.Data>
{
    public enum State
    {
        Empty,
        Box,
        Wall,
        BoxLeft,
        BoxRight
    }

    public struct Data
    {
        public Array2D<State> Map;
        public Position Start;
        public Direction[] Movements;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var rows = new List<State[]>();
        var position = new Position(-1, -1);
        var movements = new List<Direction>();
        var insideMap = true;
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
            {
                if (rows.Count > 0)
                    insideMap = false;
                continue;
            }
            if (insideMap)
            {
                var row = line.Select((ch, x) =>
                {
                    switch (ch)
                    {
                        case '#':
                            return State.Wall;
                        case '.':
                            return State.Empty;
                        case 'O':
                            return State.Box;
                        case '@':
                            if (position.X >= 0)
                            {
                                Console.Error.WriteLine("Found multiple starting positions:" + line);
                                throw new InvalidOperationException();
                            }
                            position.X = x;
                            position.Y = rows.Count;
                            return State.Empty;
                        default:
                            Console.Error.WriteLine("Invalid map character: " + ch);
                            throw new InvalidOperationException();
                    }
                }).ToArray();
                if (rows.Count > 0 && rows[0].Length != row.Length)
                {
                    Console.Error.WriteLine("Line length doesn't match: " + line);
                    throw new InvalidOperationException();
                }
                rows.Add(row);
            }
            else
            {
                movements.AddRange(line.Select(ch =>
                {
                    switch (ch)
                    {
                        case '^': return Direction.North;
                        case '>': return Direction.East;
                        case 'v': return Direction.South;
                        case '<': return Direction.West;
                        default:
                            Console.Error.WriteLine("Invalid movement character: " + ch);
                            throw new InvalidOperationException();
                    }
                }));
            }
        }
        if (rows.Count == 0)
        {
            Console.Error.WriteLine("Couldn't parse map");
            throw new InvalidOperationException();
        }
        if (position.X < 0)
        {
            Console.Error.Write("Couldn't find starting position");
            throw new InvalidOperationException();
        }
        if (movements.Count == 0)
        {
            Console.Error.WriteLine("Couldn't parse movements");
            throw new InvalidOperationException();
        }
        return new()
        {
            Map = Array2D<State>.From(rows),
            Start = position,
            Movements = [.. movements]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var map = Array2D<State>.From(data.Map);
        var robotPosition = RunRobot(map, data.Start, data.Movements);
        var sum = CalculateGps(map, State.Box);
        if (settings.Verbose)
            PrintMap(map, robotPosition);
        Console.WriteLine("Sum of gps coordinates is " + sum);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var map = ExpandMap(data.Map);
        var robotPosition = RunRobot(map, data.Start + (data.Start.X, 0), data.Movements);
        var sum = CalculateGps(map, State.BoxLeft);
        if (settings.Verbose)
            PrintMap(map, robotPosition);
        Console.WriteLine("Sum of gps coordinates (expanded) is " + sum);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day15), settings.Example ? "day15-example.txt" : "day15-input.txt");
    }

    private static void PrintMap(Array2D<State> map, Position robotPosition)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                char ch;
                if (robotPosition == (x, y))
                    ch = '@';
                else
                {
                    ch = map[x, y] switch
                    {
                        State.Empty => '.',
                        State.Box => 'O',
                        State.BoxLeft => '[',
                        State.BoxRight => ']',
                        State.Wall => '#',
                        _ => throw new InvalidOperationException()
                    };
                }
                Console.Write(ch);
            }
            Console.WriteLine();
        }
    }
    private static Position RunRobot(Array2D<State> map, Position position, IEnumerable<Direction> movements)
    {
        foreach (var direction in movements)
        {
            var next = position + direction;
            if (TryPush(map, next, direction))
                position = next;
        }
        return position;
    }
    private static int CalculateGps(Array2D<State> map, State state)
    {
        var sum = 0;
        foreach (var position in map.EnumeratePositions())
        {
            if (map[position] == state)
            {
                var gpsCoordinate = position.X + 100 * position.Y;
                sum += gpsCoordinate;
            }
        }
        return sum;
    }
    private static Array2D<State> ExpandMap(Array2D<State> map)
    {
        return new Array2D<State>(map.Data.SelectMany<State, State>(state => state switch
        {
            State.Empty => [State.Empty, State.Empty],
            State.Box => [State.BoxLeft, State.BoxRight],
            State.Wall => [State.Wall, State.Wall],
            _ => throw new InvalidOperationException()
        }), map.Width * 2, map.Height);
    }

    private static bool TryPush(Array2D<State> map, Position position, Direction direction, bool simulate = false)
    {
        var state = map[position];
        switch (state)
        {
            case State.Empty:
                return true;
            case State.Box:
                var next = position + direction;
                if (TryPush(map, next, direction))
                {
                    if (!simulate)
                    {
                        map[position] = State.Empty;
                        map[next] = State.Box;
                    }
                    return true;
                }
                return false;
            case State.BoxLeft:
            case State.BoxRight:
                var position2 = position + (state == State.BoxLeft ? Direction.East : Direction.West);
                var state2 = map[position2];
                if (state2 != State.BoxLeft && state2 != State.BoxRight)
                    throw new InvalidOperationException();
                if (state == state2)
                    throw new InvalidOperationException();
                if (direction.IsVertical())
                {
                    var next1 = position + direction;
                    var next2 = position2 + direction;
                    if (TryPush(map, next1, direction, true) && TryPush(map, next2, direction, true))
                    {
                        if (!simulate)
                        {
                            if (!TryPush(map, next1, direction, false)
                                || !TryPush(map, next2, direction, false))
                                throw new InvalidOperationException();
                            map[next1] = state;
                            map[next2] = state2;
                            map[position] = State.Empty;
                            map[position2] = State.Empty;
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    if ((state == State.BoxLeft && direction != Direction.East)
                        || (state == State.BoxRight && direction != Direction.West))
                        throw new InvalidOperationException();
                    var next2 = position2 + direction;
                    if (TryPush(map, next2, direction, simulate))
                    {
                        if (!simulate)
                        {
                            map[next2] = state2;
                            map[position2] = state;
                            map[position] = State.Empty;
                        }
                        return true;
                    }
                }
                return false;
            case State.Wall:
                return false;
            default:
                throw new InvalidOperationException();
        }
    }
}
