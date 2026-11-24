using Microsoft.EntityFrameworkCore;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Tests;

public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static User AddUser(AppDbContext db, string name)
    {
        var user = new User
        {
            Name = name,
            Email = name.ToLower() + "@test.com",
            PasswordHash = "hash",
        };

        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }
}
