using Microsoft.EntityFrameworkCore;
using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Helpers;
using SplitBill.Business.Interfaces;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Business.Services;

public class BalanceService : IBalanceService
{
    private readonly AppDbContext _db;

    public BalanceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GroupBalancesDto> GetBalancesAsync(int userId, int groupId)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);

        List<Expense> expenses = await _db.Expenses
            .Include(e => e.Shares)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();

        List<Payment> payments = await _db.Payments
            .Where(p => p.GroupId == groupId)
            .ToListAsync();

        Dictionary<int, decimal> balances = DebtCalculator.GetBalances(expenses, payments);

        foreach (GroupMember member in group.Members)
        {
            if (!balances.ContainsKey(member.UserId))
            {
                balances[member.UserId] = 0;
            }
        }

        List<Debt> debts = DebtCalculator.SimplifyDebts(balances);
        Dictionary<int, string> names = GroupHelper.GetNames(group);

        List<BalanceDto> balanceDtos = balances
            .Select(b => new BalanceDto(b.Key, names.GetValueOrDefault(b.Key, "?"), b.Value))
            .OrderBy(b => b.UserName)
            .ToList();

        List<DebtDto> debtDtos = debts
            .Select(d => new DebtDto(
                d.FromUserId,
                names.GetValueOrDefault(d.FromUserId, "?"),
                d.ToUserId,
                names.GetValueOrDefault(d.ToUserId, "?"),
                d.Amount))
            .ToList();

        return new GroupBalancesDto(balanceDtos, debtDtos);
    }

    public async Task<PaymentDto> AddPaymentAsync(int userId, int groupId, CreatePaymentDto dto)
    {
        Group group = await GroupHelper.GetGroupForUserAsync(_db, groupId, userId);

        if (dto.ToUserId == userId)
        {
            throw new BadRequestException("You can't pay to yourself.");
        }

        if (!group.Members.Any(m => m.UserId == dto.ToUserId))
        {
            throw new BadRequestException("Receiver must be a member of the group.");
        }

        if (dto.Amount <= 0 || !DebtCalculator.IsValidMoney(dto.Amount))
        {
            throw new BadRequestException("Amount is not valid.");
        }

        var payment = new Payment
        {
            GroupId = groupId,
            FromUserId = userId,
            ToUserId = dto.ToUserId,
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return new PaymentDto(payment.Id, payment.FromUserId, payment.ToUserId, payment.Amount, payment.CreatedAt);
    }
}
