using Discord;

namespace TD.Services.Reactions.Models
{
    public class AttackingShogunModel
    {
        public IUser? Shogun { get; set; }
        public int DamageDone { get; set; }
        public string DamageType { get; set; } = "";
        public int TotalDamage { get; set; }
    }
}
