using Discord;
using Discord.WebSocket;
using TD.Services.Embeds.Models;
using System.Text.RegularExpressions;

namespace TD.Services.Embeds
{
    public class KarutaEmbedSplicingService : IKarutaEmbedSplicingService
    {
        private readonly DiscordSocketClient _client;
        public KarutaEmbedSplicingService(DiscordSocketClient client)
        {
            _client = client;
        }
        public KarutaNodeEmbed SpliceKarutaNodeEmbed(IEmbed embed)
        {
            var currentClanSize = int.Parse(embed.Fields[1].Value.Split("\n").Last().Split("`")[1]);
            var shogunId = ulong.Parse(embed.Description.Split("\n")[2].Split(":")[1].Trim()[2..][..^1]);
            TimeType timeType = TimeType.Minutes; int timeToCapture = -1;
            if (embed.Fields.Last().Value.Split("\n")[1].Contains("This node will be captured"))
            {
                timeType = embed.Fields.Last().Value.Split("\n")[1].Split('`')[1].Split(' ')[1].Contains("minute") ? TimeType.Minutes : TimeType.Seconds;
                timeToCapture = int.Parse(embed.Fields.Last().Value.Split("\n")[1].Split('`')[1].Split(' ')[0]);
            }
            var currentPowerValue = int.Parse(embed.Description.Split("\n")[4].Split("**")[1].Replace(",", ""));
            var timeSinceCaptureInMin = ((25000 - currentPowerValue) / currentClanSize) * 5;
            var nodeName = embed.Title.Split(':')[1].Trim();
            return new KarutaNodeEmbed
            {
                ClanSize = currentClanSize,
                EstimatedTimeSinceCapture = timeSinceCaptureInMin,
                NodeName = nodeName,
                CurrentPower = currentPowerValue,
                TimeToCapture = timeToCapture,
                HolderId = shogunId,
                TimeType = timeType,
            };
        }
        public List<string> GetCardCodesFromEmbed(IEmbed embed)
        {
            var cards = embed.Description.Split("\n").Where(x => !string.IsNullOrEmpty(x) && !x.Contains("Cards owned")).ToList();
            var cardCodes = new List<string>();
            int i = 0;
            if (cards.First().Contains("The list is empty")) return cardCodes;
            foreach (var element in cards[0].Split("`"))
            {
                if (Regex.IsMatch(element.Trim(), "^[a-zA-Z0-9]*$"))
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            foreach (var card in cards)
            {
                cardCodes.Add(card.Split("`")[i].Trim());
            }
            return cardCodes;
        }

        public string GetCaouselFrameFromEmbed(IEmbed embed)
        {
            return embed.Description.Split(',')[1].Trim().Split(':')[1].Trim().TrimEnd('*');
        }
    }
}
