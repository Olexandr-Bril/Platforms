using Microsoft.AspNetCore.Mvc;
using SplitBill.Api.Services;
using SplitBill.Business.Dtos;
using SplitBill.Business.Interfaces;

namespace SplitBill.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtService _jwtService;

    public AuthController(IAuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto)
    {
        UserDto user = await _authService.RegisterAsync(dto);
        return Ok(new AuthResultDto(_jwtService.CreateToken(user), user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto)
    {
        UserDto user = await _authService.LoginAsync(dto);
        return Ok(new AuthResultDto(_jwtService.CreateToken(user), user));
    }
}
