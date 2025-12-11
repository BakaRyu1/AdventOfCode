using AdventOfCode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode._2025;
internal class Day11 : DayRunner<Day11.Device[]>
{
    public struct Device()
    {
        public string Name = "";
        public List<string> Outputs = [];
    }
    public override Device[] Parse(FileReference file)
    {
        var devices = new List<Device>();
        foreach (var line in file.GetLines())
        {
            var pos = line.IndexOf(':');
            if (pos < 0)
                throw new InvalidOperationException($"Invalid device specification: {line}");
            devices.Add(new()
            {
                Name = line.AsSpan(0, pos).Trim().ToString(),
                Outputs = [.. line.AsSpan(pos + 1).SplitAsStrings()]
            });
        }
        return [.. devices];
    }

    public override void Part1(Device[] data, RunSettings settings)
    {
        var devices = data.ToDictionary(item => item.Name);
        var count = CountPaths(devices, "you", "out", []);
        Console.WriteLine($"There are {count} valid paths.");
    }

    public override void Part2(Device[] data, RunSettings settings)
    {
        var devices = data.ToDictionary(item => item.Name);
        var svrFft = CountPaths(devices, "svr", "fft", []);
        if (settings.Verbose)
            Console.WriteLine($"svr->fft: {svrFft} paths");
        var svrDac = CountPaths(devices, "svr", "dac", []);
        if (settings.Verbose)
            Console.WriteLine($"svr->dac: {svrDac} paths");
        var dacFft = CountPaths(devices, "dac", "fft", []);
        if (settings.Verbose)
            Console.WriteLine($"dac->fft: {dacFft} paths");
        var fftDac = CountPaths(devices, "fft", "dac", []);
        if (settings.Verbose)
            Console.WriteLine($"fft->dac: {fftDac} paths");
        var fftOut = CountPaths(devices, "fft", "out", []);
        if (settings.Verbose)
            Console.WriteLine($"fft->out: {fftOut} paths");
        var dacOut = CountPaths(devices, "dac", "out", []);
        if (settings.Verbose)
            Console.WriteLine($"dac->out: {dacOut} paths");
        var count = (svrFft * fftDac * dacOut) + (svrDac * dacFft * fftOut);
        Console.WriteLine($"There are {count} valid paths.");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day11), settings.Example ? "day11-example1.txt" : "day11-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day11), "day11-example2.txt") : null);
    }

    private static long CountPaths(Dictionary<string, Device> devices, string from, string to, Dictionary<string, long> cache)
    {
        if (from == to)
        {
            return 1;
        }    
        if (cache.TryGetValue(from, out var cachedCount))
            return cachedCount;
        if (!devices.TryGetValue(from, out var device))
            return 0;
        var count = 0L;
        foreach (var output in device.Outputs)
        {
            count += CountPaths(devices, output, to, cache);
        }
        cache[from] = count;
        return count;
    }
}
