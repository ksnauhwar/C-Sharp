using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;


namespace ParallelLoops
{
    class Program
    {

        //static IEnumerable<int> Range(int start, int end, int step)
        //{
        //    for (int i = start; i <= end; i+=step)
        //    {
        //        yield return i;
        //    }
        //}

        static void Main(string[] args)
        {
            //ParallelLoops();
            //ParallelLoopCancellation();
            //ThreadLocalStorage();
            //var benchmark = BenchmarkRunnerCore.Run(null, null);
            Practise();
        }

        private static void Practise()
        {
            var random = new Random();
            
            const string alphabets = "abcdefghijklmnopqrstuvwxyz";

            var ppl = ParallelEnumerable.Range(1, 100000).Select(_ => {
                var name = new string(ParallelEnumerable.Range(1, random.Next(10)).Select(index => alphabets.ElementAt(random.Next(1, 26))).ToArray());
                return new Person(name);
            }).ToList();

            var part = Partitioner.Create(0, 10,2);
            Parallel.ForEach(part, rangeSize =>
            {
                Parallel.For(rangeSize.Item1, rangeSize.Item2, index =>
                {
                    ppl[index].Age = random.Next(50);
                });
            });

            var newPart = Partitioner.Create(ppl);

        }

        [Benchmark]
        public void SquareEachValue()
        {
            int count = 100000;
            var results = new int[count];
            var values = Enumerable.Range(0, count);
            Parallel.ForEach(values, x => results[x] = (int)Math.Pow(x, 2));
        }
        [Benchmark]
        public void SquareEachValueChunked()
        {
            int count = 100000;
            var results = new int[count];
            var values = Enumerable.Range(0, count);

            var part = Partitioner.Create(0, count, 10000);
            Parallel.ForEach(part, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    results[i] = (int)Math.Pow(i, 2);
                }
            });

        }


        private static void ThreadLocalStorage()
        {
            var sum = 0;
            //Parallel.For(1, 1001, x => Interlocked.Add(ref sum,x));
            Parallel.For(1, 1001,
                () => 0,
                (x, state, tls) =>
                {
                    tls += x;
                    Console.WriteLine($"Thread local storage {tls}");
                    return tls;
                },
                partialSum =>
                {
                    Console.WriteLine($"Interlocked partial sum {partialSum}");
                    Interlocked.Add(ref sum, partialSum);
                });
            Console.WriteLine(sum);
        }

        private static void ParallelLoopCancellation()
        {

            var cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            var result = Parallel.For(1, 20, po, (i, state) =>
             {
                 Console.WriteLine($"{i}[{Task.CurrentId}]");
                 if (i == 10)
                 {
                            //state.Stop();
                            //state.Break();
                            cts.Cancel();
                 }

             });
            if (result.LowestBreakIteration.HasValue)
            {
                Console.WriteLine($"{result.LowestBreakIteration.Value} broken");
            }
            else
            {
                Console.WriteLine("Not broken");
            }
        }

        private static void ParallelLoops()
        {
            var a = new Action(() => Console.WriteLine($"Task A {Task.CurrentId}"));
            var b = new Action(() => Console.WriteLine($"Task B {Task.CurrentId}"));
            var c = new Action(() => Console.WriteLine($"Task C {Task.CurrentId}"));
            //Parallel.Invoke(a, b, c);

            //Parallel.For(1, 11, i => Console.WriteLine($"{i}"));
            var words = new string[] { "oh", "what", "a", "night" };
            var po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 2;
            Parallel.ForEach(words, po, word =>
             {
                 Console.WriteLine($"{word} is of length {word.Length} (task id is {Task.CurrentId})");
             });

            //Parallel.ForEach(Range(1, 10, 2), Console.WriteLine);
        }
    }
}