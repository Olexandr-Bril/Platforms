using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SplitBill.Business.Dtos;
using SplitBill.Business.Exceptions;
using SplitBill.Business.Interfaces;
using SplitBill.Data;
using SplitBill.Data.Entities;

namespace SplitBill.Business.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
        {
            throw new BadRequestException("Email is not valid.");
        }

        if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
        {
            throw new BadRequestException("Password must be at least 6 characters.");
        }

        string email = dto.Email.Trim().ToLower();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            throw new BadRequestException("User with this email already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserDto(user.Id, user.Name, user.Email);
    }

    public async Task<UserDto> LoginAsync(LoginDto dto)
    {
        string email = (dto.Email ?? string.Empty).Trim().ToLower();
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password ?? string.Empty) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Wrong email or password.");
        }

        return new UserDto(user.Id, user.Name, user.Email);
    }
}
