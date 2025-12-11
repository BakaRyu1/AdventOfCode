using System.Diagnostics.CodeAnalysis;

namespace AdventOfCode.Utils;
public class ArrayComparer<T> : IEqualityComparer<T[]>, IAlternateEqualityComparer<Span<T>, T[]>
{
    public readonly static ArrayComparer<T> Instance = new();
    protected ArrayComparer() { }

    public T[] Create(Span<T> alternate) => alternate.ToArray();

    public bool Equals(T[]? x, T[]? y)
    {
        if (x == null || y == null)
            return x != y;
        return x.Length == y.Length && x.SequenceEqual(y);
    }

    public bool Equals(Span<T> alternate, T[] other)
    {
        return alternate.Length == other.Length && alternate.SequenceEqual(other);
    }

    public int GetHashCode([DisallowNull] T[] obj)
    {
        var hash = new HashCode();
        foreach (var i in obj)
            hash.Add(i);
        return hash.ToHashCode();
    }

    public int GetHashCode(Span<T> alternate)
    {
        var hash = new HashCode();
        foreach (var i in alternate)
            hash.Add(i);
        return hash.ToHashCode();
    }

    public static ChainedComparer With(IEqualityComparer<T> comparer) => new ChainedComparerImpl(comparer);

    public abstract class ChainedComparer(IEqualityComparer<T> comparer) : IEqualityComparer<T[]>, IAlternateEqualityComparer<Span<T>, T[]>
    {
        public IEqualityComparer<T> ElementComparer { get; } = comparer;
        public abstract T[] Create(Span<T> alternate);
        public abstract bool Equals(T[]? x, T[]? y);
        public abstract bool Equals(Span<T> alternate, T[] other);
        public abstract int GetHashCode([DisallowNull] T[] obj);
        public abstract int GetHashCode(Span<T> alternate);
    }

    private class ChainedComparerImpl(IEqualityComparer<T> comparer) : ChainedComparer(comparer)
    {
        public override T[] Create(Span<T> alternate) => alternate.ToArray();

        public override bool Equals(T[]? x, T[]? y)
        {
            if (x == null || y == null)
                return x != y;
            return x.Length == y.Length && x.SequenceEqual(y, ElementComparer);
        }

        public override bool Equals(Span<T> alternate, T[] other)
        {
            return alternate.Length == other.Length && alternate.SequenceEqual(other, ElementComparer);
        }

        public override int GetHashCode([DisallowNull] T[] obj)
        {
            var hash = new HashCode();
            foreach (var i in obj)
                hash.Add(i != null ? ElementComparer.GetHashCode(i) : 0);
            return hash.ToHashCode();
        }

        public override int GetHashCode(Span<T> alternate)
        {
            var hash = new HashCode();
            foreach (var i in alternate)
                hash.Add(i != null ? ElementComparer.GetHashCode(i) : 0);
            return hash.ToHashCode();
        }
    }
}