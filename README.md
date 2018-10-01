# 4Cast

A multi-loan amortization schedule that supports rolling over payments from accounts that have a zero balance.

## Running

Edit `Program.cs` loans variable to include your loans:

```CSHARP
new Loan(
  name: "Loan 1",
  anualRate: 0.1865m,
  balance: 9_700.00m,
  defaultMonthlyPayment: 450.00m),
```
... then run the program. A csv file will be created in the folder the app is run from, called `outputcsv`.
Open the file in excel and watch the magic of your money paying off your debt.
