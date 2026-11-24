using SplitBill.Data.Entities;

namespace SplitBill.Business.Helpers;

public record Debt(int FromUserId, int ToUserId, decimal Amount);

public static class DebtCalculator
{
    // ділимо суму в копійках, щоб в сумі вийшло рівно стільки ж
    public static List<decimal> SplitEqually(decimal amount, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be greater than zero.");
        }

        long cents = (long)Math.Round(amount * 100);
        long part = cents / count;
        long rest = cents % count;

        var result = new List<decimal>();

        for (int i = 0; i < count; i++)
        {
            long value = i < rest ? part + 1 : part;
            result.Add(value / 100m);
        }

        return result;
    }

    // плюс - користувачу винні, мінус - користувач винен
    public static Dictionary<int, decimal> GetBalances(IEnumerable<Expense> expenses, IEnumerable<Payment> payments)
    {
        var balances = new Dictionary<int, decimal>();

        foreach (Expense expense in expenses)
        {
            AddToBalance(balances, expense.PaidById, expense.Amount);

            foreach (ExpenseShare share in expense.Shares)
            {
                AddToBalance(balances, share.UserId, -share.Amount);
            }
        }

        foreach (Payment payment in payments)
        {
            AddToBalance(balances, payment.FromUserId, payment.Amount);
            AddToBalance(balances, payment.ToUserId, -payment.Amount);
        }

        return balances;
    }

    public static List<Debt> SimplifyDebts(Dictionary<int, decimal> balances)
    {
        List<BalanceItem> debtors = balances
            .Where(b => b.Value < 0)
            .OrderBy(b => b.Value)
            .Select(b => new BalanceItem { UserId = b.Key, Amount = -b.Value })
            .ToList();

        List<BalanceItem> creditors = balances
            .Where(b => b.Value > 0)
            .OrderByDescending(b => b.Value)
            .Select(b => new BalanceItem { UserId = b.Key, Amount = b.Value })
            .ToList();

        var debts = new List<Debt>();
        int i = 0;
        int j = 0;

        while (i < debtors.Count && j < creditors.Count)
        {
            decimal amount = Math.Min(debtors[i].Amount, creditors[j].Amount);
            debts.Add(new Debt(debtors[i].UserId, creditors[j].UserId, amount));

            debtors[i].Amount -= amount;
            creditors[j].Amount -= amount;

            if (debtors[i].Amount == 0)
            {
                i++;
            }

            if (creditors[j].Amount == 0)
            {
                j++;
            }
        }

        return debts;
    }

    public static bool IsValidMoney(decimal amount)
    {
        return decimal.Round(amount, 2) == amount;
    }

    private static void AddToBalance(Dictionary<int, decimal> balances, int userId, decimal amount)
    {
        if (!balances.ContainsKey(userId))
        {
            balances[userId] = 0;
        }

        balances[userId] += amount;
    }

    private class BalanceItem
    {
        public int UserId { get; set; }

        public decimal Amount { get; set; }
    }
}
