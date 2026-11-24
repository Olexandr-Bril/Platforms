namespace SplitBill.Data.Entities;

public class GroupMember
{
    public int GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;
}
