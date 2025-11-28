using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day21 : DayRunner<Day21.Button[][]>
{
    public enum Button
    {
        Num0,
        Num1,
        Num2,
        Num3,
        Num4,
        Num5,
        Num6,
        Num7,
        Num8,
        Num9,
        Up,
        Right,
        Down,
        Left,
        A,
        Empty = -1
    }
    private static readonly Array2D<Button> Numpad = new([
        Button.Num7, Button.Num8, Button.Num9,
        Button.Num4, Button.Num5, Button.Num6,
        Button.Num1, Button.Num2, Button.Num3,
        Button.Empty, Button.Num0, Button.A
    ], 3, 4);
    private static readonly Array2D<Button> Dpad = new([
        Button.Empty, Button.Up, Button.A,
        Button.Left, Button.Down, Button.Right
    ], 3, 2);
    public override Button[][] Parse(FileReference file)
    {
        return file.GetLines().Select(line =>
        {
            return line.Select(ch =>
            {
                return ch switch
                {
                    '0' => Button.Num0,
                    '1' => Button.Num1,
                    '2' => Button.Num2,
                    '3' => Button.Num3,
                    '4' => Button.Num4,
                    '5' => Button.Num5,
                    '6' => Button.Num6,
                    '7' => Button.Num7,
                    '8' => Button.Num8,
                    '9' => Button.Num9,
                    'A' => Button.A,
                    _ => throw new InvalidOperationException()
                };
            }).ToArray();
        }).ToArray();
    }

    public override void Part1(Button[][] data, RunSettings settings)
    {
        //var dPadPaths = GetButtonPaths(Dpad);
        //var numpadPaths = GetButtonPaths(Numpad);
        var transforms = GetTransforms(2);
        var sum = 0;
        foreach (var code in data)
        {
            var buttons = Enumerable.Repeat(Button.A, 1).Concat(code).Zip(code).SelectMany(pair => transforms[pair]).ToArray();
            var complexity = buttons.Length * GetNumber(code);
            if (settings.Verbose)
            {
                Console.WriteLine(GetString(code));
                Console.WriteLine("\t=> " + GetString(buttons));
                Console.WriteLine("\tComplexity is " + buttons.Length + " * " + GetNumber(code) + " = " + complexity);
            }
            sum += complexity;
        }
        Console.WriteLine("Sum of complexities is " + sum);
    }

    public override void Part2(Button[][] data, RunSettings settings)
    {
        var transforms = GetTransforms(25);
        var sum = 0;
        foreach (var code in data)
        {
            var buttons = Enumerable.Repeat(Button.A, 1).Concat(code).Zip(code).SelectMany(pair => transforms[pair]).ToArray();
            var complexity = buttons.Length * GetNumber(code);
            if (settings.Verbose)
            {
                Console.WriteLine(GetString(code));
                Console.WriteLine("\t=> " + GetString(buttons));
                Console.WriteLine("\tComplexity is " + buttons.Length + " * " + GetNumber(code) + " = " + complexity);
            }
            sum += complexity;
        }
        Console.WriteLine("Sum of complexities is " + sum);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day21), settings.Example ? "day21-example.txt" : "day21-input.txt");
    }

    private static Dictionary<(Button, Button), Button[]> GetTransforms(int depth)
    {
        var dpadData = GetPaths(Dpad);
        var numpadData = GetPaths(Numpad);
        var buttons = Numpad.Data.Where(button => button != Button.Empty);
        var paths = new Dictionary<(Button, Button), Button[]>();
        foreach (var button1 in buttons)
        {
            foreach (var button2 in buttons)
            {
                Console.WriteLine("Best path for " + button1 + "=>" + button2 + " is ");
                var bestLength = int.MaxValue;
                Button[] bestPath = [];
                var transformed = TransformButtons(numpadData, [button2], button1);
                for (var i = 0; i < depth; ++i)
                    transformed = transformed.SelectMany(path => TransformButtons(dpadData, path.ToArray()));
                foreach (var path in transformed)
                {
                    if (path.Count() < bestLength)
                    {
                        bestPath = path.ToArray();
                        Console.WriteLine("\tFound better: " + GetString(bestPath));
                        bestLength = bestPath.Length;
                        
                    }
                }
                Console.WriteLine("\t=> " + GetString(bestPath));
                paths[(button1, button2)] = bestPath;
            }
        }
        return paths;
    }

    private static string GetString(IEnumerable<Button> code)
    {
        return String.Join("", code.Select(button => button switch
        {
            Button.Num0 => '0',
            Button.Num1 => '1',
            Button.Num2 => '2',
            Button.Num3 => '3',
            Button.Num4 => '4',
            Button.Num5 => '5',
            Button.Num6 => '6',
            Button.Num7 => '7',
            Button.Num8 => '8',
            Button.Num9 => '9',
            Button.Up => '^',
            Button.Right => '>',
            Button.Down => 'v',
            Button.Left => '<',
            Button.A => 'A',
            _ => throw new InvalidOperationException()
        }));
    }

    private static int GetNumber(Button[] code) => GetString(code.Where(button => button != Button.A)).AsSpan().ToInt();

    //private static Button[] GetBestPath(Array2D<Button> map, IEnumerable<>)



    private static IEnumerable<IEnumerable<Button>> TransformButtons(PathData pathData, IEnumerable<Button> source, Button prevButton = Button.A)
    {
        if (!source.Any())
        {
            yield return [];
            yield break;
        }
        /*
        var curButton = source.First();
        foreach (var path in GetPathsBetween(map, prevButton, curButton))
        {
            var prefix = path.Concat([Button.A]).ToArray();
            foreach (var subPath in TransformButtons(map, source.Skip(1).ToArray(), curButton))
                yield return prefix.Concat(subPath).ToArray();
        }
        */
        var paths = Enumerable.Repeat(prevButton, 1).Concat(source).Zip(source).Select(pair => pathData.Paths[pair]).ToArray();
        var queue = new Queue<(IEnumerator<Button[]>[], int)>();
        var enumerators = new List<IEnumerator<Button[]>>
        {
            paths[0].AsEnumerable().GetEnumerator()
        };
        while (enumerators.Count > 0)
        {
            if (enumerators.Last().MoveNext())
            {
                if (enumerators.Count == paths.Length)
                {
                    yield return enumerators
                        .Select(enumerator => enumerator.Current)
                        .Aggregate(Enumerable.Empty<Button>(), (a, b) => a.Concat(b));
                }
                else
                    enumerators.Add(paths[enumerators.Count].AsEnumerable().GetEnumerator());
            }
            else
                enumerators.RemoveAt(enumerators.Count - 1);
        }
        
    }

    private class PathData(Array2D<Button> map, Dictionary<(Button, Button), Button[][]> paths)
    {
        public Array2D<Button> Map = map;
        public Dictionary<(Button, Button), Button[][]> Paths = paths;
    }

    private static PathData GetPaths(Array2D<Button> map)
    {
        var paths = new Dictionary<(Button, Button), Button[][]>();
        var buttons = map.Data.Where(button => button != Button.Empty);
        foreach (var button1 in buttons)
        {
            foreach (var button2 in buttons)
            {
                paths[(button1, button2)] = GetPathsBetween(map, button1, button2).ToArray();
            }
        }
        return new(map, paths);
    }

    private static IEnumerable<Button[]> GetPathsBetween(Array2D<Button> map, Button from, Button to)
    {
        if (from == to)
        {
            yield return [Button.A];
            yield break;
        }
        var startPos = map.PositionAt(Array.IndexOf(map.Data, from));
        var endPos = map.PositionAt(Array.IndexOf(map.Data, to));
        var emptyPos = map.PositionAt(Array.IndexOf(map.Data, Button.Empty));
        var path = new List<Button[]>();
        var delta = endPos - startPos;
        var xButtons = delta.X > 0
            ? Enumerable.Repeat(Button.Right, delta.X)
            : Enumerable.Repeat(Button.Left, -delta.X);
        var yButtons = delta.Y > 0
            ? Enumerable.Repeat(Button.Down, delta.Y)
            : Enumerable.Repeat(Button.Up, -delta.Y);
        if ((startPos + (delta.X, 0)) != emptyPos)
            yield return xButtons.Concat(yButtons).Concat([Button.A]).ToArray();
        if ((startPos + (0, delta.Y)) != emptyPos)
            yield return yButtons.Concat(xButtons).Concat([Button.A]).ToArray();
    }
    /*
    private static Dictionary<(Button, Button), Button[]> GetButtonPaths(Array2D<Button> map)
    {
        var buttons = map.Data.Where(button => button != Button.Empty);
        var paths = new Dictionary<(Button, Button), Button[]>();
        foreach (var button1 in buttons)
        {
            foreach (var button2 in buttons)
                paths[(button1, button2)] = GetPathBetween(map, button1, button2);
        }
        return paths;
    }*/
}
