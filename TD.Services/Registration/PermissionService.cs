using Discord.WebSocket;
using TD.Services.Cache;

namespace TD.Services.Registration
{
    public class PermissionService : IPermissionService
    {
        private readonly CacheService _cacheService;
        public PermissionService(CacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public bool CheckIfValidNodeTaker(SocketGuildUser user)
        {
            if (user == null) return false;
            var permissions = _cacheService.permissions.Where(x => x.RoleType == Domain.Enums.RoleType.NodeTaker).Where(x => x.GuildId == user.Guild.Id).SelectMany(x => x.RoleNames).Select(x => x.Role);
            return user.Roles.Any(x => permissions.Contains(x.Name));
        }
    }
}
