using SplitBill.Business.Helpers;
using SplitBill.Data.Entities;

namespace SplitBill.Tests;

public class DebtCalculatorTests
{
    [Fact]
    public void SplitEqually_SumIsSameAsAmount()
    {
        List<decimal> parts = DebtCalculator.SplitEqually(100m, 3);

        Assert.Equal(3, parts.Count);
        Assert.Equal(100m, parts.Sum());
        Assert.Equal(33.34m, parts[0]);
        Assert.Equal(33.33m, parts[1]);
        Assert.Equal(33.33m, parts[2]);
    }

    [Fact]
    public void SplitEqually_ZeroCount_Throws()
    {
        Assert.Throws<ArgumentException>(() => DebtCalculator.SplitEqually(10m, 0));
    }

    [Fact]
    public void GetBalances_PayerGetsPlus_OthersGetMinus()
    {
        var expense = new Expense
        {
            Amount = 90m,
            PaidById = 1,
            Shares =
            {
                new ExpenseShare { UserId = 1, Amount = 30m },
                new ExpenseShare { UserId = 2, Amount = 30m },
                new ExpenseShare { UserId = 3, Amount = 30m },
            },
        };

        Dictionary<int, decimal> balances = DebtCalculator.GetBalances(new[] { expense }, new List<Payment>());

        Assert.Equal(60m, balances[1]);
        Assert.Equal(-30m, balances[2]);
        Assert.Equal(-30m, balances[3]);
    }

    [Fact]
    public void GetBalances_PaymentReducesDebt()
    {
        var expense = new Expense
        {
            Amount = 100m,
            PaidById = 1,
            Shares =
            {
                new ExpenseShare { UserId = 1, Amount = 50m },
                new ExpenseShare { UserId = 2, Amount = 50m },
            },
        };
        var payment = new Payment { FromUserId = 2, ToUserId = 1, Amount = 50m };

        Dictionary<int, decimal> balances = DebtCalculator.GetBalances(new[] { expense }, new[] { payment });

        Assert.Equal(0m, balances[1]);
        Assert.Equal(0m, balances[2]);
    }

    [Fact]
    public void SimplifyDebts_ReturnsWhoPaysWhom()
    {
        var balances = new Dictionary<int, decimal>
        {
            [1] = 60m,
            [2] = -30m,
            [3] = -30m,
        };

        List<Debt> debts = DebtCalculator.SimplifyDebts(balances);

        Assert.Equal(2, debts.Count);
        Assert.All(debts, d => Assert.Equal(1, d.ToUserId));
        Assert.Equal(60m, debts.Sum(d => d.Amount));
    }

    [Fact]
    public void SimplifyDebts_AllZero_NoDebts()
    {
        var balances = new Dictionary<int, decimal> { [1] = 0m, [2] = 0m };

        Assert.Empty(DebtCalculator.SimplifyDebts(balances));
    }

    [Theory]
    [InlineData(10.5, true)]
    [InlineData(10.55, true)]
    [InlineData(10.555, false)]
    public void IsValidMoney_ChecksDigitsAfterPoint(double value, bool expected)
    {
        Assert.Equal(expected, DebtCalculator.IsValidMoney((decimal)value));
    }
}
