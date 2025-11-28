namespace AdventOfCode.Utils;

public class Array2D<T> (T[] data, int width, int height)
{
    private readonly T[] InternalData = CheckLength(data, width * height);
    public T[] Data { get => InternalData; }
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;

    public Array2D(int width, int height) : this(new T[width * height], width, height) {}
    public Array2D(IEnumerable<T> data, int width, int height) : this([.. data.Take(width * height)], width, height) { }

    public T this[int x, int y]
    {
        get => InternalData[x + y * Width];
        set => InternalData[x + y * Width] = value;
    }
    public T this[Position position]
    {
        get => InternalData[position.X + position.Y * Width];
        set => InternalData[position.X + position.Y * Width] = value;
    }
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var item in InternalData)
            hashCode.Add(item);
        return hashCode.ToHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (obj is Array2D<T> other)
            return Enumerable.SequenceEqual(InternalData, other.InternalData);
        return false;
    }

    /*
    public static Array2D<T> operator*(Array2D<T> left, Array2D<T> right)
        where T : INumber<T>
    {
        var result = new Array2D<T>(left.Width, right.Height);
        for (int y = 0; y < result.Height; ++y)
        {
            for (int x = 0; x < result.Width; ++x)
            {
                result[x, y] = left[x, y] * right[x, y];
            }
        }
        return result;
    }
    */
    private static T[] CheckLength(T[] data, int length)
    {
        if (data.Length < length)
            throw new ArgumentException("data");
        return data;
    }

    public void ForEach(Action<int, int, T> func)
    {
        int i = 0;
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                func(x, y, InternalData[i]);
                ++i;
            }
        }
    }
    public void ForEach(Action<Position, T> func)
        => ForEach((x, y, value) => func(new Position(x, y), value));
    public int IndexAt(Position position)
        => position.X + position.Y * Width;
    public Position PositionAt(int index)
        => new(index % Width, index / Width);
    public Position? IndexOf(T value)
    {
        var index = Array.IndexOf(InternalData, value);
        if (index < 0)
            return null;
        return PositionAt(index);
    }
    public Position? IndexOf(T value, Position start)
    {
        var index = Array.IndexOf(InternalData, value, IndexAt(start));
        if (index < 0)
            return null;
        return PositionAt(index);
    }
    public Position? FindIndex(Predicate<T> match)
    {
        
        var index = Array.FindIndex(InternalData, match);
        if (index < 0)
            return null;
        return PositionAt(index);
    }
    public IEnumerable<Position> EnumeratePositions()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
                yield return new(x, y);
        }
    }
    public IEnumerable<Position> EnumerateBorderPositions()
    {
        if (Width <= 0 || Height <= 0)
            yield break;
        for (int x = 0; x < Width; ++x)
            yield return new(x, 0);
        
        if (Height > 1)
        {
            var lastX = Width - 1;
            var lastY = Height - 1;
            for (int y = 1; y < lastY; ++y)
            {
                yield return new(0, y);
                if (lastX > 0)
                    yield return new(lastX, y);
            }
            for (int x = 0; x < Width; ++x)
                yield return new(x, lastY);
        }
    }
    public IEnumerable<(Position, Direction)> EnumerateBorderPositionsWithDirections()
    {
        if (Width <= 0 || Height <= 0)
            yield break;

        for (int x = 0; x < Width; ++x)
            yield return ((x, 0), Direction.South);
        var lastX = Width - 1;
        var lastY = Height - 1;
        for (int y = 0; y < Height; ++y)
        {
            yield return ((0, y), Direction.East);
            if (lastX > 0)
                yield return ((lastX, y), Direction.West);
        }
        for (int x = 0; x < Width; ++x)
            yield return ((x, lastY), Direction.North);
    }
    public IEnumerable<Position> EnumerateRowPositions(int y)
    {
        for (int x = 0; x < Width; ++x)
            yield return new(x, y);
    }
    public IEnumerable<Position> EnumerateColumnPositions(int x)
    {
        for (int y = 0; y < Height; ++y)
            yield return new(x, y);
    }
    public IEnumerable<Position> FindPositions(T value)
    {
        var index = Array.IndexOf(InternalData, value);
        while (index >= 0)
        {
            yield return PositionAt(index);
            index = Array.IndexOf(InternalData, value, index + 1);
        }
    }
    public Array2D<T> AsResized(int width, int height, Position shift = default)
    {
        var newArray = new Array2D<T>(width, height);
        var copyWidth = Math.Min(width, Width);
        var copyHeight = Math.Min(height, Height);
        if (copyWidth > 0 && copyHeight > 0)
        {
            for (int y = 0; y < copyHeight; ++y)
                Array.Copy(InternalData, y * Width, newArray.InternalData, shift.X + (y + shift.Y) * width, copyWidth);
        }
        return newArray;
    }
    public Array2D<T> AsResized(Position positionToInclude, Position shift = default)
    {
        var newWidth = Math.Max(Width, positionToInclude.X + 1) + shift.X;
        var newHeight = Math.Max(Height, positionToInclude.Y + 1) + shift.Y;
        if (shift.X == 0 && shift.Y == 0 && newWidth <= Width && newHeight <= Height)
            return new(InternalData, Width, Height);
        return AsResized(newWidth, newHeight, shift);

    }

    public Span<T> AsSpan(int x, int y, int length)
        => InternalData.AsSpan(x + y * Width, length);
    public Span<T> AsSpan(int x, int y)
        => InternalData.AsSpan(x + y * Width, Width - x);

    public static Array2D<T> FromSize<U>(Array2D<U> source, T value)
        => new(Enumerable.Repeat(value, source.Width * source.Height), source.Width, source.Height);
    public static Array2D<T> From<U>(Array2D<U> source, Func<U, T> transform)
        => new(source.InternalData.Select(transform), source.Width, source.Height);
    public static Array2D<T> From(Array2D<T> source)
        => new([.. source.InternalData], source.Width, source.Height);
    public static Array2D<T> From<U>(IList<IList<U>> source, Func<U, T> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Count, source.Count);
    public static Array2D<T> From(IList<IList<T>> source)
        => new(source.Aggregate(Enumerable.Empty<T>(), (a, b) => a.Concat(b)), source[0].Count, source.Count);
    public static Array2D<T> From<U>(IList<U[]> source, Func<U, T> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Length, source.Count);
    public static Array2D<T> From(IList<T[]> source)
        => new(source.Aggregate(Enumerable.Empty<T>(), (a, b) => a.Concat(b)), source[0].Length, source.Count);
    public static Array2D<T> From<U>(IList<U>[] source, Func<U, T> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Count, source.Length);
    public static Array2D<T> From(IList<T>[] source)
        => new(source.Aggregate(Enumerable.Empty<T>(), (a, b) => a.Concat(b)), source[0].Count, source.Length);
    public static Array2D<U> From<U>(IList<string> source, Func<char, U> transform)
        => new(source.Aggregate(Enumerable.Empty<char>(), (a, b) => a.Concat(b)).Select(transform), source[0].Length, source.Count);
    public static Array2D<char> From(IList<string> source)
        => new(source.Aggregate(Enumerable.Empty<char>(), (a, b) => a.Concat(b)), source[0].Length, source.Count);
}
