using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Pharmacy.AuthService.Data;
using Pharmacy.AuthService.DTOs;
using Pharmacy.AuthService.Entities;
using Pharmacy.AuthService.Interfaces;

namespace Pharmacy.AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IEmailService _emailService;
    private readonly UsersDbContext _context;

    private readonly JwtService _jwtService;
    public AuthService(
    UsersDbContext context,
    JwtService jwtService,
    IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
        {
            return "User already exists";
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == dto.Role);

        if (role == null)
        {
            return "Role not found";
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return "User Registered Successfully";
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            return "Invalid Credentials";
        }

        bool validPassword = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash);

        if (!validPassword)
        {
            return "Invalid Credentials";
        }

        // return $"Login Successful. Role: {user.Role.Name}";
        var token = _jwtService.GenerateToken(user);

        return token;
    }

    public async Task<string> ForgotPasswordAsync(
    ForgotPasswordDto dto)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Email == dto.Email);

    if (user == null)
    {
        return "User not found";
    }

    var otp = new Random()
        .Next(100000, 999999)
        .ToString();

    user.ResetOtp = otp;
    user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
    user.IsOtpVerified = false;

    await _context.SaveChangesAsync();

    await _emailService.SendEmailAsync(
        user.Email,
        "Password Reset OTP",
        $"Your OTP is: {otp}");

    return "OTP sent successfully";
}

public async Task<string> VerifyOtpAsync(
    VerifyOtpDto dto)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Email == dto.Email);

    if (user == null)
    {
        return "User not found";
    }

    if (user.ResetOtp != dto.Otp)
    {
        return "Invalid OTP";
    }

    if (user.OtpExpiry < DateTime.UtcNow)
    {
        return "OTP Expired";
    }

    user.IsOtpVerified = true;

    await _context.SaveChangesAsync();

    return "OTP Verified Successfully";
}
public async Task<string> ResetPasswordAsync(
    ResetPasswordDto dto)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Email == dto.Email);

    if (user == null)
    {
        return "User not found";
    }

    if (!user.IsOtpVerified)
    {
        return "OTP verification required";
    }

    user.PasswordHash =
        BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

    user.ResetOtp = null;
    user.OtpExpiry = null;
    user.IsOtpVerified = false;

    await _context.SaveChangesAsync();

    return "Password Reset Successful";
}
}



// using BCrypt.Net;
// using Microsoft.EntityFrameworkCore;
// using Pharmacy.AuthService.Data;
// using Pharmacy.AuthService.DTOs;
// using Pharmacy.AuthService.Entities;
// using Pharmacy.AuthService.Interfaces;

// namespace Pharmacy.AuthService.Services;

// public class AuthService : IAuthService
// {
//     private readonly UsersDbContext _context;

//     public AuthService(UsersDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<string> RegisterAsync(RegisterDto dto)
//     {
//         var existingUser = await _context.Users
//             .FirstOrDefaultAsync(u => u.Email == dto.Email);

//         if (existingUser != null)
//         {
//             return "User already exists";
//         }

//         var role = await _context.Roles
//             .FirstOrDefaultAsync(r => r.Name == dto.Role);

//         if (role == null)
//         {
//             return "Role not found";
//         }

//         var user = new User
//         {
//             Name = dto.Name,
//             Email = dto.Email,
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//             RoleId = role.Id,
//             CreatedAt = DateTime.Now
//         };

//         _context.Users.Add(user);
//         await _context.SaveChangesAsync();

//         return "User Registered Successfully";
//     }

//     public async Task<string> LoginAsync(LoginDto dto)
//     {
//         var user = await _context.Users
//             .Include(u => u.Role)
//             .FirstOrDefaultAsync(u =>
//                 u.Email == dto.Email &&
//                 u.PasswordHash == dto.Password);

//         if (user == null)
//         {
//             return "Invalid Credentials";
//         }

//         return $"Login Successful. Role: {user.Role.Name}";
//     }
// }





// // using Microsoft.EntityFrameworkCore;
// // using Pharmacy.AuthService.Data;
// // using Pharmacy.AuthService.DTOs;
// // using Pharmacy.AuthService.Entities;
// // using Pharmacy.AuthService.Interfaces;

// // namespace Pharmacy.AuthService.Services;

// // public class AuthService : IAuthService
// // {
// //     private readonly UsersDbContext _context;

// //     public AuthService(UsersDbContext context)
// //     {
// //         _context = context;
// //     }

// //     public async Task<string> RegisterAsync(RegisterDto dto)
// //     {
// //         var existingUser = await _context.Users
// //             .FirstOrDefaultAsync(u => u.Email == dto.Email);

// //         if (existingUser != null)
// //         {
// //             return "User already exists";
// //         }

// //         var user = new User
// //         {
// //             Name = dto.Name,
// //             Email = dto.Email,
// //             PasswordHash = dto.Password,
// //             CreatedAt = DateTime.Now
// //         };

// //         _context.Users.Add(user);
// //         await _context.SaveChangesAsync();

// //         return "User Registered Successfully";
// //     }

// //     public async Task<string> LoginAsync(LoginDto dto)
// //     {
// //         var user = await _context.Users
// //             .FirstOrDefaultAsync(u =>
// //                 u.Email == dto.Email &&
// //                 u.PasswordHash == dto.Password);

// //         if (user == null)
// //         {
// //             return "Invalid Credentials";
// //         }

// //         return "Login Successful";
// //     }
// // }













// // // using Pharmacy.AuthService.Data;
// // // using Pharmacy.AuthService.DTOs;
// // // using Pharmacy.AuthService.Interfaces;

// // // namespace Pharmacy.AuthService.Services;

// // // public class AuthService : IAuthService
// // // {
// // //     private readonly UsersDbContext _context;

// // //     public AuthService(UsersDbContext context)
// // //     {
// // //         _context = context;
// // //     }

// // //     public async Task<string> RegisterAsync(RegisterDto dto)
// // //     {
// // //         throw new NotImplementedException();
// // //     }

// // //     public async Task<string> LoginAsync(LoginDto dto)
// // //     {
// // //         throw new NotImplementedException();
// // //     }
// // // }