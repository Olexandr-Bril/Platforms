using Microsoft.EntityFrameworkCore;
using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Helpers;
using SplitBill.Business.Interfaces;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Business.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ExpenseDto> AddAsync(int userId, int groupId, CreateExpenseDto dto)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);
        List<int> memberIds = group.Members.Select(m => m.UserId).ToList();

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new BadRequestException("Description is required.");
        }

        if (dto.Description.Trim().Length > 200)
        {
            throw new BadRequestException("Description is too long (max 200).");
        }

        if (dto.Amount <= 0)
        {
            throw new BadRequestException("Amount must be greater than zero.");
        }

        if (!DebtCalculator.IsValidMoney(dto.Amount))
        {
            throw new BadRequestException("Amount can have only 2 digits after the point.");
        }

        if (!memberIds.Contains(dto.PaidById))
        {
            throw new BadRequestException("Payer must be a member of the group.");
        }

        var expense = new Expense
        {
            GroupId = groupId,
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            PaidById = dto.PaidById,
            CreatedAt = DateTime.UtcNow,
        };

        if (dto.SplitType == SplitType.Equal)
        {
            List<int> users = dto.UserIds != null && dto.UserIds.Count > 0
                ? dto.UserIds.Distinct().ToList()
                : memberIds;

            if (users.Any(id => !memberIds.Contains(id)))
            {
                throw new BadRequestException("All users must be members of the group.");
            }

            List<decimal> parts = DebtCalculator.SplitEqually(dto.Amount, users.Count);

            for (int i = 0; i < users.Count; i++)
            {
                expense.Shares.Add(new ExpenseShare { UserId = users[i], Amount = parts[i] });
            }
        }
        else
        {
            if (dto.Shares == null || dto.Shares.Count == 0)
            {
                throw new BadRequestException("Shares are required for Exact split.");
            }

            if (dto.Shares.Select(s => s.UserId).Distinct().Count() != dto.Shares.Count)
            {
                throw new BadRequestException("Each user can be only once in shares.");
            }

            if (dto.Shares.Any(s => !memberIds.Contains(s.UserId)))
            {
                throw new BadRequestException("All users must be members of the group.");
            }

            if (dto.Shares.Any(s => s.Amount < 0 || !DebtCalculator.IsValidMoney(s.Amount)))
            {
                throw new BadRequestException("Share amounts are not valid.");
            }

            if (dto.Shares.Sum(s => s.Amount) != dto.Amount)
            {
                throw new BadRequestException("Sum of shares must be equal to the amount.");
            }

            foreach (ShareInputDto share in dto.Shares)
            {
                expense.Shares.Add(new ExpenseShare { UserId = share.UserId, Amount = share.Amount });
            }
        }

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return ToDto(expense, GroupHelper.GetNames(group));
    }

    public async Task<List<ExpenseDto>> GetAllAsync(int userId, int groupId)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);

        List<Expense> expenses = await _db.Expenses
            .Include(e => e.Shares)
            .Where(e => e.GroupId == groupId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        Dictionary<int, string> names = GroupHelper.GetNames(group);
        return expenses.Select(e => ToDto(e, names)).ToList();
    }

    private static ExpenseDto ToDto(Expense expense, Dictionary<int, string> names)
    {
        List<ShareDto> shares = expense.Shares
            .Select(s => new ShareDto(s.UserId, names.GetValueOrDefault(s.UserId, "?"), s.Amount))
            .ToList();

        return new ExpenseDto(
            expense.Id,
            expense.Description,
            expense.Amount,
            expense.PaidById,
            names.GetValueOrDefault(expense.PaidById, "?"),
            expense.CreatedAt,
            shares);
    }
}
