using TD.Domain.Entities.Abstract;

namespace TD.Domain.Entities
{
    public class RegisteredUser : Entity<int>
    {
        public ulong UUId { get; set; }
        public double TimeZone { get; set; }
    }
}
