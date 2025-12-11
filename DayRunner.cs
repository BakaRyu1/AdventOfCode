using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode;

internal interface IDayRunner
{
    public void Run(RunSettings settings);
}

internal abstract class DayRunner<T> : DayRunner<T, T>
{
    public override T Parse2(FileReference file) => Parse(file);
    protected override T InternalParse2(FileReference file1, FileReference file2, T? data1)
    {
        if (data1 == null|| !file1.Equals(file2))
        {
            return base.InternalParse2(file1, file2, data1);
        }
        return data1;
    }
}

internal abstract partial class DayRunner<T, U> : IDayRunner
{
    public abstract T Parse(FileReference file);
    public abstract U Parse2(FileReference file);
    public abstract void Part1(T data, RunSettings settings);
    public abstract void Part2(U data, RunSettings settings);
    public virtual void InitSettings(ref RunSettings settings)
    {
        if (settings.File1 == null)
        {
            var type = GetType();
            var typeName = type.Name.ToLowerInvariant();
            settings.File1 = FileReference.Resource(type, settings.Example ? $"{typeName}-example.txt" : $"{typeName}-input.txt");
        }
    }
    protected virtual U InternalParse2(FileReference file1, FileReference file2, T? data1)
    {
        Console.WriteLine("Parsing...");
        using (TimeUtils.MeasureTime())
            return Parse2(file2);
    }

    public void Run(RunSettings settings)
    {
        Console.WriteLine($"--- {GetDayName(GetType())} ---");
        InitSettings(ref settings);
        T? data1 = default;
        var file1 = settings.File1 ?? throw new InvalidOperationException();
        var file2 = settings.File2 ?? file1;
        if (settings.Part1)
        {
            Console.WriteLine("Parsing...");
            using (TimeUtils.MeasureTime())
                data1 = Parse(file1);
            using (TimeUtils.MeasureTime())
                Part1(data1, settings);
        }
        if (settings.Part2)
        {
            U data2 = InternalParse2(file1, file2, data1);
            using (TimeUtils.MeasureTime())
                Part2(data2, settings);
        }
    }

    private static readonly Dictionary<int, Dictionary<int, string>> _names = [];

    private static string GetDayName(Type type)
    {
        var yearMatch = YearPattern().Match(type.Namespace ?? "");
        if (!yearMatch.Success)
            return type.Name;
        var dayMatch = DayPattern().Match(type.Name);
        if (!dayMatch.Success)
            return type.Name;
        var year = yearMatch.Groups["year"].ValueSpan.ToInt();
        var day = dayMatch.Groups["day"].ValueSpan.ToInt();
        if (!_names.TryGetValue(year, out var dayNames))
        {
            try
            {
                var names = FileReference.Resource(type, "names.txt").GetLines();
                dayNames = [];
                foreach (var line in names)
                {
                    var nameMatch = DayNamePattern().Match(line);
                    if (nameMatch.Success)
                        dayNames[nameMatch.Groups["day"].ValueSpan.ToInt()] = line;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: failed to fetch names for year {year}");
                Console.WriteLine(e);
                dayNames = [];
            }
            _names[year] = dayNames;
        }
        return dayNames.GetValueOrDefault(day, type.Name);
    }

    [GeneratedRegex(@"^AdventOfCode\._(?<year>\d+)$")]
    private static partial Regex YearPattern();
    [GeneratedRegex(@"^Day(?<day>\d+)$")]
    private static partial Regex DayPattern();
    [GeneratedRegex(@"^Day (?<day>\d+):")]
    private static partial Regex DayNamePattern();
}
