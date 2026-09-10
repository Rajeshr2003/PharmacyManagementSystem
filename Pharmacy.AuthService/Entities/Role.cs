using System.Reflection.Metadata;

namespace Pharmacy.AuthService.Entities;
public class Role
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();//A collections(list-like container) of User objects
}