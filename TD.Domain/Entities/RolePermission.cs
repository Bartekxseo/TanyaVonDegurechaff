using TD.Domain.Entities.Abstract;
using TD.Domain.Enums;
using System.Collections.Generic;

namespace TD.Domain.Entities
{
    public class RolePermission : Entity<int>
    {
        public ulong GuildId { get; set; }
        public virtual ICollection<RoleName> RoleNames { get; set; } = new List<RoleName>();
        public RoleType RoleType { get; set; }
    }
}
