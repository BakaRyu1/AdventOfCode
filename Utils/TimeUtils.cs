namespace AdventOfCode.Utils;

internal static class TimeUtils
{
    private class Chronometer : IDisposable
    {
        private long Start;
        public Chronometer()
        {
            Start = Environment.TickCount;
        }

        public void Dispose()
        {
            var time = Environment.TickCount - Start;
            Console.WriteLine($"\t(took {time}ms)");
        }
    }
    public static IDisposable MeasureTime() => new Chronometer();
}
