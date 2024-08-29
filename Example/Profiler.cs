namespace Example
{
    using Hexa.NET.ImGui;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Profiler
    {
        private static readonly Dictionary<string, long> timestamps = [];

        public static void Begin(string name)
        {
            timestamps[name] = Stopwatch.GetTimestamp();
        }

        public static double End(string name)
        {
            var end = Stopwatch.GetTimestamp();
            var start = timestamps[name];
            return (end - start) / (double)Stopwatch.Frequency;
        }

        public static void EndImGui(string name)
        {
            var duration = End(name);
            ImGui.Text($"{name}: {duration * 1000}ms");
        }
    }
}