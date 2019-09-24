using System;
using System.Collections.Generic;
using System.Text;

namespace ParProgPractise
{
    public class BankAccount
    {
        public int Balance { get; private set; }

        public void Deposit(int amount)
        {
            Balance += amount;
        }

        public void Withdraw(int amount)
        {
            Balance -= amount;
        }

        public void Transfer(BankAccount who, int amount)
        {
            Balance -= amount;
            who.Balance += amount;
        }

    }
}
