using Discord.WebSocket;
using TD.Domain.Enums;

namespace TD.Services.Registration
{
    public interface IManagementService
    {
        public string AddPrefix(string prefix, ulong guildId);
        public void CreateRoles(SocketGuild guild);
        string AddRole(string roleName, RoleType roleType, ulong guildId);
        string RemoveRole(string roleName, RoleType roleType, ulong guildId);
    }
}
