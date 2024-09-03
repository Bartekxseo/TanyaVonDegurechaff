namespace TD.Services.Embeds.Models
{
    public class KarutaNodeEmbed
    {
        public int ClanSize { get; set; }
        public string NodeName { get; set; } = "";
        public int CurrentPower { get; set; }
        public int EstimatedTimeSinceCapture { get; set; }
        public ulong HolderId { get; set; }

        public int? TimeToCapture { get; set; }
        public TimeType TimeType { get; set; } = TimeType.Minutes;

    }

    public enum TimeType
    {
        Minutes,
        Seconds
    }
}
