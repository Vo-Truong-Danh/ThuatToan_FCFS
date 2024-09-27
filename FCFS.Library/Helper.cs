using System.Runtime.InteropServices;

namespace FCFS.Library
{
    public static class Helper
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public static IEnumerable<T> Interleave<T>(params IEnumerable<T>[] enumerables)
        {
            var enumerators = enumerables.Select(e => e.GetEnumerator()).ToList();
            while (enumerators.Any())
            {
                enumerators.RemoveAll(e =>
                {
                    var ended = !e.MoveNext();
                    if (ended) e.Dispose();
                    return ended;
                });

                foreach (var enumerator in enumerators)
                    yield return enumerator.Current;
            }
        }
    }

    public enum BurstType
    {
        IO,
        CPU,
    }

    public class Data
    {
        public BurstType Type { get; set; }
        public uint Time { get; set; }
        public string Text => $"{Type} Burst Time {Time}";
        public uint Value { get; set; }

        public Data(BurstType type, uint time, uint value)
        {
            Type = type;
            Time = time;
            Value = value;
        }
    }
}
