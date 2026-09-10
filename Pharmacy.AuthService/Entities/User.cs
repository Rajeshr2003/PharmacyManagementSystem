namespace Pharmacy.AuthService.Entities;
public class User
{
    public int Id { get; set;}
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Role? Role { get; set; }
    public string? ResetOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public bool IsOtpVerified { get; set; } = false;

}