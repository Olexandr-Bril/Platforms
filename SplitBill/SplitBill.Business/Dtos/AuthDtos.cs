namespace SplitBill.Business.Dtos;

public record RegisterDto(string Name, string Email, string Password);

public record LoginDto(string Email, string Password);

public record UserDto(int Id, string Name, string Email);

public record AuthResultDto(string Token, UserDto User);
