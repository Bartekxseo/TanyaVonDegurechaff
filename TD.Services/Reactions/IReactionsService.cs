using Discord;
using Discord.WebSocket;
using TD.Services.Cache;

namespace TD.Services.Reactions
{
    public interface IReactionsService
    {
        public Task HandleReaction(IUserMessage message, SocketReaction reaction, DiscordSocketClient _client);
        public Task<IUserMessage> CheckForEdit(IUserMessage mess, CancellationToken token, List<Event>? extraEvents = null);
    }
}
