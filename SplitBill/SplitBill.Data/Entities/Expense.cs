namespace SplitBill.Data.Entities;

public class Expense
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int PaidById { get; set; }

    public User PaidBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public List<ExpenseShare> Shares { get; set; } = new();
}
