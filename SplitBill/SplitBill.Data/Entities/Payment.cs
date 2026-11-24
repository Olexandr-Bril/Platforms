namespace SplitBill.Data.Entities;

// повернення боргу від одного учасника іншому
public class Payment
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public int FromUserId { get; set; }

    public User FromUser { get; set; } = null!;

    public int ToUserId { get; set; }

    public User ToUser { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
