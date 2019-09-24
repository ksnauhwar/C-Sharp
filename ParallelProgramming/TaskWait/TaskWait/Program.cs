using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskWait
{
    class Program
    {
        static void Main(string[] args)
        {
            //WaitingInsideATask();
            WaitForATask();
        }

        private static void WaitForATask()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var task = new Task(() =>
            {
                Console.WriteLine("I will wait for 5 seconds");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                }
                Console.WriteLine("I'm done");
            }, token);
            task.Start();

            var task2 = Task.Factory.StartNew(() => Thread.Sleep(3000));

            Console.ReadKey();
            cts.Cancel();

            //task.Wait(); //Wait gives an exception if a task is cancelled irrespective of whether the token was passed
            Task.WaitAny(new[] { task, task2 }, 4000);//WaitAny/WaitAll gives an exception if a task is cancelled and only if the token was passed to it
            //Console.WriteLine($"{task.Status} {task2.Status}");
            Console.WriteLine("Main Program ended");
            Console.ReadKey();
        }

        private static void WaitingInsideATask()
        {
            var cts = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("You have 5 seconds.Press any key to disarm the bomb.");
                var isCancelled = cts.Token.WaitHandle.WaitOne(5000);
                //Thread.Sleep(5000);
                //Thread.SpinWait(1);
                //SpinWait.SpinUntil(()=>isCancelled == false);

                Console.WriteLine(isCancelled ? "Bomb has been disarmed" : "BOOM!");
            }, cts.Token);

            Console.ReadKey();
            cts.Cancel();

            Console.WriteLine("Main thread Ended...");
            Console.ReadKey();
        }
    }
}
