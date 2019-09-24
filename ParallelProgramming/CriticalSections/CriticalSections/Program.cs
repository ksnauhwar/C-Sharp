using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CriticalSections
{
    class Program
    {
        static SpinLock _sl = new SpinLock(); //For LockRecursion
        static void Main(string[] args)
        {
            SimpleLockAndInterLock();
            //SpinLock();
            //LockRecursion(5);
            //MutexDemo();
            //CompositeMutex();
            //GlobalMutex();
            //ReaderWriterLockDemo();
            Console.ReadKey();
        }

        private static void ReaderWriterLockDemo()
        {
            var random = new Random();
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            int x = 0;
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    rwLock.EnterReadLock();
                    rwLock.EnterWriteLock();
                    Console.WriteLine("Entered read lock");

                    if (x % 2 == 1)
                    {
                        x = 123;
                    }
                    
                    rwLock.ExitWriteLock();
                    Console.Write($"Value of x is {x}");
                    rwLock.ExitReadLock();
                    Console.WriteLine("Exited read lock");
                }));
            }
            Task.WaitAll(tasks.ToArray());
            while (true)
            {
                Console.ReadKey();
                rwLock.EnterWriteLock();
                Console.Write("Entered write lock");

                Console.Write($"x is set to {x = random.Next(10)}");

                rwLock.ExitWriteLock();
                Console.WriteLine("\nExited write lock");
            }
        }

        private static void GlobalMutex()
        {
            Mutex mutex;
            var appName = "MyApp";
            try
            {
                var lockTaken = Mutex.OpenExisting(appName);
                Console.WriteLine("App already running");
                return;

            }
            catch (WaitHandleCannotBeOpenedException e)
            {
                mutex = new Mutex(false, appName);
                Console.WriteLine("Mutex has been acquired");
                mutex.ReleaseMutex(); //This line is causing exception application exception why?
            }
        }

        private static void CompositeMutex()
        {
            var ba = new BankAccount();
            var ba2 = new BankAccount();
            var tasks = new List<Task>();
            var mutex = new Mutex();
            var mutex2 = new Mutex();

            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = mutex.WaitOne();
                    try
                    {
                        ba.Deposit(1);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = mutex2.WaitOne();
                    try
                    {
                        ba2.Deposit(1);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            mutex2.ReleaseMutex();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = WaitHandle.WaitAll(new[] { mutex, mutex2 });
                    try
                    {
                        ba.Transfer(ba2, 1);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            mutex.ReleaseMutex();
                            mutex2.ReleaseMutex();
                        }
                    }

                }));
            };
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"The final balance is {ba.Balance}");
            Console.WriteLine($"The final balance is {ba2.Balance}");
        }

        private static void MutexDemo()

        {
            var ba = new BankAccount();
            var tasks = new List<Task>();
            var mutex = new Mutex();

            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = mutex.WaitOne();
                    try
                    {
                        ba.Deposit(1000);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = mutex.WaitOne();
                    try
                    {
                        ba.Withdraw(1000);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"The final balance is {ba.Balance}");

        }

        private static void LockRecursion(int v)
        {
            var lockTaken = false;
            try
            {
                _sl.Enter(ref lockTaken);
            }
            catch (LockRecursionException le)
            {
                Console.WriteLine(le);
            }
            finally
            {
                if (lockTaken)
                {
                    Console.WriteLine($"Lock taken for value  {v}");
                    LockRecursion(v - 1);
                }
                else
                {
                    Console.WriteLine($"Unable to obtain a lock for value {v}");
                }
            }
        }

        private static void SpinLock()
        {
            var ba = new BankAccount();
            var tasks = new List<Task>();
            var sl = new SpinLock();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = false;
                    try
                    {
                        sl.Enter(ref lockTaken);
                        ba.Deposit(1000);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            sl.Exit();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bool lockTaken = false;
                    try
                    {
                        sl.Enter(ref lockTaken);
                        ba.Withdraw(1000);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            sl.Exit();
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"The final balance is {ba.Balance}");
        }

        private static void SimpleLockAndInterLock()
        {
            var ba = new BankAccount();
            var tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => ba.Deposit(1000)));
                tasks.Add(Task.Factory.StartNew(() => ba.Withdraw(1000)));
            }
            //Task.WaitAll();// does not work,you have to explicitly tell WaitAll for the tasks you need to wait for
            //Task.WaitAll(tasks.ToArray());
            Task.WhenAll(tasks).ContinueWith(t => Console.WriteLine($"The  balance in the account is {ba.Balance}"));
            //Console.WriteLine($"The final balance is {ba.Balance}");
        }
    }

    public class BankAccount
    {
        public int Balance { get => _balance; private set => _balance = value; }
        private object _padlock = new object();
        private int _balance;

        public void Deposit(int amount)
        {
            //lock (_padlock)
            //{
            //    Balance += amount;
            //}
            //Interlocked.Add(ref _balance, amount);
            Balance += amount;

        }

        public void Withdraw(int amount)
        {
            //lock (_padlock)
            //{
            //    Balance -= amount;
            //}
            //Interlocked.Add(ref _balance, -amount);
            Balance -= amount;

        }

        internal void Transfer(BankAccount where, int amount)
        {
            where.Balance += amount;
            Balance -= amount;
        }
    }
}
