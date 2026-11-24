using Microsoft.EntityFrameworkCore;
using SplitBill.Data.Entities;

namespace SplitBill.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<ExpenseShare> ExpenseShares => Set<ExpenseShare>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Group>(e =>
        {
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasKey(m => new { m.GroupId, m.UserId });
            e.HasOne(m => m.Group).WithMany(g => g.Members).HasForeignKey(m => m.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany(u => u.Groups).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Description).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Group).WithMany(g => g.Expenses).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PaidBy).WithMany().HasForeignKey(x => x.PaidById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpenseShare>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Expense).WithMany(x => x.Shares).HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Group).WithMany(g => g.Payments).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.FromUser).WithMany().HasForeignKey(x => x.FromUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToUser).WithMany().HasForeignKey(x => x.ToUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
