using Microsoft.EntityFrameworkCore;
using SplitBill.Business.Exceptions;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Business.Services;

internal static class GroupHelper
{
    public static async Task<Group> GetGroupForUserAsync(AppDbContext db, int groupId, int userId)
    {
        Group? group = await db.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            throw new NotFoundException($"Group {groupId} not found.");
        }

        if (!group.Members.Any(m => m.UserId == userId))
        {
            throw new ForbiddenException("You are not a member of this group.");
        }

        return group;
    }

    public static Dictionary<int, string> GetNames(Group group)
    {
        return group.Members.ToDictionary(m => m.UserId, m => m.User.Name);
    }
}
