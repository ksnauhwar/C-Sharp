using System;
using System.Threading;
using System.Threading.Tasks;
namespace TaksBasics
{
    class Program
    {
        public static void Write(char c)
        {
            int i = 1000;
            while (i-- > 0)
                Console.Write(c);
        }

        public static void Write(object o)
        {
            int i = 1000;
            while (i-- > 0)
            {
                Console.Write(o);
            }
        }

        public static int TextLength(object o)
        {
            Console.WriteLine($"The task id {Task.CurrentId} with {o} argument");
            Thread.Sleep(7000);
            return o.ToString().Length;
        }

        static void Main(string[] args)
        {
            // BasicTask();
            // BasicTaskWithObjectArgs();
            TasksWithReturnType();
            Console.WriteLine("Main program ended");
            Console.ReadKey();
        }

        private static void TasksWithReturnType()
        {
            var task1 = Task.Factory.StartNew(TextLength, "testing");
            var task2 = new Task<int>(TextLength, 123);
            task2.Start();

            Console.WriteLine($"Results of task1 is {task1.Result}");
            Console.WriteLine($"Results of task2 is {task2.Result}");
        }

        private static void BasicTaskWithObjectArgs()
        {
            Task.Factory.StartNew(Write, "?");
            Task task = new Task(Write, ".");
            task.Start();
        }

        private static void BasicTask()
        {
            Task.Factory.StartNew(() => Write('?'));
            var task = new Task(() => Write('.'));
            task.Start();
            Write('-');
        }
    }
}
