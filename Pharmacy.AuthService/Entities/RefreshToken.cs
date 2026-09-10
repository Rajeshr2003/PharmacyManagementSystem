namespace Pharmacy.AuthService.Entities;
public class RefreshToken
{
    public required int Id { get; set; }
    public int UserId { get; set; }
    public required string TokenHash { get; set; }
    public required DateTime IssuedAt { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public User? User { get; set; }
}