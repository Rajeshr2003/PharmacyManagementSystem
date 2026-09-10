using Pharmacy.AuthService.Entities;

namespace Pharmacy.AuthService.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task UpdateAsync(User user);
}