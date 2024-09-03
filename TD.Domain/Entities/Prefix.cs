using TD.Domain.Entities.Abstract;

namespace TD.Domain.Entities
{
    public class Prefix : Entity<int>
    {
        public string prefix { get; set; }

        public ulong GuildId { get; set; }
    }
}
