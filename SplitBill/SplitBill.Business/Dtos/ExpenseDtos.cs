namespace SplitBill.Business.Dtos;

public enum SplitType
{
    Equal,
    Exact,
}

public record ShareInputDto(int UserId, decimal Amount);

public record CreateExpenseDto(
    string Description,
    decimal Amount,
    int PaidById,
    SplitType SplitType,
    List<int>? UserIds,
    List<ShareInputDto>? Shares);

public record ShareDto(int UserId, string UserName, decimal Amount);

public record ExpenseDto(
    int Id,
    string Description,
    decimal Amount,
    int PaidById,
    string PaidByName,
    DateTime CreatedAt,
    List<ShareDto> Shares);
