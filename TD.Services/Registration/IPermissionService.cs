using Discord.WebSocket;

namespace TD.Services.Registration
{
    public interface IPermissionService
    {
        bool CheckIfValidNodeTaker(SocketGuildUser user);
    }
}
