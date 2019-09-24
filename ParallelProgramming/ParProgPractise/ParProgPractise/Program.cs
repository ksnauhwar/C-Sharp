using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ParProgPractise
{
    class Program
    {
        public static BlockingCollection<int> bc = new BlockingCollection<int>(new ConcurrentBag<int>(), 10);
        public static Random random = new Random();
        public static CancellationTokenSource cts = new CancellationTokenSource();

        public static void Producer()
        {
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                var num = random.Next(100);
                bc.Add(num);
                Console.WriteLine($"+{num}");
                Thread.Sleep(random.Next(1000));
            }
        }

        public static void Consumer()
        {
            foreach (var item in bc.GetConsumingEnumerable())
            {
                Console.WriteLine($"-{item}");
                Thread.Sleep(random.Next(1000));
            }
        }

        static void Main(string[] args)
        {
            //ProducerConsumer();
            //BreakDemo();
            //var stop = Task.Factory.StartNew(StopDemo);
            //stop.Wait();
            PartitionsDemo();
            Console.ReadKey();
        }

        private static void PartitionsDemo()
        {
            var random = new Random();
            Console.WriteLine($"{partitions}");
            var ppl = Enumerable.Range(1, 1000).Select(_ => new Person()
            {
                Age = random.Next(50),
                Name = string.Join("",Enumerable.Range(1,5).Select(x=> "abcdefghijklmnopqrstuvwxyz".ElementAt(random.Next(26))))
            });
            var partitioner = Partitioner.Create(ppl.ToList(),true);
            var lst = new List<string>();
            var padlock = new ReaderWriterLockSlim();
            Parallel.ForEach(partitioner, (person) =>
            {
                padlock.EnterWriteLock();
                lst.Add(person.Name);
                padlock.ExitWriteLock();
            });
            Parallel.ForEach(lst, (name) =>
            {
                Console.Write($"{name}\t");
            });
            Console.WriteLine($"Number of name processed {lst.Count}");

        }

        private static void StopDemo()
        {
            ParallelLoopResult result = Parallel.For(1, 10, (num, state) =>
            {
                Thread.Sleep(1000);
                if (state.IsStopped)
                {
                    Console.WriteLine($"stopping exexution of iteration {num}");
                    return;
                }
                if (num * 5 > 15)
                {
                    state.Stop();
                    Console.WriteLine($"Stop called on iteration {num}");
                    return;
                }
                Console.WriteLine($"Iteration for stop demo is {num}[{Task.CurrentId}]");
            });
            Console.WriteLine($"Stop demo completed");
        }

        private static void BreakDemo()
        {
            ParallelLoopResult result = Parallel.For(1, 10, (num, state) => 
            {
                Thread.Sleep(3000);
                if (state.ShouldExitCurrentIteration)
                {
                    if (num > state.LowestBreakIteration)
                        return;
                }
                if (num * 5 > 15)
                {
                    state.Break();
                    if (state.LowestBreakIteration.HasValue)
                    {
                        Console.WriteLine($"Lowest break iteration is {state.LowestBreakIteration}");
                    }
                    return;
                }
                Console.WriteLine($"Iteration for {num}[{Task.CurrentId}]");
            });
            Console.WriteLine($"The loop was {(result.IsCompleted == false?"Cancelled":"Completed")} at iteration {(result.LowestBreakIteration.HasValue ? result.LowestBreakIteration:0)}");
        }

        private static void ProducerConsumer()
        {
            var producer = new Task(Producer, cts.Token);
            producer.Start();
            var consumer = new Task(Consumer, cts.Token);
            consumer.Start();
            //Task.WaitAll(new[] { producer, consumer });
            Console.ReadKey();
            cts.Cancel();
            Console.ReadKey();
            Console.WriteLine($"{(producer.IsCanceled ? "Cancelled" : producer.Status.ToString())} {consumer.Status}");
        }
    }
}
