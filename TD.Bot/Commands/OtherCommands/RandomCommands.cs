using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TD.Bot.Commands.Extras.Preconditions;
using TD.Bot.HostedServices;
using TD.DataAccess;
using TD.Domain.Enums;
using TD.Services.Cache;
using TD.Services.Embeds;

namespace TD.Bot.Commands.OtherCommands
{
    public class RandomCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IKarutaEmbedSplicingService _embedSplicingService;
        private readonly TimedService _timedService;
        private readonly TDDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        public RandomCommands(IKarutaEmbedSplicingService embedSplicingService, TimedService timedService, TDDbContext context, IServiceProvider serviceProvider)
        {
            _embedSplicingService = embedSplicingService;
            _timedService = timedService;
            _context = context;
            _serviceProvider = serviceProvider;
        }
        [Command("TestMorph")]
        [Alias("tm", "mt")]
        public Task TestMorphs([Remainder] string frame)
        {
            if (Context.Message.ReferencedMessage.Embeds.Any())
            {
                var embed = Context.Message.ReferencedMessage.Embeds.First();
                var denominator = string.IsNullOrEmpty(embed.Title) ? embed.Author!.Value.Name : embed.Title;
                if (denominator.Contains("Card Collection"))
                {
                    frame = frame.Trim();
                    var cardCodes = _embedSplicingService.GetCardCodesFromEmbed(Context.Message.ReferencedMessage.Embeds.First());
                    if (!frame.Contains("frame"))
                    {
                        frame += " frame";
                    }
                    var finallMessage = "__Here are your commands:__\n";
                    foreach (var code in cardCodes)
                    {
                        finallMessage += $"ku {frame} {code}\n";
                    }
                    return Context.Message.ReplyAsync(finallMessage);
                }
            }
            return Context.Message.ReplyAsync("Reply to a kc message to generate ku commands");
        }
        [RequireOwner]
        [Command("safeExit")]
        public async Task SafeExit()
        {
            await _timedService.StopAsync(new CancellationToken());
            _timedService.Dispose();
            await Context.Message.ReplyAsync("Service stopped and disposed");
        }
        [RequireOwner]
        [Command("restart")]
        public async Task Restart()
        {
            await _timedService.StartAsync(new CancellationToken());
            await Context.Message.ReplyAsync("Service restarted");
        }
        [RequireOwner]
        [Command("status")]
        public async Task CheckStatus()
        {
            if (_timedService.started)
            {
                await Context.Message.ReplyAsync("Service is running");
                return;
            }
            else
            {
                await Context.Message.ReplyAsync("Service is stopped");
                return;
            }
        }

        [RequireOwner]
        [Command("log")]
        public Task LogNextRun()
        {
            _timedService.logNextComp = true;
            _timedService.logNextPublisher = true;
            return Context.Message.ReplyAsync("Next runs will get logged in console");
        }
        [RequireOwner]
        [Command("RestartCache")]
        public async Task RestartCache()
        {
            var message = await Context.Message.ReplyAsync("Reseting cache");
            await Task.Run(()=> _serviceProvider.GetRequiredService<CacheService>().RecreateEntities());
            await message.ModifyAsync(x => x.Content = "Cache reset");
            return;
        }
    }
}
