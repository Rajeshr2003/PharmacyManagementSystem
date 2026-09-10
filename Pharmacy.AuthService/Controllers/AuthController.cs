using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.AuthService.DTOs;
using Pharmacy.AuthService.Interfaces;
using System.Security.Claims;

namespace Pharmacy.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);

        return Ok(new
        {
            accessToken = token
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        Name = User.FindFirst(ClaimTypes.Name)?.Value,
        Email = User.FindFirst(ClaimTypes.Email)?.Value,
        Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    [HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(
    ForgotPasswordDto dto)
{
    var result =
        await _authService.ForgotPasswordAsync(dto);

    return Ok(result);
}

[HttpPost("verify-otp")]
public async Task<IActionResult> VerifyOtp(
    VerifyOtpDto dto)
{
    var result =
        await _authService.VerifyOtpAsync(dto);

    return Ok(result);
}

[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordDto dto)
{
    var result =
        await _authService.ResetPasswordAsync(dto);

    return Ok(result);
}

// [HttpGet("test-error")]
// public IActionResult TestError()
// {
//     throw new Exception("Middleware Working");
// }

}