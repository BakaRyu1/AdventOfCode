using AdventOfCode.Utils;
using System.Text;

namespace AdventOfCode._2015;
internal class Day10 : DayRunner<string>
{
    public override string Parse(FileReference file)
    {
        return file.GetText();
    }

    public override void Part1(string data, RunSettings settings)
    {
        var result = data;
        if (settings.Verbose)
            Console.WriteLine(data);
        for (var i = 0; i < 40; ++i)
        {
            result = EnunciateString(result);
            if (settings.Verbose)
                Console.WriteLine(" -> " + result);
        }
        Console.WriteLine($"The final string length is {result.Length}");
    }

    public override void Part2(string data, RunSettings settings)
    {
        var result = data;
        if (settings.Verbose)
            Console.WriteLine(data);
        for (var i = 0; i < 50; ++i)
        {
            result = EnunciateString(result);
            if (settings.Verbose)
                Console.WriteLine(" -> " + result);
        }
        Console.WriteLine($"The final string length is {result.Length}");
    }

    private static string EnunciateString(string input)
    {
        var pos = 0;
        var sb = new StringBuilder();
        while (pos < input.Length)
        {
            var ch = input[pos++];
            var count = 1;
            while (pos < input.Length && input[pos] == ch)
            {
                ++count;
                ++pos;
            }
            sb.Append(count.ToString());
            sb.Append(ch);
        }
        return sb.ToString();
    }
}
