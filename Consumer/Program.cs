using DemoCache;
using System;
using System.Diagnostics;
using System.Linq;

namespace Consumer
{
    class Program
    {
        static void Main()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var memoryMap = new MemoryMap<BigDataParent>("SomeKey");
            var sampleBigData = memoryMap.Load();

            Console.WriteLine(
                $"memoryMap.Load elapsed time: {stopWatch.Elapsed}");

            Console.WriteLine(
                $"Loaded '{sampleBigData.Description}'");
            Console.WriteLine(
                $"Total SomeDouble: {sampleBigData.BigDataChildren.Sum(x => x.SomeDouble)}");

            Console.ReadKey();
        }
    }
}
