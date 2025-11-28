using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day04 : DayRunner<Array2D<char>>
{
    public override Array2D<char> Parse(FileReference file)
    {
        var lines = file.GetLines().ToArray();
        var width = lines.Min(line => line.Length);
        if (lines.Any(line => line.Length != width))
        {
            Console.Error.WriteLine("Invalid word search: at least one line isn't the same length.");
        }
        return Array2D<char>.From(lines);
    }

    public override void Part1(Array2D<char> wordSearch, RunSettings settings)
    {
        int count = 0;
        string XMAS = "XMAS";
        int minCoord = XMAS.Length - 1;
        int maxY = wordSearch.Height - XMAS.Length;
        int maxX = wordSearch.Width - XMAS.Length;
        var matched = Array2D<bool>.FromSize(wordSearch, false);
        
        for (int y = 0; y < wordSearch.Height; ++y)
        {
            for (int x = 0; x < wordSearch.Width; ++x)
            {
                if (wordSearch[x, y] == 'X')
                {
                    if (x >= minCoord)
                    {
                        if (y >= minCoord && MatchWord(wordSearch, XMAS, x, y, -1, -1))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, -1, -1);
                            ++count;
                        }
                        if (MatchWord(wordSearch, XMAS, x, y, -1, 0))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, -1, 0);
                            ++count;
                        }
                        if (y <= maxY && MatchWord(wordSearch, XMAS, x, y, -1, 1))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, -1, 1);
                            ++count;
                        }
                    }
                    if (y >= minCoord && MatchWord(wordSearch, XMAS, x, y, 0, -1))
                    {
                        if (settings.Verbose)
                            MarkWord(matched, XMAS, x, y, 0, -1);
                        ++count;
                    }
                    if (y <= maxY && MatchWord(wordSearch, XMAS, x, y, 0, 1))
                    {
                        if (settings.Verbose)
                            MarkWord(matched, XMAS, x, y, 0, 1);
                        ++count;
                    }
                    if (x <= maxX)
                    {
                        if (y >= minCoord && MatchWord(wordSearch, XMAS, x, y, 1, -1))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, 1, -1);
                            ++count;
                        }
                        if (MatchWord(wordSearch, XMAS, x, y, 1, 0))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, 1, 0);
                            ++count;
                        }
                        if (y <= maxY && MatchWord(wordSearch, XMAS, x, y, 1, 1))
                        {
                            if (settings.Verbose)
                                MarkWord(matched, XMAS, x, y, 1, 1);
                            ++count;
                        }
                    }
                }
            }
        }
        if (settings.Verbose)
        {
            PrintWordSearch(wordSearch, matched);
            Console.WriteLine();
        }
        Console.WriteLine("XMAS count is " + count);
    }
    public override void Part2(Array2D<char> wordSearch, RunSettings settings)
    {
        int count = 0;
        int maxX = wordSearch.Width - 2;
        int maxY = wordSearch.Height - 2;
        string MAS = "MAS";
        var matched = Array2D<bool>.FromSize(wordSearch, false);

        for (int y = 1; y <= maxY; ++y)
        {
            for (int x = 1; x <= maxX; ++x)
            {
                if (wordSearch[x, y] == 'A')
                {
                    if ((MatchWord(wordSearch, MAS, x - 1, y - 1, 1, 1) || MatchWord(wordSearch, MAS, x + 1, y + 1, -1, -1))
                        && (MatchWord(wordSearch, MAS, x + 1, y - 1, -1, 1) || MatchWord(wordSearch, MAS, x - 1, y + 1, 1, -1)))
                    {
                        if (settings.Verbose)
                        {
                            MarkWord(matched, MAS, x - 1, y - 1, 1, 1);
                            MarkWord(matched, MAS, x - 1, y + 1, 1, -1);
                        }
                        ++count;
                    }
                }
            }
        }
        if (settings.Verbose)
        {
            PrintWordSearch(wordSearch, matched);
            Console.WriteLine();
        }
        Console.WriteLine("X-MAS count is " + count);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day04), settings.Example ? "day04-example.txt" : "day04-input.txt");
    }

    private static bool MatchWord(Array2D<char> wordSearch, string word, int x, int y, int dx, int dy)
    {
        foreach (var ch in word)
        {
            if (wordSearch[x, y] != ch)
                return false;
            x += dx;
            y += dy;
        }
        return true;
    }
    private static void MarkWord(Array2D<bool> target, string word, int x, int y, int dx, int dy)
    {
        foreach (var ch in word)
        {
            target[x, y] = true;
            x += dx;
            y += dy;
        }
    }
    private static void PrintWordSearch(Array2D<char> wordSearch, Array2D<bool> matched)
    {
        for (int y = 0; y < wordSearch.Height; ++y)
        {
            for (int x = 0; x < wordSearch.Width; ++x)
            {
                if (matched[x, y])
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                    Console.ResetColor();
                Console.Write(wordSearch[x, y]);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
