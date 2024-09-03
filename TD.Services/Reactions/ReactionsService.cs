using Discord;
using Discord.WebSocket;
using TD.Services.Cache;
using TD.Services.Embeds;
using TD.Services.Reactions.Models;

namespace TD.Services.Reactions
{
    public class ReactionsService : IReactionsService
    {
        private readonly IKarutaEmbedSplicingService _splicingService;
        private readonly Publisher _publisher;
        private readonly IKarutaEmbedSplicingService _embedSplicingService;
        private readonly CacheService _cacheService;
        public ReactionsService(IKarutaEmbedSplicingService splicingService, Publisher publisher, IKarutaEmbedSplicingService embedSplicingService, CacheService cacheService)
        {
            _splicingService = splicingService;
            _publisher = publisher;
            _embedSplicingService = embedSplicingService;
            _cacheService = cacheService;
        }
        public async Task HandleReaction(IUserMessage message, SocketReaction reaction, DiscordSocketClient _client)
        {
            switch (reaction.Emote.Name)
            {
                case "NaoCamera":
                    await HandleDaggerAsync(message, _client);
                    break;
                case "NaoCamera1":
                    //await HandleSwords(message, _client);
                    break;
                case "EHHH":
                    await HandleCalendar(message, _client);
                    break;
                case "oooh":
                    await HandleMoney(message);
                    break;
                default:
                    break;
            }
        }
        private async Task HandleDaggerAsync(IUserMessage mess, DiscordSocketClient _client)
        {
            if (mess.Embeds.Any())
            {
                var messageRef = await mess.ReferencedMessage.ReplyAsync("Getting user please wait");
                var embed = mess.Embeds.First();
                var denominator = string.IsNullOrEmpty(embed.Title) ? embed.Author!.Value.Name : embed.Title;
                if (denominator.Contains("Node"))
                {
                    var shogunId = ulong.Parse(embed.Description.Split("\n")[2].Split(":")[1].Trim()[2..][..^1]);
                    var user = await _client.GetUserAsync(shogunId);
                    var userName = "";
                    if (user.Discriminator != "0000")
                        userName = $"{user.Username}#{user.Discriminator}";
                    else
                        userName = user.Username;
                    var message = $"Shogun id: {user.Id}\nShogun username: {userName}";
                    await messageRef.ModifyAsync(x => x.Content = message);
                    if (mess.Embeds.First().Fields.Length > 2)
                        await HandleSwords(mess, _client, messageRef, message);
                }
            }
        }
        private async Task HandleSwords(IUserMessage karutaMessage, DiscordSocketClient _client, IUserMessage infoMessage, string baseMessage)
        {
            var embed = karutaMessage.Embeds.First();
            if (embed.Fields.Length > 2)
            {
                List<AttackingShogunModel> AttackingShogunList = new();
                var flag = true;
                var deductionBase = int.Parse(embed.Footer!.Value.Text.Split("of")[1]);
                do
                {
                    deductionBase = await GetShoguns(embed, _client, deductionBase, AttackingShogunList, CheckIfGoSecondPage(karutaMessage));
                    AttackingShogunList.Sort(delegate (AttackingShogunModel x, AttackingShogunModel y)
                    {
                        if (x.DamageDone > y.DamageDone)
                        {
                            return -1;
                        }
                        return 1;
                    });
                    var replyMessage = "";
                    var totalDamage = AttackingShogunList.Sum(x => x.TotalDamage);
                    foreach (var shogunModel in AttackingShogunList)
                    {
                        if (shogunModel.DamageType == "done")
                        {
                            replyMessage += "⚔️";
                        }
                        else
                        {
                            replyMessage += "🛡";
                        }
                        var shogunName = "";
                        if (shogunModel.Shogun?.Discriminator != "0000")
                            shogunName = $"{shogunModel.Shogun?.Username}#{shogunModel.Shogun?.Discriminator}";
                        else
                            shogunName = shogunModel.Shogun.Username;
                        replyMessage += $"{shogunName} `{shogunModel.Shogun?.Id}` with damage {shogunModel.DamageType}: {shogunModel.DamageDone:N0}\\{shogunModel.TotalDamage:N0}\n";
                    }
                    var replyEmbed = new EmbedBuilder().WithTitle("Battle info:").AddField("Currently fighting shoguns:", replyMessage).Build();
                    await infoMessage.ModifyAsync(x => x.Content = baseMessage + "\n\nCurrently fighting shoguns:\n" + replyMessage);
                    if (CheckIfGoSecondPage(karutaMessage) && deductionBase > 0)
                    {
                        var token = new CancellationTokenSource();
                        token.CancelAfter(TimeSpan.FromSeconds(30));
                        try
                        {
                            karutaMessage = await CheckForEdit(karutaMessage, token.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            flag = false;
                        }
                        embed = karutaMessage.Embeds.First();
                    }
                    else
                    {
                        flag = false;
                    }

                } while (flag);


            }
        }

        private static async Task<int> GetShoguns(IEmbed embed, DiscordSocketClient _client, int deductionBase, List<AttackingShogunModel> AttackingShogunList, bool multiPage)
        {
            var activeBattle = embed.Fields.Last().Value.Split("\n");
            var iterator = 0;
            var skip = 0;
            if (deductionBase < 10 && multiPage)
            {
                skip = 10 - deductionBase;
            }
            foreach (var attack in activeBattle.Where(x => x.StartsWith(">")))
            {
                if (skip > iterator)
                {
                    iterator++;
                }
                else
                {
                    ++iterator;
                    var attackTemplate = attack.Split("·");
                    var attackingShogunTmp = attackTemplate[1].Trim().Substring(2);
                    var attackingShogunId = (ulong)long.Parse(attackingShogunTmp.Substring(0, attackingShogunTmp.Length - 1));
                    var user = await _client.GetUserAsync(attackingShogunId);
                    var attackDamage = attackTemplate[0].Trim().Split("/");
                    var damageToDo = int.Parse(attackDamage[0].Split("**")[1].Replace(",", ""));
                    var damage = int.Parse(attackDamage[1].Replace(",", ""));
                    var finalDamage = damage - damageToDo;
                    if (!AttackingShogunList.Select(x => x.Shogun?.Id).Contains(user.Id))
                    {
                        var damageType = "";
                        if (attackDamage[0].Split("**")[0][2..].Trim() == "⚔️")
                            damageType = "done";
                        else
                            damageType = "defended";
                        AttackingShogunList.Add(new AttackingShogunModel { Shogun = user, DamageDone = finalDamage, DamageType = damageType, TotalDamage = damage });

                    }
                    else
                    {
                        AttackingShogunList.Where(x => x.Shogun?.Id == user.Id).First().DamageDone += finalDamage;
                        AttackingShogunList.Where(x => x.Shogun?.Id == user.Id).First().TotalDamage += damage;
                    }
                }
            }

            return deductionBase - iterator;
        }

        private async Task HandleCalendar(IUserMessage mess, DiscordSocketClient _client)
        {
            if (mess.Embeds.Any())
            {
                var embed = mess.Embeds.First();
                var denominator = string.IsNullOrEmpty(embed.Title) ? embed.Author!.Value.Name : embed.Title;
                if (denominator.Contains("Clan"))
                {
                    var followersMessage = "```";
                    var followers = embed.Fields[0].Value.Split("\n");
                    var iterator = 1;
                    var messageRef = await mess.ReferencedMessage.ReplyAsync("Getting users please wait");
                    if (followers.Any())
                    {
                        foreach (var follower in followers)
                        {
                            var id = follower.Split("<@")[1].Split('>')[0];
                            var followerId = long.Parse(id);
                            var followerUser = await _client.GetUserAsync((ulong)followerId);
                            var followerName = "";
                            if (followerUser.Discriminator != "0000")
                                followerName = $"{followerUser.Username}#{followerUser.Discriminator}";
                            else
                                followerName = followerUser.Username;
                            followersMessage += $"{iterator}. {followerUser.Id} - {followerName}\n";
                            ++iterator;
                        }
                        followersMessage += "```";
                        await messageRef.ModifyAsync(x => x.Content = followersMessage);
                    }
                    else
                    {
                        await messageRef.ModifyAsync(x => x.Content = "No followers to display");
                    }

                }
                if (denominator.Contains("Nodes"))
                {
                    var ListOfShoguns = new List<NodeOwnerModel>();
                    var finallMessage = "```Nodes with shogun Id/Name\n";
                    var messageReference = mess.ReferencedMessage.ReplyAsync("Getting users please wait").Result;
                    ListOfShoguns.AddRange(GetNodes(embed.Description, _client));
                    await messageReference.ModifyAsync(x => x.Content = "Go to the other page");
                    var token = new CancellationTokenSource();
                    token.CancelAfter(TimeSpan.FromSeconds(30));
                    var newMessage = await CheckForEdit(mess, token.Token);
                    await messageReference.ModifyAsync(x => x.Content = "Getting user please wait");
                    ListOfShoguns.AddRange(GetNodes(newMessage.Embeds.First().Description, _client));
                    foreach (var node in ListOfShoguns)
                    {
                        finallMessage += CreateMessage(node, _client);
                    }
                    finallMessage += "```";
                    await messageReference.ModifyAsync(x => x.Content = finallMessage);

                }
                if (denominator.Contains("Bits"))
                {
                    var messageRef = mess.ReferencedMessage.ReplyAsync("Calculating please wait").Result;
                    var bits = embed.Description.Split("\n").Where(x => x.StartsWith('◾')).ToList();
                    var bitsList = new List<BitModel>();
                    var totalAmount = 0;
                    foreach (var bit in bits)
                    {
                        bitsList.Add(GetBit(bit));
                    }
                    if (CheckIfGoSecondPage(mess))
                    {
                        await messageRef.ModifyAsync(x => x.Content = "Go to the next page of your bits");
                        var token = new CancellationTokenSource();
                        token.CancelAfter(TimeSpan.FromSeconds(30));
                        var newMessage = await CheckForEdit(mess, token.Token);
                        await messageRef.ModifyAsync(x => x.Content = "Calculating please wait");
                        embed = newMessage.Embeds.First();
                        bits = embed.Description.Split("\n").Where(x => x.StartsWith('◾')).ToList();
                        foreach (var bit in bits)
                        {
                            var bitModel = GetBit(bit);
                            if (!bitsList.Exists(x => x.Name == bitModel.Name))
                                bitsList.Add(bitModel);
                        }
                    }
                    var finalMessage = "";
                    foreach (var bit in bitsList)
                    {
                        totalAmount += bit.Amount;
                        finalMessage += $"{bit.StringAmount} · {bit.Name}\n";
                    }
                    finalMessage += $"{totalAmount} total bits";

                    await messageRef.ModifyAsync(x => x.Content = finalMessage);
                }
                if (denominator.Contains("Card Collection"))
                {
                    await RipCardCodes(mess, embed, true);
                }
            }
        }
        private async Task HandleMoney(IUserMessage mess)
        {
            if (mess.Embeds.Any())
            {
                var embed = mess.Embeds.First();
                var denominator = string.IsNullOrEmpty(embed.Title) ? embed.Author!.Value.Name : embed.Title;
                if (denominator.Contains("Bits"))
                {
                    var messageRef = mess.ReferencedMessage.ReplyAsync("Calculating please wait").Result;
                    var bits = embed.Description.Split("\n").Where(x => x.StartsWith('◾')).ToList();
                    var bitsList = new List<BitModel>();
                    var totalMorphs = 0;
                    var totalAmount = 0;
                    foreach (var bit in bits)
                    {
                        bitsList.Add(GetBit(bit));
                    }
                    if (CheckIfGoSecondPage(mess))
                    {
                        await messageRef.ModifyAsync(x => x.Content = "Go to the next page of your bits");
                        var token = new CancellationTokenSource();
                        token.CancelAfter(TimeSpan.FromSeconds(30));
                        var newMessage = await CheckForEdit(mess, token.Token);
                        embed = newMessage.Embeds.First();
                        bits = embed.Description.Split("\n").Where(x => x.StartsWith('◾')).ToList();
                        foreach (var bit in bits)
                        {
                            var bitModel = GetBit(bit);
                            if (!bitsList.Exists(x => x.Name == bitModel.Name))
                                bitsList.Add(bitModel);
                        }
                    }
                    var finalMessage = "";
                    foreach (var bit in bitsList)
                    {
                        totalAmount += bit.Amount;
                        totalMorphs += bit.Morphs;
                        finalMessage += $", {bit.Amount} {bit.Name}";
                    }
                    finalMessage = finalMessage.Trim(',');
                    finalMessage += $"\n**{totalAmount}** bits (**{totalMorphs}** total morphs | {totalAmount / 250} combined morphs)";
                    await messageRef.ModifyAsync(x => x.Content = finalMessage);

                }
                if (denominator.Contains("Card Collection"))
                {
                    await RipCardCodes(mess, embed, false);
                }
            }
        }
        private async Task RipCardCodes(IUserMessage mess, IEmbed embed, bool appendComas)
        {
            var flag = true;
            var messageRef = mess.ReferencedMessage.ReplyAsync("Getting codes please wait").Result;
            var first = true;
            var message = "";
            do
            {
                var cardCodes = _splicingService.GetCardCodesFromEmbed(embed);
                if (!cardCodes.Any())
                {
                    await messageRef.ModifyAsync(x => x.Content = "No card codes to get :(");
                    return;
                }
                var token = new CancellationTokenSource();
                token.CancelAfter(TimeSpan.FromSeconds(30));
                foreach (var cardCode in cardCodes)
                {
                    if (!message.Contains(cardCode))
                    {
                        var temp = $"{(appendComas ? ", " : ' ')}{cardCode}";
                        if (message.Length + temp.Length > 1900)
                        {
                            await messageRef.ModifyAsync(x => x.Content = message);
                            messageRef = mess.ReferencedMessage.ReplyAsync("Getting codes please wait").Result;
                            first = true;
                            message = "";
                        }
                        message += temp;
                    }
                }
                if (first)
                {
                    message = message.TrimStart(',');
                    first = false;
                }
                await messageRef.ModifyAsync(x => x.Content = message);
                try
                {
                    mess = await CheckForEdit(mess, token.Token);
                }
                catch (TaskCanceledException)
                {
                    flag = false;
                }
                embed = mess.Embeds.First();
            } while (flag);
        }
        public async Task<IUserMessage> CheckForEdit(IUserMessage mess, CancellationToken token, List<Event>? extraEvents = null)
        {
            var flag = true;
            var editEvent = new Event
            {
                EventName = "edit",
                EventType = EventType.MessageEdit,
                CreatedAt = DateTime.Now,
                MessageId = mess.Id
            };
            editEvent.OnRaise += () => { flag = false; };
            if (extraEvents != null)
            {
                foreach (var extraEvent in extraEvents)
                    extraEvent.OnRaise += () => { flag = false; };
            }
            _publisher.Events.Add(editEvent);
            var channel = mess.Channel;
            while (flag)
            {
                if (token.IsCancellationRequested)
                {
                    _publisher.ClearEvent(editEvent);
                    throw new TaskCanceledException();
                }
            }
            var newMessage = await channel.GetMessageAsync(mess.Id);
            _publisher.ClearEvent(editEvent);
            return (IUserMessage)newMessage;
        }
        private static List<NodeOwnerModel> GetNodes(string EmbedDescription, DiscordSocketClient _clinet)
        {
            var listOfShoguns = new List<NodeOwnerModel>();
            var nodes = EmbedDescription.Split("\n");
            foreach (var node in nodes)
            {
                var info = node.Split("·");
                if (!info[0].Contains("gold"))
                {
                    var name = info.First().Split("`")[1];
                    listOfShoguns.Add(new NodeOwnerModel
                    {
                        OwnerId = ulong.Parse(info.Last().Split("<@").Last().Trim('>')),
                        NodeName = name,
                        NodeNumber = int.Parse(info.First().Split(".").First()),
                    });


                }
            }
            return listOfShoguns;
        }
        private static string CreateMessage(NodeOwnerModel node, DiscordSocketClient _client)
        {
            var shogun = _client.GetUserAsync(node.OwnerId).Result;
            string? shogunName;
            if (shogun.Discriminator != "0000")
                shogunName = $"{shogun.Username}#{shogun.Discriminator}";
            else
                shogunName = shogun.Username;
            return $"{node.NodeName} | {shogun.Id} {shogunName}\n";
        }
        private static BitModel GetBit(string bit)
        {
            var bitComponents = bit.Split('·');
            var amountString = bitComponents[0].Split("**")[1];
            var amount = int.Parse(amountString.Replace(",", ""));
            return new BitModel()
            {
                StringAmount = bitComponents[0].Split("**")[1],
                Name = bitComponents[1].Trim().Trim('`') + "s",
                Amount = amount,
                Morphs = amount / 250
            };
        }
        private static bool CheckIfGoSecondPage(IUserMessage message)
        {
            return message.Components.Any();
        }
    }
}
