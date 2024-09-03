using TD.Domain.Entities.Abstract;
using System.Collections.Generic;

namespace TD.Domain.Entities
{
    public class RoleName : Entity<int>
    {
        public string Role { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
