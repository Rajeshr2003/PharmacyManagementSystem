using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pharmacy.AuthService.Data;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        optionsBuilder.UseSqlServer(
    "Server=.;Database=UsersDB;User Id=sa;Password=Mandal@123;TrustServerCertificate=True;");
        return new UsersDbContext(optionsBuilder.Options);
    }
}
