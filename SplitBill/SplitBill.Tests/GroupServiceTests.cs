using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Services;

namespace SplitBill.Tests;

public class GroupServiceTests
{
    [Fact]
    public async Task Create_AddsOwnerAndMembers()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        TestDb.AddUser(db, "Olena");
        var service = new GroupService(db);

        GroupDto group = await service.CreateAsync(ivan.Id, new CreateGroupDto("Trip", new List<string> { "olena@test.com" }));

        Assert.Equal("Trip", group.Name);
        Assert.Equal(2, group.Members.Count);
        Assert.Equal(ivan.Id, group.OwnerId);
    }

    [Fact]
    public async Task Create_EmptyName_Throws()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        var service = new GroupService(db);

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(ivan.Id, new CreateGroupDto("  ", null)));
    }

    [Fact]
    public async Task Create_UnknownEmail_Throws()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        var service = new GroupService(db);

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(ivan.Id, new CreateGroupDto("Trip", new List<string> { "nobody@test.com" })));
    }

    [Fact]
    public async Task Get_NotMember_ThrowsForbidden()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        var petro = TestDb.AddUser(db, "Petro");
        var service = new GroupService(db);
        GroupDto group = await service.CreateAsync(ivan.Id, new CreateGroupDto("Trip", null));

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetAsync(petro.Id, group.Id));
    }

    [Fact]
    public async Task Get_WrongId_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        var service = new GroupService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(ivan.Id, 999));
    }

    [Fact]
    public async Task AddMember_AlreadyInGroup_Throws()
    {
        using var db = TestDb.Create();
        var ivan = TestDb.AddUser(db, "Ivan");
        TestDb.AddUser(db, "Olena");
        var service = new GroupService(db);
        GroupDto group = await service.CreateAsync(ivan.Id, new CreateGroupDto("Trip", new List<string> { "olena@test.com" }));

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.AddMemberAsync(ivan.Id, group.Id, new AddMemberDto("olena@test.com")));
    }
}
