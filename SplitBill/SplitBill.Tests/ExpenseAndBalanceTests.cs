using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Services;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Tests;

public class ExpenseAndBalanceTests
{
    private readonly AppDbContext _db;
    private readonly User _ivan;
    private readonly User _olena;
    private readonly int _groupId;

    public ExpenseAndBalanceTests()
    {
        _db = TestDb.Create();
        _ivan = TestDb.AddUser(_db, "Ivan");
        _olena = TestDb.AddUser(_db, "Olena");

        var groupService = new GroupService(_db);
        _groupId = groupService.CreateAsync(_ivan.Id, new CreateGroupDto("Trip", new List<string> { "olena@test.com" }))
            .GetAwaiter().GetResult().Id;
    }

    [Fact]
    public async Task AddExpense_Equal_SplitsBetweenAll()
    {
        var service = new ExpenseService(_db);

        ExpenseDto expense = await service.AddAsync(_ivan.Id, _groupId,
            new CreateExpenseDto("Pizza", 300m, _ivan.Id, SplitType.Equal, null, null));

        Assert.Equal(2, expense.Shares.Count);
        Assert.All(expense.Shares, s => Assert.Equal(150m, s.Amount));
    }

    [Fact]
    public async Task AddExpense_ExactWrongSum_Throws()
    {
        var service = new ExpenseService(_db);
        var shares = new List<ShareInputDto>
        {
            new(_ivan.Id, 50m),
            new(_olena.Id, 60m),
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.AddAsync(_ivan.Id, _groupId,
            new CreateExpenseDto("Taxi", 100m, _ivan.Id, SplitType.Exact, null, shares)));
    }

    [Fact]
    public async Task AddExpense_NegativeAmount_Throws()
    {
        var service = new ExpenseService(_db);

        await Assert.ThrowsAsync<BadRequestException>(() => service.AddAsync(_ivan.Id, _groupId,
            new CreateExpenseDto("Pizza", -5m, _ivan.Id, SplitType.Equal, null, null)));
    }

    [Fact]
    public async Task Balances_AfterExpense_OlenaOwesIvan()
    {
        var expenseService = new ExpenseService(_db);
        var balanceService = new BalanceService(_db);
        await expenseService.AddAsync(_ivan.Id, _groupId,
            new CreateExpenseDto("Pizza", 300m, _ivan.Id, SplitType.Equal, null, null));

        GroupBalancesDto result = await balanceService.GetBalancesAsync(_ivan.Id, _groupId);

        DebtDto debt = Assert.Single(result.Debts);
        Assert.Equal(_olena.Id, debt.FromUserId);
        Assert.Equal(_ivan.Id, debt.ToUserId);
        Assert.Equal(150m, debt.Amount);
    }

    [Fact]
    public async Task Balances_AfterPayment_NoDebts()
    {
        var expenseService = new ExpenseService(_db);
        var balanceService = new BalanceService(_db);
        await expenseService.AddAsync(_ivan.Id, _groupId,
            new CreateExpenseDto("Pizza", 300m, _ivan.Id, SplitType.Equal, null, null));

        await balanceService.AddPaymentAsync(_olena.Id, _groupId, new CreatePaymentDto(_ivan.Id, 150m));
        GroupBalancesDto result = await balanceService.GetBalancesAsync(_ivan.Id, _groupId);

        Assert.Empty(result.Debts);
    }

    [Fact]
    public async Task Payment_ToYourself_Throws()
    {
        var balanceService = new BalanceService(_db);

        await Assert.ThrowsAsync<BadRequestException>(
            () => balanceService.AddPaymentAsync(_ivan.Id, _groupId, new CreatePaymentDto(_ivan.Id, 10m)));
    }
}
