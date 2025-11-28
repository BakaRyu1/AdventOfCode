// See https://aka.ms/new-console-template for more information
using System.Reflection;

namespace AdventOfCode;

public static class Program
{
    private static void RunDay(Type day, RunSettings settings)
    {
        var instance = Activator.CreateInstance(day) as IDayRunner
            ?? throw new InvalidOperationException();
        instance.Run(settings);
    }
    public static void RunAll(Type[] year, RunSettings? settings = null)
    {
        settings ??= new RunSettings();
        foreach (var day in year)
            RunDay(day, settings.Value);
    }
    public static void RunAll(int year, RunSettings? settings = null)
        => RunAll(GetYear(year), settings);

    public static void Run(Type[] year, int day, RunSettings? settings = null)
    {
        var dayType = GetDay(year, day)
            ?? throw new InvalidOperationException();
        RunDay(dayType, settings ?? new RunSettings());
    }

    public static void Run(int year, int day, RunSettings? settings = null)
        => Run(GetYear(year), day, settings);

    public static Type[] GetYear(int year)
    {
        var ns = $"AdventOfCode._{year}";
        return [.. Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass 
                && t.Namespace == ns
                && t.IsAssignableTo(typeof(IDayRunner)))
            .OrderBy(t => t.Name)];
    }

    public static Type? GetDay(Type[] year, int day)
    {
        var name = $"Day{day:00}";
        return year.FirstOrDefault(t => t.Name == name);
    }

    private static void Main(string[] args)
    {
        var settings = new RunSettings()
        {
            Example = false,
            Verbose = true,
            Part1 = true,
            Part2 = true
        };
        //RunAll(2024, settings);
        Run(2015, 3, settings);
    }
}