namespace AdventOfCode.Utils;

internal class DirectionalArray2D<T>(int width, int height)
    where T : new()
{
    private readonly Array2D<T[]> Data = new(width, height);

    public IEnumerable<(Direction direction, T data)> GetDatas(Position position)
    {
        var datas = Data[position];
        if (datas == null)
            yield break;
        foreach (var direction in Directions.All)
        {
            var data = datas[direction.Index()];
            if (data != null)
                yield return (direction, data);
        }
    }
    public T Get(Position position, Direction direction)
    {
        var datas = Data[position];
        if (datas == null)
            Data[position] = datas = new T[Directions.All.Length];
        var dirIndex = direction.Index();
        var data = datas[dirIndex];
        if (data == null)
            datas[dirIndex] = data = new();
        return data;
    }
    public T this[Position position, Direction direction] => Get(position, direction);
}
