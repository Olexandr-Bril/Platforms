namespace SplitBill.Data.Entities;

// частка витрати, яку має заплатити конкретна людина
public class ExpenseShare
{
    public int Id { get; set; }

    public int ExpenseId { get; set; }

    public Expense Expense { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
}
