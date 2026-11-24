using Microsoft.EntityFrameworkCore;
using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Interfaces;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Business.Services;

public class GroupService : IGroupService
{
    private const int MaxMembers = 50;

    private readonly AppDbContext _db;

    public GroupService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GroupDto> CreateAsync(int userId, CreateGroupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BadRequestException("Group name is required.");
        }

        if (dto.Name.Trim().Length > 100)
        {
            throw new BadRequestException("Group name is too long (max 100).");
        }

        var group = new Group
        {
            Name = dto.Name.Trim(),
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
        };
        group.Members.Add(new GroupMember { UserId = userId });

        if (dto.MemberEmails != null)
        {
            List<string> emails = dto.MemberEmails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .ToList();

            if (emails.Count + 1 > MaxMembers)
            {
                throw new BadRequestException($"Group can't have more than {MaxMembers} members.");
            }

            foreach (string email in emails)
            {
                User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    throw new BadRequestException($"User with email {email} not found.");
                }

                if (user.Id != userId)
                {
                    group.Members.Add(new GroupMember { UserId = user.Id });
                }
            }
        }

        _db.Groups.Add(group);
        await _db.SaveChangesAsync();

        return await GetAsync(userId, group.Id);
    }

    public async Task<List<GroupShortDto>> GetUserGroupsAsync(int userId)
    {
        return await _db.Groups
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderBy(g => g.Id)
            .Select(g => new GroupShortDto(g.Id, g.Name, g.Members.Count))
            .ToListAsync();
    }

    public async Task<GroupDto> GetAsync(int userId, int groupId)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);
        return ToDto(group);
    }

    public async Task<GroupDto> AddMemberAsync(int userId, int groupId, AddMemberDto dto)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BadRequestException("Email is required.");
        }

        string email = dto.Email.Trim().ToLower();
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new BadRequestException($"User with email {email} not found.");
        }

        if (group.Members.Any(m => m.UserId == user.Id))
        {
            throw new BadRequestException("User is already in this group.");
        }

        if (group.Members.Count >= MaxMembers)
        {
            throw new BadRequestException($"Group can't have more than {MaxMembers} members.");
        }

        group.Members.Add(new GroupMember { UserId = user.Id, User = user });
        await _db.SaveChangesAsync();

        return ToDto(group);
    }

    private static GroupDto ToDto(Group group)
    {
        List<UserDto> members = group.Members
            .Select(m => new UserDto(m.UserId, m.User.Name, m.User.Email))
            .OrderBy(m => m.Name)
            .ToList();

        return new GroupDto(group.Id, group.Name, group.OwnerId, group.CreatedAt, members);
    }
}
