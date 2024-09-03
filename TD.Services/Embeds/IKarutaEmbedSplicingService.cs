using Discord;
using TD.Services.Embeds.Models;

namespace TD.Services.Embeds
{
    public interface IKarutaEmbedSplicingService
    {
        public KarutaNodeEmbed SpliceKarutaNodeEmbed(IEmbed embed);
        public List<string> GetCardCodesFromEmbed(IEmbed embed);
        public string GetCaouselFrameFromEmbed(IEmbed embed);
    }
}
