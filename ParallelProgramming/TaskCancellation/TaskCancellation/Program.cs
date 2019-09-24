using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCancellation
{
    class Program
    {
        static void Main(string[] args)
        {
            //CancellationBasic();

            CompositeCancellation();
        }

        private static void CompositeCancellation()
        {
            var regular = new CancellationTokenSource();
            var emergency = new CancellationTokenSource();
            var preventative = new CancellationTokenSource();

            var paranoid = CancellationTokenSource.CreateLinkedTokenSource(regular.Token, emergency.Token, preventative.Token);
            paranoid.Token.Register(() => Console.WriteLine("Paranoid handler called"));
            Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (true)
                {
                    Console.WriteLine(i++);
                    paranoid.Token.ThrowIfCancellationRequested();
                }
            }, paranoid.Token);

            Console.ReadKey();

            regular.Cancel();

            Console.WriteLine("Main program has ended");

            Console.ReadKey();
        }

        private static void CancellationBasic()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(() => Console.WriteLine("Cancellation has been requested"));
            var task = new Task(() =>
            {
                int i = 0;
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    Console.WriteLine(i++);
                }

            }, token);
            task.Start();

            Task.Factory.StartNew(() =>
            {
                token.WaitHandle.WaitOne();
                Console.WriteLine("Wait handle has been released and therefore cancellation was requested");
            });
            Console.ReadKey();
            cts.Cancel();

            Console.WriteLine("Main Program Ended");
            Console.ReadKey();
        }
    }
}
