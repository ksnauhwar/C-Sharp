using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace ConcurrentCollections
{
    class Program
    {
        static ConcurrentDictionary<string, string> capitals = new ConcurrentDictionary<string, string>();
        private static string removed;

        static BlockingCollection<int> coll = new BlockingCollection<int>(new ConcurrentBag<int>(), 10);

        static CancellationTokenSource cts = new CancellationTokenSource();

        static Random random = new Random();

        static void ProducerAndConsumer()
        {
            var producer = Task.Factory.StartNew(RunProducer,cts.Token);
            var consumer = Task.Factory.StartNew(RunConsumer,cts.Token);
            try
            {
                Task.WaitAll(new[] { producer, consumer }, cts.Token);
            }
            catch (OperationCanceledException ae)
            {
                Console.WriteLine("Exception handled");
            }
            finally
            {
                Console.WriteLine($"Consumer status : {consumer.Status}, Producer status: {producer.Status}");
            }

        }
        
        static void RunProducer()
        {
            try
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    int i = random.Next(100);
                    coll.Add(i);
                    Console.WriteLine($"+{i}");
                }
            }
            catch (OperationCanceledException oc)
            {
                Console.WriteLine("Producer exception handled");
            }
        }

        static void RunConsumer()
        {
            try
            {
                cts.Token.ThrowIfCancellationRequested();
                foreach (var item in coll.GetConsumingEnumerable())
                {
                    Console.WriteLine($"-{item}");
                }
            }
            catch (OperationCanceledException oe)
            {
                Console.WriteLine($"Consumer exception handled");
              
            }
        }

        static void AddParis()
        {
            bool success = capitals.TryAdd("France", "Paris");
            var who = Task.CurrentId.HasValue ? "Task " + Task.CurrentId : "Main thread";
            Console.WriteLine($"{who} {(success ? "added" : "failed to add") } the element");
        }

        static void Main(string[] args)
        {
            //ConDictionary();

            //ConBag();


            var t =Task.Factory.StartNew(ProducerAndConsumer,cts.Token);

            Console.ReadKey();
            cts.Cancel();

            Console.ReadKey();
        }

        private static void ConBag()
        {
            var bag = new ConcurrentBag<int>();

            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                var i1 = i;
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bag.Add(i1);
                }));
            }

            Task.WhenAll(tasks).ContinueWith(t =>
            {
                foreach (var item in bag)
                {
                    Console.WriteLine(item);
                }
            });
        }

        private static void ConDictionary()
        {
            Task.Factory.StartNew(AddParis).Wait();
            AddParis();

            capitals["Russia"] = "Leningrad";
            capitals.AddOrUpdate("Russia", "Moscow",
                (k, old) => old + " --> Moscow");
            Console.WriteLine(capitals["Russia"]);
            var capitalOfRussia = capitals.GetOrAdd("Russia", "Moscow");
            Console.WriteLine(capitalOfRussia);
            var isRemoved = capitals.TryRemove("Russia", out removed);
            Console.WriteLine($"{(isRemoved ? removed : "not removed")}");
        }
    }
}
