namespace TD.Services.Reactions.Models
{
    public class NodeOwnerModel
    {
        public int NodeNumber { get; set; }
        public string NodeName { get; set; } = "";
        public ulong OwnerId { get; set; }
    }
}
