using SplitBill.Business.Dtos;

namespace SplitBill.Business.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);

    Task<UserDto> LoginAsync(LoginDto dto);
}

public interface IGroupService
{
    Task<GroupDto> CreateAsync(int userId, CreateGroupDto dto);

    Task<List<GroupShortDto>> GetUserGroupsAsync(int userId);

    Task<GroupDto> GetAsync(int userId, int groupId);

    Task<GroupDto> AddMemberAsync(int userId, int groupId, AddMemberDto dto);
}

public interface IExpenseService
{
    Task<ExpenseDto> AddAsync(int userId, int groupId, CreateExpenseDto dto);

    Task<List<ExpenseDto>> GetAllAsync(int userId, int groupId);
}

public interface IBalanceService
{
    Task<GroupBalancesDto> GetBalancesAsync(int userId, int groupId);

    Task<PaymentDto> AddPaymentAsync(int userId, int groupId, CreatePaymentDto dto);
}
