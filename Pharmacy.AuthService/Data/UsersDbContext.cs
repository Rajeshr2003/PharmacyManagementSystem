using Microsoft.EntityFrameworkCore;
using Pharmacy.AuthService.Entities;

namespace Pharmacy.AuthService.Data;
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}