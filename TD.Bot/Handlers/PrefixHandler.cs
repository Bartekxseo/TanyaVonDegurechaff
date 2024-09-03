using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TD.Services.Cache;
using TD.Services.Extras;
using TD.Services.Registration;
using Serilog;
using System.Reflection;

namespace TD.Bot.Handlers
{
    public class PrefixHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceScopeFactory _services;
        private readonly CacheService _cacheService;
        private readonly Publisher _publisher;

        public PrefixHandler(
            DiscordSocketClient client,
            CommandService commands,
            IServiceScopeFactory services,
            CacheService cacheService,
            Publisher publisher
            )
        {
            _client = client;
            _commands = commands;
            _services = services;
            _cacheService = cacheService;
            _publisher = publisher;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services.CreateScope().ServiceProvider);
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageUpdated += MessageUpdated;
            _client.JoinedGuild += JoinedGuild;
            _client.LeftGuild += LeftGuild;
        }

        private Task JoinedGuild(SocketGuild arg)
        {
            _services.CreateScope().ServiceProvider.GetRequiredService<IManagementService>().CreateRoles(arg);
            Log.Information($"Joined guild {arg.Id} {arg.Name}");
            return Task.CompletedTask;
        }

        private Task LeftGuild(SocketGuild arg)
        {
            Log.Information($"Left guild {arg.Id} {arg.Name}");
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            _publisher.Events.FirstOrDefault(x => x.MessageId == arg2.Id && x.EventType == EventType.MessageEdit)?.Raise();
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            //var scope = _services.CreateScope();
            if (arg is not SocketUserMessage msg) return;
            if (msg.Author.Id == _client.CurrentUser.Id) return;
            var tempChan = msg.Channel as SocketGuildChannel;
            var prefixes = _cacheService.prefixes.Where(x => x.GuildId == tempChan?.Guild.Id).Select(x => x.prefix).ToArray();
            int pos = 0;
            if (msg.HasStringPrefix("d!", ref pos, StringComparison.OrdinalIgnoreCase) || msg.HasOneOfStringPrefix(prefixes, ref pos, StringComparison.OrdinalIgnoreCase) || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                var context = new SocketCommandContext(_client, msg);
                var result = await _commands.ExecuteAsync(context, pos, _services.CreateScope().ServiceProvider);
                if(result.Error != CommandError.UnknownCommand)
                {
                    Log.Debug($"User {context.User.Username} ({context.User.Id}) executed command {context.Message.Content[pos..]} in {context.Channel.Name} ({context.Channel.Id})");
                    Log.Debug($"Command executed with result {result}");
                }
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.ReplyAsync(result.ErrorReason);
            }
            return;

        }
    }
}
