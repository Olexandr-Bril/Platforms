namespace SplitBill.Data.Entities;

public class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<GroupMember> Members { get; set; } = new();

    public List<Expense> Expenses { get; set; } = new();

    public List<Payment> Payments { get; set; } = new();
}
