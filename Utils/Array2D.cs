using System.Collections;

namespace AdventOfCode.Utils;

public abstract class BaseArray2D<T>(int width, int height)
{
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;

    public abstract IEnumerable<T> Data { get; }
    protected abstract int IndexOfData(T value, int startAt);
    protected abstract int FindDataIndex(Predicate<T> pred, int startAt);
    public abstract T this[int x, int y] { get; set; }
    public T this[Position position]
    {
        get => this[position.X, position.Y];
        set => this[position.X, position.Y] = value;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var item in Data)
            hashCode.Add(item);
        return hashCode.ToHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (obj is not BaseArray2D<T> other)
            return false;
        return Width == other.Width && Height == other.Height && Enumerable.SequenceEqual(Data, other.Data);
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

    public void ForEach(Action<int, int, T> func)
    {
        var enumerator = Data.GetEnumerator();
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Data ended early");
                func(x, y, enumerator.Current);
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
        var index = IndexOfData(value, 0);
        if (index < 0)
            return null;
        return PositionAt(index);
    }
    public Position? IndexOf(T value, Position start)
    {
        var index = IndexOfData(value, IndexAt(start));
        if (index < 0)
            return null;
        return PositionAt(index);
    }
    public Position? FindIndex(Predicate<T> match)
    {
        
        var index = FindDataIndex(match, 0);
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
        var index = IndexOfData(value, 0);
        while (index >= 0)
        {
            yield return PositionAt(index);
            index = IndexOfData(value, index + 1);
        }
    }

    public IEnumerable<T> GetRow(int y) => Data.Skip(y * Width).Take(Width);
    public IEnumerable<T> GetColumn(int x)
    {
        for (var y = 0; y < Height; ++y)
            yield return this[x, y];
    }

    public void SetRow(int y, IEnumerable<T> data)
    {
        foreach (var (x, e) in data.Index().Take(Width))
            this[x, y] = e;
    }
    public void SetColumn(int x, IEnumerable<T> data)
    {
        foreach (var (y, e) in data.Index().Take(Height))
            this[x, y] = e;
    }
}

public abstract class BaseArray2D<T, U>(int width, int height) : BaseArray2D<T>(width, height)
    where U : BaseArray2D<T>
{
    protected abstract U Create(int width, int height);

    public U AsTransposed()
    {
        var array = Create(Height, Width);
        foreach (var pos in EnumeratePositions())
            array[pos.Y, pos.X] = this[pos];
        return array;
    }
    public U AsClockwiseRotated()
    {
        var array = Create(Height, Width);
        foreach (var pos in EnumeratePositions())
            array[Height - 1 - pos.Y, pos.X] = this[pos];
        return array;
    }
    public U AsCounterClockwiseRotated()
    {
        var array = Create(Height, Width);
        foreach (var pos in EnumeratePositions())
            array[pos.Y, Width - 1 - pos.X] = this[pos];
        return array;
    }
    public U AsFlippedX()
    {
        var array = Create(Height, Width);
        foreach (var pos in EnumeratePositions())
            array[Width - 1 - pos.X, pos.Y] = this[pos];
        return array;
    }
    public U AsFlippedY()
    {
        var array = Create(Height, Width);
        foreach (var pos in EnumeratePositions())
            array[pos.X, Height - 1 - pos.Y] = this[pos];
        return array;
    }
}

public sealed class Array2D<T>(T[] data, int width, int height) : BaseArray2D<T, Array2D<T>>(width, height)
{
    private readonly T[] InternalData = CheckLength(data, (long)width * height);
    public override T[] Data => InternalData;


    public Array2D(int width, int height) : this(new T[width * height], width, height) { }
    public Array2D(IEnumerable<T> data, int width, int height) : this([.. data.Take(width * height)], width, height) { }

    protected override Array2D<T> Create(int width, int height) => new(width, height);
    protected override int IndexOfData(T value, int startIndex) => Array.IndexOf(InternalData, value, startIndex);
    protected override int FindDataIndex(Predicate<T> pred, int startIndex) => Array.FindIndex(InternalData, startIndex, pred);

    public override T this[int x, int y]
    {
        get => InternalData[x + y * Width];
        set => InternalData[x + y * Width] = value;
    }

    private static T[] CheckLength(T[] data, long length)
    {
        if (data.LongLength < length)
            throw new ArgumentException("Array backing data doesn't match dimensions", nameof(data));
        return data;
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
        return AsResized(newWidth, newHeight, shift);
    }

    public T[][] To2DArray()
        => [.. Enumerable.Range(0, Height).Select(y => AsSpan(0, y).ToArray())];
    public Span<T> AsSpan(int x, int y, int length)
        => InternalData.AsSpan(x + y * Width, length);
    public Span<T> AsSpan(int x, int y)
        => InternalData.AsSpan(x + y * Width, Width - x);
    public IEnumerable<T> AsEnumerable(int x, int y, int length)
        => new SubArray<T>(InternalData, x + y * Width, length);
    public IEnumerable<T> AsEnumerable(int x, int y)
        => new SubArray<T>(InternalData, x + y * Width, Width - x);

    public static Array2D<T> FromSize<U>(BaseArray2D<U> source, T value)
        => new(Enumerable.Repeat(value, source.Width * source.Height), source.Width, source.Height);
    public static Array2D<T> From<U>(BaseArray2D<U> source, Func<U, T> transform)
        => new(source.Data.Select(transform), source.Width, source.Height);
    public static Array2D<T> From(BaseArray2D<T> source)
        => new([.. source.Data], source.Width, source.Height);
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

public class BitArray2D(BitArray data, int width, int height) : BaseArray2D<bool, BitArray2D>(width, height)
{
    private readonly BitArray InternalData = CheckLength(data, (long)width * height);

    public BitArray2D(int width, int height) : this(new BitArray(width * height), width, height) { }
    public BitArray2D(IEnumerable<bool> data, int width, int height) : this(width, height)
    {
        foreach (var (i, item) in data.Take(Width * Height).Index())
            InternalData[i] = item;
    }

    protected override BitArray2D Create(int width, int height) => new(width, height);
    public override bool this[int x, int y]
    {
        get => InternalData[x + y * Width];
        set => InternalData[x + y * Width] = value;
    }

    private static BitArray CheckLength(BitArray data, long length)
    {
        if (data.Length < length)
            throw new ArgumentException("Array backing data doesn't match dimensions", nameof(data));
        return data;
    }

    protected override int FindDataIndex(Predicate<bool> pred, int startAt)
    {
        foreach (var (i, item) in Data.Index().Skip(startAt))
        {
            if (pred(item))
                return i;
        }
        return -1;
    }

    public override IEnumerable<bool> Data => InternalData.OfType<bool>();

    protected override int IndexOfData(bool value, int startAt)
    {
        foreach (var (i, item) in Data.Index().Skip(startAt))
        {
            if (item == value)
                return i;
        }
        return -1;
    }

    public bool[][] To2DArray()
        => [.. Enumerable.Range(0, Height).Select(y => Data.Skip(y * Width).Take(Width).ToArray())];
    public IEnumerable<bool> AsEnumerable(int x, int y, int length)
        => Data.Skip(x + y * Width).Take(length);
    public IEnumerable<bool> AsEnumerable(int x, int y)
        => Data.Skip(x + y * Width).Take(Width - x);

    public static BitArray2D FromSize<U>(BaseArray2D<U> source, bool value)
        => new(Enumerable.Repeat(value, source.Width * source.Height), source.Width, source.Height);
    public static BitArray2D From(BitArray2D source, Func<bool, bool> transform)
        => new(new BitArray(source.InternalData), source.Width, source.Height);
    public static BitArray2D From<U>(BaseArray2D<U> source, Func<U, bool> transform)
        => new(source.Data.Select(transform), source.Width, source.Height);
    public static BitArray2D From(BaseArray2D<bool> source)
        => new(source.Data, source.Width, source.Height);
    public static BitArray2D From<U>(IList<IList<U>> source, Func<U, bool> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Count, source.Count);
    public static BitArray2D From(IList<IList<bool>> source)
        => new(source.Aggregate(Enumerable.Empty<bool>(), (a, b) => a.Concat(b)), source[0].Count, source.Count);
    public static BitArray2D From<U>(IList<U[]> source, Func<U, bool> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Length, source.Count);
    public static BitArray2D From(IList<bool[]> source)
        => new(source.Aggregate(Enumerable.Empty<bool>(), (a, b) => a.Concat(b)), source[0].Length, source.Count);
    public static BitArray2D From<U>(IList<U>[] source, Func<U, bool> transform)
        => new(source.Aggregate(Enumerable.Empty<U>(), (a, b) => a.Concat(b)).Select(transform), source[0].Count, source.Length);
    public static BitArray2D From(IList<bool>[] source)
        => new(source.Aggregate(Enumerable.Empty<bool>(), (a, b) => a.Concat(b)), source[0].Count, source.Length);
}

internal class SubArray<T>(T[] baseArray, int offset, int length) : IList<T>, IEnumerable<T>, IList, IEnumerable
{
    public T[] BaseArray { get; } = baseArray;
    public int Offset { get; } = offset;
    public int Length { get; } = length;

    public T this[int index] { get => BaseArray[Offset + CheckBounds(index)]; set => BaseArray[Offset + CheckBounds(index)] = value; }
    object? IList.this[int index] { get => BaseArray.GetValue(Offset + CheckBounds(index)); set => BaseArray.SetValue(value, Offset + CheckBounds(index)); }
    public int Count => Length;
    public bool IsReadOnly => false;
    public bool IsFixedSize => true;
    public bool IsSynchronized => false;
    public object SyncRoot => BaseArray;

    private int CheckBounds(int bounds)
    {
        if (bounds < 0 || bounds > Length)
            throw new IndexOutOfRangeException();
        return bounds;
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(T item) => Array.IndexOf(BaseArray, item, Offset, Length) >= BaseArray.GetLowerBound(0);
    public bool Contains(object? value) => Array.IndexOf(BaseArray, value, Offset, Length) >= BaseArray.GetLowerBound(0);
    public int IndexOf(T item) => Array.IndexOf(BaseArray, item, Offset, Length);
    public int IndexOf(object? value) => Array.IndexOf(BaseArray, value, Offset, Length);
    public void CopyTo(T[] array, int arrayIndex) => Array.Copy(BaseArray, Offset, array, arrayIndex, Length);
    public void CopyTo(Array array, int index) => Array.Copy(BaseArray, Offset, array, index, Length);
    public void Insert(int index, T item) => throw new NotSupportedException();
    public void Insert(int index, object? value) => throw new NotSupportedException();
    public void Add(T item) => throw new NotSupportedException();
    public int Add(object? value) => throw new NotSupportedException();
    public bool Remove(T item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
    public void Remove(object? value) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();

    private class Enumerator(SubArray<T> parent) : IEnumerator<T>, IEnumerator
    {
        private SubArray<T> Parent = parent;
        private int Index = -1;
        public T Current => Parent[Index];
        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (Index < Parent.Length)
            {
                ++Index;
                return Index < Parent.Length;
            }
            else
                return false;
        }
        public void Reset() => Index = -1;
    }
}
