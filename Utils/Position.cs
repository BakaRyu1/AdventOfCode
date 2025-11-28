using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace AdventOfCode.Utils;

[DebuggerDisplay("X = {X} Y = {Y}")]
public struct LongPosition(long x, long y)
{
    public long X = x;
    public long Y = y;
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Position p)
            return X == p.X && Y == p.Y;
        return false;
    }
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);
    public readonly bool IsInside<T>(Array2D<T> map)
        => (X >= 0 && Y >= 0 && X < map.Width && Y < map.Height);
    public readonly LongPosition Abs()
        => new(Math.Abs(X), Math.Abs(Y));
    public readonly void Deconstruct(out long x, out long y)
    {
        x = X;
        y = Y;
    }
    public override readonly string ToString() => $"[X={X}, Y={Y}]";
    public static bool operator ==(LongPosition left, LongPosition right) => left.Equals(right);
    public static bool operator !=(LongPosition left, LongPosition right) => !(left == right);
    public static LongPosition operator +(LongPosition left, LongPosition right) => new(left.X + right.X, left.Y + right.Y);
    public static LongPosition operator -(LongPosition left, LongPosition right) => new(left.X - right.X, left.Y - right.Y);
    public static LongPosition operator +(LongPosition left, long right) => new(left.X * right, left.Y * right);
    public static LongPosition operator -(LongPosition pos) => new(-pos.X, -pos.Y);
    public static LongPosition operator +(LongPosition left, Direction right) => left + right.Delta();
    public static implicit operator LongPosition((long, long) pos) => new(pos.Item1, pos.Item2);
    public static implicit operator LongPosition(Position pos) => new(pos.X, pos.Y);
}
[DebuggerDisplay("X = {X} Y = {Y}")]
public struct Position(int x, int y)
{
    public static readonly Position Zero = default;

    public int X = x;
    public int Y = y;
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Position p)
            return X == p.X && Y == p.Y;
        return false;
    }
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);
    public readonly bool IsInside<T>(Array2D<T> map)
        => (X >= 0 && Y >= 0 && X < map.Width && Y < map.Height);
    public readonly Position Abs()
        => new(Math.Abs(X), Math.Abs(Y));
    public readonly Position Min(Position right) => new(Math.Min(X, right.X), Math.Min(Y, right.Y));
    public readonly Position Max(Position right) => new(Math.Max(X, right.X), Math.Max(Y, right.Y));
    public readonly void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
    public override readonly string ToString() => $"[X={X}, Y={Y}]";
    public static bool operator ==(Position left, Position right) => left.Equals(right);
    public static bool operator !=(Position left, Position right) => !(left == right);
    public static Position operator +(Position left, Position right) => new(left.X + right.X, left.Y + right.Y);
    public static Position operator -(Position left, Position right) => new(left.X - right.X, left.Y - right.Y);
    public static LongPosition operator +(Position left, LongPosition right) => (LongPosition)left + right;
    public static LongPosition operator -(LongPosition left, Position right) => left + (LongPosition)right;
    public static Position operator *(Position left, int right) => new(left.X * right, left.Y * right);
    public static Position operator -(Position pos) => new(-pos.X, -pos.Y);
    public static Position operator +(Position left, Direction right) => left + right.Delta();
    public static implicit operator Position((int, int) pos) => new(pos.Item1, pos.Item2);
    public static explicit operator Position(Direction direction) => direction.Delta();
    public static explicit operator Position(LongPosition pos) => new((int)pos.X, (int)pos.Y);
}
