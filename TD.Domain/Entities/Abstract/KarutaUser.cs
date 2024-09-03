namespace TD.Domain.Entities.Abstract
{
    public abstract class KarutaUser : Entity<int>
    {
        public ulong UUId { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
    }
}
