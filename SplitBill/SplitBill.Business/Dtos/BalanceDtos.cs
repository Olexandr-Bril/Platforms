namespace SplitBill.Business.Dtos;

public record BalanceDto(int UserId, string UserName, decimal Balance);

public record DebtDto(int FromUserId, string FromName, int ToUserId, string ToName, decimal Amount);

public record GroupBalancesDto(List<BalanceDto> Balances, List<DebtDto> Debts);

public record CreatePaymentDto(int ToUserId, decimal Amount);

public record PaymentDto(int Id, int FromUserId, int ToUserId, decimal Amount, DateTime CreatedAt);
