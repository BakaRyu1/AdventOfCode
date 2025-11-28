using AdventOfCode.Utils;
using System.Security.Cryptography;
using System.Text;

namespace AdventOfCode._2015;
internal class Day04 : DayRunner<string>
{
    public override string Parse(FileReference file)
    {
        return file.GetText();
    }

    private static (byte[], int) InitBuffer(string key)
    {
        var buffer = new byte[255];
        var count = Encoding.UTF8.GetBytes(key, buffer);
        return (buffer, count);
    }

    private static int AppendBuffer(byte[] buffer, int startPos, ReadOnlySpan<char> appended)
    {
        return startPos + Encoding.UTF8.GetBytes(appended, buffer.AsSpan(startPos));
    }

    public override void Part1(string data, RunSettings settings)
    {
        var number = 1;
        var found = false;
        var (buffer, startPos) = InitBuffer(data);
        do
        {
            var totalLength = AppendBuffer(buffer, startPos, number.ToString());
            var result = MD5.HashData(buffer.AsSpan(0, totalLength));
            if (result[0] == 0x00 && result[1] == 0x00 && (result[2] & 0xF0) == 0x00)
            {
                Console.WriteLine($"Found number {number} with 5 leading zeroes.");
                if (settings.Verbose)
                    Console.WriteLine($"MD5: {Convert.ToHexString(result)}");
                found = true;
            }
            else
                ++number;
        } while (!found);
    }

    public override void Part2(string data, RunSettings settings)
    {
        var number = 1;
        var found = false;
        var (buffer, startPos) = InitBuffer(data);
        do
        {
            var totalLength = AppendBuffer(buffer, startPos, number.ToString());
            var result = MD5.HashData(buffer.AsSpan(0, totalLength));
            if (result[0] == 0x00 && result[1] == 0x00 && result[2] == 0x00)
            {
                Console.WriteLine($"Found number {number} with 6 leading zeroes.");
                if (settings.Verbose)
                    Console.WriteLine($"MD5: {Convert.ToHexString(result)}");
                found = true;
            }
            else
                ++number;
        } while (!found);
    }
}
