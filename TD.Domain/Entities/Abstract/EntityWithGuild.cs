using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD.Domain.Entities.Abstract
{
    public abstract class EntityWithGuild<TKey> : Entity<TKey>
    {
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
    }
}
