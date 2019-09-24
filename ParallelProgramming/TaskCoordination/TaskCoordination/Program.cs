using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordination
{
    class Program
    {

        static void Main(string[] args)
        {
            //WhenAll();

            //ContinueWith();

            //ContinueWhenAllAny();

            //ChildTask();

            //BarrierDemo();

            //CountdownEventDemo();

            //ManualAutoResetDemo();

            SemaphoreDemo();
        }

        private static void SemaphoreDemo()
        {
            var semphore = new SemaphoreSlim(2,10);
            for (int i = 0; i < 20; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Entering task {Task.CurrentId}");
                    semphore.Wait();
                    Console.WriteLine($"Processing task {Task.CurrentId}");
                });
            }

            while (true)
            {
                Console.WriteLine($"Semaphore count is {semphore.CurrentCount}");
                Console.ReadKey();
                semphore.Release(1);
                //Console.WriteLine($"Semaphore count post release is {semphore.CurrentCount}");
            }
        }

        private static void ManualAutoResetDemo()
        {
            //ManualResetSlim();
            AutoResetDemo();
        }

        private static void AutoResetDemo()
        {
            var evt = new AutoResetEvent(false);

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Boiling the water");
                evt.Set();
                //evt.Set();//this does not cause the second waitone to unblock
                Thread.Sleep(3000);
                evt.Set();
            });
            var finalTask = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Waiting for the water to boil");
                evt.WaitOne();
                Console.WriteLine("Pouring water in cup");
                var isTeaMade = evt.WaitOne(6000);
                if (isTeaMade)
                {
                    Console.WriteLine("Enjoy your tea");
                }
                else
                {
                    Console.WriteLine("No tea for you");
                }
            });
            finalTask.Wait();
        }

        private static void ManualResetSlim()
        {
            var evt = new ManualResetEventSlim();

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Boiling the water");
                evt.Set();//Once the manual reset event is set it is set forever i.e all waits will execute simulatenaously
            });

            var task = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Waiting for the water to boil");
                evt.Wait();
                Console.WriteLine("Enjoy your tea");
            });
            task.Wait();
        }

        static Random random = new Random();
        static CountdownEvent cte = new CountdownEvent(5);
        private static void CountdownEventDemo()
        {
            for (int i = 0; i < 5; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Starting a new task with {Task.CurrentId}");
                    Thread.Sleep(random.Next(3000));
                    cte.Signal();
                    Console.WriteLine($"Finishing task with {Task.CurrentId}");
                });
            }

            var finalTask = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Entering the final task with id {Task.CurrentId}"); ;
                cte.Wait();
                Console.WriteLine("All tasks done");
            });

            finalTask.Wait();
        }

        static Barrier barrier = new Barrier(2, b =>
        {
            Console.WriteLine($"\nPhase {b.CurrentPhaseNumber} finished\n");
        });
        private static void BarrierDemo()
        {
            var water = Task.Factory.StartNew(Water);
            var cup = Task.Factory.StartNew(Cup);

            var task = Task.Factory.ContinueWhenAll(new[] { water, cup }, tasks =>
            {
                Console.WriteLine("Enjoying the tea");
            });

            task.Wait();
        }


        public static void Water()
        {
            Console.WriteLine("Boil water in kettle.");
            barrier.SignalAndWait();
            Console.WriteLine("Pour water in cup");
            barrier.SignalAndWait();
            Console.WriteLine("Put the kettle away");
        }
        public static void Cup()
        {
            Console.WriteLine("Finding a nice cup");
            barrier.SignalAndWait();
            Console.WriteLine("Add tea to cup");
            barrier.SignalAndWait();
        }

        private static void ChildTask()
        {
            var cts = new CancellationTokenSource();
            var parent = new Task(() =>
            {
                var child = new Task(() =>
                {
                    Console.WriteLine("Child task started");
                    //throw new Exception("some exception");
                    Thread.Sleep(3000);
                    Console.WriteLine("Child task ended");

                }, TaskCreationOptions.AttachedToParent);

                var continuationHandler = child.ContinueWith(t =>
                {
                    Console.WriteLine($"{t.Id} has completed with status {t.Status}");
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);

                var failureHandler = child.ContinueWith(t =>
                {
                    Console.WriteLine($"{t.Id} has completed with status {t.Status}");
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnFaulted);
                child.Start();
            });

            parent.Start();

            cts.Cancel();
            try
            {
                parent.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Exception handled");
                    return true;
                });
            }
        }

        private static void ContinueWhenAllAny()
        {
            var task = Task.Factory.StartNew(() => Console.WriteLine("task1"));
            var task2 = Task.Factory.StartNew(() => Console.WriteLine("task2"));

            var task3 = Task.Factory.ContinueWhenAny(new[] { task, task2 },
                tasks =>
                {
                    Console.WriteLine("continuation task");
                });

            task3.Wait();
        }

        private static void ContinueWith()
        {
            var task = Task.Factory.StartNew(() => Console.WriteLine("Boil the water"));

            var task2 = task.ContinueWith(t =>
            {
                Console.WriteLine("continuation task " + t.Id);
            });

            task2.Wait();//this wait is required
        }

        private static void WhenAll()
        {
            var task = new Task<string>(() => "Task 1");
            var task2 = new Task<string>(() => "Task 2");
            task.Start();
            task2.Start();
            //This is bad
            Task.WhenAll(new[] { task, task2 }).ContinueWith(t => Console.WriteLine(""));
        }
    }
}
