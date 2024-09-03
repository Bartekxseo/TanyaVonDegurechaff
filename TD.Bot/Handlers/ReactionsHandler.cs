using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TD.Services.Cache;
using TD.Services.Embeds;
using TD.Services.Reactions;
using TD.Services.Registration;
using Serilog;
using System.Reactive;

namespace TD.Bot.Handlers
{
    public class ReactionsHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Publisher _publisher;
        private readonly IPermissionService _permissionService;
        private readonly Dictionary<string, Emote> _emoteDictionary = new();

        public ReactionsHandler(DiscordSocketClient client, IServiceProvider services, Publisher publisher, IPermissionService permissionService)
        {
            _client = client;
            _services = services;
            _publisher = publisher;
            _permissionService = permissionService;
        }
        public Task InitializeAsync()
        {
            _client.ReactionAdded += HandleReactionAsync;
            _client.ReactionRemoved += HandleRemoveReactionAsync;
            _client.MessageReceived += HandleMessageAsync;
            InitializeEmoteDictionary();
            return Task.CompletedTask;
        }


        private Task HandleMessageAsync(SocketMessage arg)
        {
            _ = Task.Run(() =>
            {
                var tempChan = arg.Channel as SocketGuildChannel;
                if (arg is not SocketUserMessage msg) return;
                if (msg.Author.IsBot && msg.Author.Username == "Karuta" && msg.Embeds.Any())
                {
                    Emote emote;
                    var denominator = string.IsNullOrEmpty(msg.Embeds.First().Title) ? msg.Embeds.First().Author!.Value.Name : msg.Embeds.First().Title;
                    if (denominator.Contains("Bits"))
                    {
                        emote = _emoteDictionary["oooh"];
                        _ = msg.AddReactionAsync(emote);
                        emote = _emoteDictionary["ehhh"];
                        _ = msg.AddReactionAsync(emote);
                        return;
                    }
                    if (denominator.Contains("Card Collection"))
                    {
                        emote = _emoteDictionary["ehhh"];
                        _ = msg.AddReactionAsync(emote);
                        emote = _emoteDictionary["oooh"];
                        _ = msg.AddReactionAsync(emote);
                        return;
                    }
                    if (denominator.Contains("Clan") || denominator.Contains("Nodes"))
                    {
                        emote = _emoteDictionary["ehhh"];
                        _ = msg.AddReactionAsync(emote);
                        return;
                    }


                    return;
                };
            });
            return Task.CompletedTask;

        }

        private Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _services.CreateScope();
                if (_client.GetUser(reaction.UserId).IsBot) return;
                var mess = message.GetOrDownloadAsync().Result;
                if (mess == null) return;
                if (mess.Author.Username == "Karuta" && mess.ReferencedMessage.Author == reaction.User.Value)
                {
                    if (mess.Reactions.Where(x => x.Key.Name == reaction.Emote.Name).FirstOrDefault().Value.IsMe)
                    {
                        var _reactionsService = scope.ServiceProvider.GetRequiredService<IReactionsService>();
                        await _reactionsService.HandleReaction(mess, reaction, _client);
                        return;
                    }
                }
                if (mess.Author.IsBot) return;
            });
            return Task.CompletedTask;

        }
        private Task HandleRemoveReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _services.CreateScope();
                var mess = await message.GetOrDownloadAsync();
            });
            return Task.CompletedTask;

        }

        private void InitializeEmoteDictionary()
        {
            if (Emote.TryParse("<:oooh:1112055850522116227>", out var emote))
                _emoteDictionary.Add("oooh", emote);
            if (Emote.TryParse("<:EHHH:1112000487286394931>", out emote))
                _emoteDictionary.Add("ehhh", emote);
            if (Emote.TryParse("<:NaoCamera:1112000501274378302>", out emote))
                _emoteDictionary.Add("NaoCamera", emote);
            if (Emote.TryParse("<:NaoCamera1:1112000512477372437>", out emote))
                _emoteDictionary.Add("NaoCamera1", emote);
        }

        private bool CheckRolesForTairo(IUser user)
        {
            if (user is SocketGuildUser gUser)
            {
                var rolesPermited = _services.GetRequiredService<CacheService>().permissions.Where(x => (x.RoleType == Domain.Enums.RoleType.Tairo || x.RoleType == Domain.Enums.RoleType.Shogun) && x.GuildId == gUser.Guild.Id).ToList().SelectMany(x => x.RoleNames.Select(x => x.Role));
                if (gUser.Roles.Any(x => rolesPermited.Contains(x.Name)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
