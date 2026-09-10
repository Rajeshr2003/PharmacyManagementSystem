using Pharmacy.AuthService.DTOs;

namespace Pharmacy.AuthService.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);

    Task<string> LoginAsync(LoginDto dto);

    Task<string> ForgotPasswordAsync(ForgotPasswordDto dto);

    Task<string> VerifyOtpAsync(VerifyOtpDto dto);

    Task<string> ResetPasswordAsync(ResetPasswordDto dto);
}