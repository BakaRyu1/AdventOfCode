using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day16 : DayRunner<Array2D<Day16.State>>
{
    public enum State
    {
        Empty,
        MirrorBLTR,
        MirrorTLBR,
        HorizontalSplitter,
        VerticalSplitter
    }

    public override Array2D<State> Parse(FileReference file)
    {
        var rows = new List<State[]>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var row = line.Select(ch =>
            {
                switch (ch)
                {
                    case '.': return State.Empty;
                    case '/': return State.MirrorBLTR;
                    case '\\': return State.MirrorTLBR;
                    case '|': return State.VerticalSplitter;
                    case '-': return State.HorizontalSplitter;
                    default:
                        Console.Error.WriteLine("Unknown object: " + ch);
                        throw new InvalidOperationException();
                }
            }).ToArray();
            if (rows.Count > 0 && rows[0].Length != row.Length)
            {
                Console.Error.WriteLine("Row of differing length: " + line);
                throw new InvalidOperationException();
            }
            rows.Add(row);
        }
        return Array2D<State>.From(rows);
    }

    public override void Part1(Array2D<State> data, RunSettings settings)
    {
        var energized = Array2D<bool>.FromSize(data, false);
        TraceRay(data, (0, 0), Direction.East, (position, _) => {
            energized[position] = true;
            return true;
        });
        if (settings.Verbose)
        {
            PrintMap(data, energized);
            Console.WriteLine();
        }
        var tilesEnergized = energized.Data.Count(pos => pos);
        Console.WriteLine("Tiles energized count is " + tilesEnergized);
    }

    public override void Part2(Array2D<State> data, RunSettings settings)
    {
        var best = data.EnumerateBorderPositionsWithDirections()
            .Select(tuple =>
            {
                var (position, direction) = tuple;
                var energized = Array2D<bool>.FromSize(data, false);
                TraceRay(data, position, direction, (position, _) =>
                {
                    energized[position] = true;
                    return true;
                });
                var tilesEnergized = energized.Data.Count(pos => pos);
                return (position, direction, energized, tilesEnergized);
            }).MaxBy(tuple => tuple.tilesEnergized);
        if (settings.Verbose)
        {
            PrintMap(data, best.energized);
            Console.WriteLine();
        }
        Console.WriteLine("Best energized count is " + best.tilesEnergized);
    }

    private static void PrintMap(Array2D<State> map, Array2D<bool> energized)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (energized[x, y])
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                    Console.ResetColor();
                switch (map[x, y])
                {
                    case State.Empty:
                        Console.Write('.');
                        break;
                    case State.MirrorBLTR:
                        Console.Write('/');
                        break;
                    case State.MirrorTLBR:
                        Console.Write('\\');
                        break;
                    case State.HorizontalSplitter:
                        Console.Write('-');
                        break;
                    case State.VerticalSplitter:
                        Console.Write('|');
                        break;
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static void TraceRay(Array2D<State> map, Position startPosition, Direction startDirection, Func<Position, Direction, bool> func)
    {
        var visited = Array2D<Direction>.FromSize(map, Direction.None);
        var queue = new Queue<(Position, Direction)>();
        queue.Enqueue((startPosition, startDirection));
        while (queue.Count > 0)
        {
            var (position, direction) = queue.Dequeue();
            if (!position.IsInside(map) || visited[position].HasFlag(direction))
                continue;
            visited[position] |= direction;
            if (!func(position, direction))
                return;
            switch (map[position])
            {
                case State.Empty:
                    queue.Enqueue((position + direction, direction));
                    break;
                case State.MirrorBLTR:
                    if (direction.IsHorizontal())
                        direction = direction.RotateLeft();
                    else
                        direction = direction.RotateRight();
                    queue.Enqueue((position + direction, direction));
                    break;
                case State.MirrorTLBR:
                    if (direction.IsHorizontal())
                        direction = direction.RotateRight();
                    else
                        direction = direction.RotateLeft();
                    queue.Enqueue((position + direction, direction));
                    break;
                case State.HorizontalSplitter:
                    if (direction.IsHorizontal())
                        queue.Enqueue((position + direction, direction));
                    else
                    {
                        queue.Enqueue((position, direction.RotateLeft()));
                        queue.Enqueue((position, direction.RotateRight()));
                    }
                    break;
                case State.VerticalSplitter:
                    if (direction.IsVertical())
                        queue.Enqueue((position + direction, direction));
                    else
                    {
                        queue.Enqueue((position, direction.RotateLeft()));
                        queue.Enqueue((position, direction.RotateRight()));
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
