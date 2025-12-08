using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AdventOfCode.Utils;
[DebuggerDisplay("X = {X} Y = {Y} Z = {Z}")]
public struct Position3D(int x, int y, int z)
{
    public static readonly Position3D Zero = default;

    public int X = x;
    public int Y = y;
    public int Z = z;
    public Position3D(Position pos, int z) : this(pos.X, pos.Y, z) { }
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Position3D p)
            return X == p.X && Y == p.Y && Z == p.Z;
        return false;
    }
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
    public readonly Position3D Abs()
        => new(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    public readonly Position3D Min(Position3D right) => new(Math.Min(X, right.X), Math.Min(Y, right.Y), Math.Min(Z, right.Z));
    public readonly Position3D Max(Position3D right) => new(Math.Max(X, right.X), Math.Max(Y, right.Y), Math.Max(Z, right.Z));
    public readonly double DistanceSquared(Position3D right)
    {
        var vec = this - right;
        return (double)vec.X * vec.X + (double)vec.Y * vec.Y + (double)vec.Z * vec.Z;
    }
    public readonly double Distance(Position3D right) => Math.Sqrt(DistanceSquared(right));
    public readonly void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }
    public override readonly string ToString() => $"[X={X}, Y={Y}, Z={Z}]";
    public static bool operator ==(Position3D left, Position3D right) => left.Equals(right);
    public static bool operator !=(Position3D left, Position3D right) => !(left == right);
    public static Position3D operator +(Position3D left, Position3D right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Position3D operator -(Position3D left, Position3D right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static Position3D operator *(Position3D left, int right) => new(left.X * right, left.Y * right, left.Z * right);
    public static Position3D operator -(Position3D pos) => new(-pos.X, -pos.Y, -pos.Z);
    public static implicit operator Position3D((int x, int y, int z) pos) => new(pos.x, pos.y, pos.z);
    public static implicit operator Position3D((Position pos, int z) tuple) => new(tuple.pos.X, tuple.pos.Y, tuple.z);
    public static implicit operator Position3D(Position pos) => new(pos.X, pos.Y, 0);
}
