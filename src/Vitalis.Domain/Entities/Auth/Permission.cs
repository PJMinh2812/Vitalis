namespace Vitalis.Domain.Entities.Auth;

public class Permission : BaseEntity
{
    public string Name { get; set; } = null!;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
