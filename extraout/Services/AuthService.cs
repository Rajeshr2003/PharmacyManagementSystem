using Microsoft.EntityFrameworkCore;
using Pharmacy.AuthService.Data;
using Pharmacy.AuthService.DTOs;
using Pharmacy.AuthService.Entities;
using Pharmacy.AuthService.Interfaces;

namespace Pharmacy.AuthService.Services;

public class AuthService : IAuthService
{
    private readonly UsersDbContext _context;

    public AuthService(UsersDbContext context)
    {
        _context = context;
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
            PasswordHash = dto.Password,
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
            .FirstOrDefaultAsync(u =>
                u.Email == dto.Email &&
                u.PasswordHash == dto.Password);

        if (user == null)
        {
            return "Invalid Credentials";
        }

        return $"Login Successful. Role: {user.Role.Name}";
    }
}





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

//         var user = new User
//         {
//             Name = dto.Name,
//             Email = dto.Email,
//             PasswordHash = dto.Password,
//             CreatedAt = DateTime.Now
//         };

//         _context.Users.Add(user);
//         await _context.SaveChangesAsync();

//         return "User Registered Successfully";
//     }

//     public async Task<string> LoginAsync(LoginDto dto)
//     {
//         var user = await _context.Users
//             .FirstOrDefaultAsync(u =>
//                 u.Email == dto.Email &&
//                 u.PasswordHash == dto.Password);

//         if (user == null)
//         {
//             return "Invalid Credentials";
//         }

//         return "Login Successful";
//     }
// }













// // using Pharmacy.AuthService.Data;
// // using Pharmacy.AuthService.DTOs;
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
// //         throw new NotImplementedException();
// //     }

// //     public async Task<string> LoginAsync(LoginDto dto)
// //     {
// //         throw new NotImplementedException();
// //     }
// // }