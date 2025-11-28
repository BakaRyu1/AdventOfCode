using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace AdventOfCode.Utils;

[Flags]
public enum Direction : byte
{
    None = 0,
    North = 1,
    West = 2,
    South = 4,
    East = 8
}
public static class Directions
{
    internal struct DirectionData(Direction opposite, Direction leftRotation, Direction rightRotation, int dx, int dy, int index)
    {
        public Direction Opposite = opposite;
        public Direction LeftRotation = leftRotation;
        public Direction RightRotation = rightRotation;
        public Position Delta = new(dx, dy);
        public int Index = index;
    }
    private static readonly DirectionData?[] DirectionDatas = [
        null,
        new(Direction.South, Direction.West, Direction.East, 0, -1, 0),
        new(Direction.East, Direction.South, Direction.North, -1, 0, 1),
        null,
        new(Direction.North, Direction.East, Direction.West, 0, 1, 2),
        null,
        null,
        null,
        new(Direction.West, Direction.North, Direction.South, 1, 0, 3)
    ];
    private static readonly Dictionary<Direction, char> TableCharacters = new()
    {
        { Direction.West | Direction.East | Direction.North | Direction.South, '┼' },
        { Direction.West | Direction.North | Direction.South, '┤' },
        { Direction.East | Direction.North | Direction.South, '├' },
        { Direction.West | Direction.East| Direction.North, '┴' },
        { Direction.West | Direction.East| Direction.South, '┬' },
        { Direction.West | Direction.East, '─' },
        { Direction.North | Direction.South, '│' },
        { Direction.West | Direction.North, '┘' },
        { Direction.East | Direction.North, '└' },
        { Direction.West | Direction.South, '┐' },
        { Direction.East | Direction.South, '┌' },
        { Direction.North, '^' },
        { Direction.South, 'v' },
        { Direction.West, '<' },
        { Direction.East, '>' },
        { Direction.None, ' ' }
    };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DirectionData GetData(Direction direction)
        => DirectionDatas[(int)direction] ?? throw new InvalidOperationException();
    public static readonly ImmutableArray<Direction> All = [Direction.North, Direction.West, Direction.South, Direction.East];
    public static Direction Opposite(this Direction direction) => GetData(direction).Opposite;
    public static Direction RotateRight(this Direction direction) => GetData(direction).RightRotation;
    public static Direction RotateLeft(this Direction direction) => GetData(direction).LeftRotation;
    public static Position Delta(this Direction direction) => GetData(direction).Delta;
    public static int Index(this Direction direction) => GetData(direction).Index;
    public static bool IsHorizontal(this Direction direction) => direction == Direction.West || direction == Direction.East;
    public static bool IsVertical(this Direction direction) => direction == Direction.North || direction == Direction.South;
    private static IEnumerable<int> EnumerateRange(int direction, int rangeStart, int rangeCount, int enumStart)
    {
        if (direction == 0)
            yield break;
        var rangeEnd = rangeStart + rangeCount;
        if (direction > 0)
        {
            if (enumStart < rangeStart)
                enumStart = rangeStart;
            for (var i = enumStart; i < rangeEnd; ++i)
                yield return i;
        }
        else
        {
            if (enumStart < 0 || enumStart >= rangeEnd)
                enumStart = rangeEnd - 1;
            for (var i = enumStart; i >= rangeStart; --i)
                yield return i;
        }
    }
    private static int GetRangeBound(int direction, int rangeStart, int rangeCount)
    {
        if (direction == 0)
            throw new InvalidOperationException();
        if (direction > 0)
            return rangeStart + rangeCount - 1;
        else
            return rangeStart;
    }
    public static IEnumerable<int> EnumerateRangeX(this Direction direction, int rangeStart, int rangeCount, int enumStart = -1)
        => EnumerateRange(direction.Delta().X, rangeStart, rangeCount, enumStart);
    public static IEnumerable<int> EnumerateRangeY(this Direction direction, int rangeStart, int rabgeCount, int enumStart = -1)
        => EnumerateRange(direction.Delta().Y, rangeStart, rabgeCount, enumStart);
    public static int GetRangeBoundX(this Direction direction, int rangeStart, int rangeCount)
        => GetRangeBound(direction.Delta().X, rangeStart, rangeCount);
    public static int GetRangeBoundY(this Direction direction, int rangeStart, int rangeCount)
        => GetRangeBound(direction.Delta().Y, rangeStart, rangeCount);
    public static char AsPrintableChar(this Direction direction) => TableCharacters[direction];
    public static IEnumerable<Direction> Separate(this Direction directions)
        => All.Where(d => directions.HasFlag(d));
}
