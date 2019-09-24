using System;
using System.Threading.Tasks;

namespace AsynchronousProgramming
{
    class Program
    {
        static void Main(string[] args)
        {
          Foo();
        }

        private static async void Foo()
        {
            var num = await CalculateAsync();
            Console.WriteLine(num);
        }

        private async static Task<int> CalculateAsync()
        {
            return 123;
        }
    }
}
