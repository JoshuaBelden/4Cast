using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _4cast
{
    class Program
    {
        static void Main(string[] args)
        {
            var loans = new List<Loan>
            {
                new Loan(
                    name: "Loan 1",
                    anualRate: 0.1865m,
                    balance: 9_700.00m,
                    defaultMonthlyPayment: 450.00m),

                new Loan(
                    name: "Loan 2",
                    anualRate: 0.0659m,
                    balance: 16_726.00m,
                    defaultMonthlyPayment: 279.00m),

                new Loan(
                    name: "Loan 3",
                    anualRate: 0.0399m,
                    balance: 18_174.69m,
                    defaultMonthlyPayment: 400.00m)
            };

            OutputCSV(
                loanSchedules: new LoanScheduleCalculator().CalculateLoanSchedules(loans),
                filename: "output.csv");
        }

        static void OutputCSV(LoanSchedules loanSchedules, string filename)
        {
            var builder = new StringBuilder();

            builder.Append("Term");
            foreach (var loan in loanSchedules.Loans)
                builder.Append($",{loan.Name},,");
            builder.Append(Environment.NewLine);

            foreach (var termSchedules in loanSchedules.TermSchedules)
            {
                builder.Append($"Term: {termSchedules.Term}");

                foreach (var scheduleItem in termSchedules.ScheduleItems)
                    builder.Append($",{scheduleItem.Payment.Payment:#.00},{scheduleItem.Payment.InterestAmount:#.00},{scheduleItem.Balance:#.00}");

                builder.Append(Environment.NewLine);
            }

            File.WriteAllText(filename, builder.ToString());
        }
    }

    class LoanScheduleCalculator
    {
        public LoanSchedules CalculateLoanSchedules(IEnumerable<Loan> loans)
        {
            var termSchedules = new List<TermSchedule>();

            var rolloverBalance = new RolloverBalance(monthlyBalance: 0.00m);

            var term = 1;
            while (loans.Any(l => l.Balance > 0))
            {
                termSchedules.Add(new TermSchedule
                {
                    Term = term,
                    ScheduleItems = loans.Select(l =>
                    {
                        var payment = new PaymentResult();

                        if (l.Balance > 0)
                            payment = l.MakePayment(rolloverBalance.ApplyRollover(l.DefaultMonthlyPayment));

                        return new ScheduleItem
                        {
                            Term = term,
                            Loan = l,
                            Payment = payment,
                            Balance = l.Balance
                        };
                    })
                });

                rolloverBalance.AddMonthlyBalance(loans.Where(l => l.LoanState == LoanState.Paid).Sum(l => l.Rollover()));
                term++;
            }

            return new LoanSchedules
            {
                Loans = loans,
                TermSchedules = termSchedules
            };
        }
    }

    class RolloverBalance
    {
        decimal MonthlyBalance { get; set; }
        decimal AvailableBalance { get; set; }

        public RolloverBalance(decimal monthlyBalance)
        {
            MonthlyBalance =
                AvailableBalance = monthlyBalance;
        }

        public void AddMonthlyBalance(decimal amount)
        {
            MonthlyBalance =
                AvailableBalance = amount;
        }

        public decimal ApplyRollover(decimal amount)
        {
            var amountToApply = AvailableBalance;
            AvailableBalance = 0;

            return amount + amountToApply;
        }

        public void ResetRollover()
        {
            AvailableBalance = MonthlyBalance;
        }
    }

    class LoanSchedules
    {
        public IEnumerable<Loan> Loans { get; set; }

        public IEnumerable<TermSchedule> TermSchedules { get; set; }
    }

    class TermSchedule
    {
        public int Term { get; set; }
        public IEnumerable<ScheduleItem> ScheduleItems { get; set; }
    }

    class Loan
    {
        public string Name { get; set; }
        public decimal AnualRate { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal DefaultMonthlyPayment { get; set; }
        public LoanState LoanState { get; set; } = LoanState.Active;

        public Loan(string name, decimal anualRate, decimal balance, decimal defaultMonthlyPayment)
        {
            Name = name;
            AnualRate = anualRate;
            StartingBalance =
                Balance = balance;
            DefaultMonthlyPayment = defaultMonthlyPayment;
        }

        public PaymentResult MakePayment(decimal amount)
        {
            var interestAmount = Balance * AnualRate / 12;
            var principalAmount = amount - interestAmount;

            if (Balance - principalAmount > 0)
                Balance -= principalAmount;
            else
            {
                Balance = 0;
                principalAmount = Balance;
                LoanState = LoanState.Paid;
            }

            return new PaymentResult
            {
                Payment = amount,
                InterestAmount = interestAmount,
                PrincipalAmount = principalAmount
            };
        }

        public decimal Rollover()
        {
            if (LoanState != LoanState.Paid)
                return 0;

            LoanState = LoanState.Archived;
            return DefaultMonthlyPayment;
        }
    }

    enum LoanState
    {
        Active,
        Paid,
        Archived
    }

    class PaymentResult
    {
        public decimal Payment { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
    }

    class ScheduleItem
    {
        public int Term { get; set; }
        public Loan Loan { get; set; }
        public PaymentResult Payment { get; set; }
        public decimal Balance { get; set; }
    }
}