// See https://aka.ms/new-console-template for more information
namespace AdventOfCode;

public static class Program
{
    public static readonly Type[] Year2015 = [
        typeof(_2015.Day01),
        typeof(_2015.Day02),
        typeof(_2015.Day03),
    ];
    public static readonly Type[] Year2023 = [
        typeof(_2023.Day01),
        typeof(_2023.Day02),
        typeof(_2023.Day03),
        typeof(_2023.Day04),
        typeof(_2023.Day05),
        typeof(_2023.Day06),
        typeof(_2023.Day07),
        typeof(_2023.Day08),
        typeof(_2023.Day09),
        typeof(_2023.Day10),
        typeof(_2023.Day11),
        typeof(_2023.Day12),
        typeof(_2023.Day13),
        typeof(_2023.Day14),
        typeof(_2023.Day15),
        typeof(_2023.Day16),
        typeof(_2023.Day17),
        typeof(_2023.Day18),
    ];

    public static readonly Type[] Year2024 = [
        typeof(_2024.Day01),
        typeof(_2024.Day02),
        typeof(_2024.Day03),
        typeof(_2024.Day04),
        typeof(_2024.Day05),
        typeof(_2024.Day06),
        typeof(_2024.Day07),
        typeof(_2024.Day08),
        typeof(_2024.Day09),
        typeof(_2024.Day10),
        typeof(_2024.Day11),
        typeof(_2024.Day12),
        typeof(_2024.Day13),
        typeof(_2024.Day14),
        typeof(_2024.Day15),
        typeof(_2024.Day16),
        typeof(_2024.Day17),
        typeof(_2024.Day18),
        typeof(_2024.Day19),
        typeof(_2024.Day20),
        typeof(_2024.Day21),
        typeof(_2024.Day22),
        typeof(_2024.Day23),
        typeof(_2024.Day24),
        typeof(_2024.Day25)
    ];

    public static readonly Type[] Year2025 = [
    ];

    public const int FirstYear = 2015;
    public static readonly Type[][] Years =
    [
        Year2015,
        [], // Year2016
        [], // Year2017
        [], // Year2018
        [], // Year2019
        [], // Year2020
        [], // Year2021
        [], // Year2022
        Year2023,
        Year2024,
        Year2025
    ];

    public static bool IsYearAvailable(int year) => year >= FirstYear && year - FirstYear < Years.Length;

    private static void RunDay(Type day, RunSettings settings)
    {
        var instance = Activator.CreateInstance(day) as IDayRunner ?? throw new InvalidOperationException();
        instance.Run(settings);
    }
    public static void RunAll(Type[] year, RunSettings? settings = null)
    {
        settings ??= new RunSettings();
        foreach (var day in year)
            RunDay(day, settings.Value);
    }
    public static void RunAll(int year, RunSettings? settings = null)
    {
        if (IsYearAvailable(year))
            RunAll(Years[year - FirstYear], settings);
    }

    public static void Run(Type[] year, int day, RunSettings? settings = null)
    {
        if (day >= 1 && day <= year.Length)
            RunDay(year[day - 1], settings ?? new RunSettings());
        else
            throw new InvalidOperationException();
    }

    public static void Run(int year, int day, RunSettings? settings = null)
    {
        if (IsYearAvailable(year))
            Run(Years[year - FirstYear], day, settings);
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