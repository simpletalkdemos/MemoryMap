using DemoCache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Producer
{
    class Program
    {
        static void Main()
        {
            var sampleBigDataIn = CreateBigDataParent(10000);
            Console.WriteLine(
                $"Created '{sampleBigDataIn.Description}'");            
            Console.WriteLine(
                $"with a total SomeDouble: {sampleBigDataIn.BigDataChildren.Sum(x => x.SomeDouble)}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var memoryMap = new MemoryMap<BigDataParent>("SomeKey");
            memoryMap.Create(sampleBigDataIn);

            Console.WriteLine(
                $"memoryMap.Create elapsed time: {stopWatch.Elapsed}");

            var sampleBigDataOut = memoryMap.Load();

            Console.WriteLine(
                $"Create and Load elapsed time: {stopWatch.Elapsed}");
            stopWatch.Stop();
                        
            var childId = sampleBigDataIn.BigDataChildren[0].Id;
            Console.WriteLine($"BigDataParent comparison check for childId: {childId}");

            var totalSomeDoubleIn = sampleBigDataIn.BigDataChildren
                .Where(x => x.Id.Equals(childId)).Sum(x => x.SomeDouble);
            Console.WriteLine($"totalSomeDoubleIn : {totalSomeDoubleIn}");

            var totalSomeDoubleOut = sampleBigDataOut.BigDataChildren
                .Where(x => x.Id.Equals(childId)).Sum(x => x.SomeDouble);           
            Console.WriteLine($"totalSomeDoubleOut: {totalSomeDoubleOut}");

            Console.ReadKey();
        }

        private static BigDataParent CreateBigDataParent(int count)
        {
            var random = new Random();

            var bigDataChildList = new List<BigDataChild>();
            for (var i = 0; i < count; i++)
                bigDataChildList.Add(
                    new BigDataChild {
                        Id = random.Next(0, 100),
                        SomeDouble = random.NextDouble(),
                        SomeString = string.Empty.PadLeft(random.Next(1,1000), 'x') }
                );

            return new BigDataParent {
                Description = $"BigDataParent with {count} BigDataChild",
                BigDataChildren = bigDataChildList };        
        }
    }
}
