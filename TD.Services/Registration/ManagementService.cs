using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TD.DataAccess;
using TD.Domain.Entities;
using TD.Domain.Enums;
using TD.Services.Cache;
using Serilog;

namespace TD.Services.Registration
{
    public class ManagementService : IManagementService
    {
        private readonly TDDbContext _dbContext;
        private readonly CacheService _cacheService;
        public ManagementService(TDDbContext dbContext, CacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        public string AddPrefix(string prefix, ulong guildId)
        {
            var currentPrefixes = _dbContext.Set<Prefix>().Where(x => x.GuildId == guildId).ToList();
            if (!currentPrefixes.Select(x => x.prefix.ToLower()).Contains(prefix.ToLower()))
            {
                var newPrefix = new Prefix
                {
                    prefix = prefix,
                    GuildId = guildId,
                };
                _dbContext.Set<Prefix>().Add(newPrefix);
                _dbContext.SaveChanges();
                return $"Added {prefix} as a new prefix";

            }
            else
            {
                var prefixModel = currentPrefixes.Where(x => x.prefix == prefix && x.GuildId == guildId).FirstOrDefault();
                if (prefixModel != null)
                {
                    _dbContext.Set<Prefix>().Remove(prefixModel);
                    _dbContext.SaveChanges();
                    return $"Removed {prefix} from prefix list";
                }
                return "An Error occured";
            }
        }

        public void CreateRoles(SocketGuild guild)
        {
            var roleNames = new List<RoleName>();
            foreach (var role in guild.Roles.Where(x => x.Permissions.Administrator && !x.Members.All(x => x.IsBot)))
            {
                roleNames.Add(new RoleName { Role = role.Name });
            }
            var roleTypes = Enum.GetValues(typeof(RoleType));
            _dbContext.Set<RoleName>().UpdateRange(roleNames);
            foreach (var roleType in roleTypes)
            {
                if ((RoleType)roleType != RoleType.Shogun)
                    _dbContext.Set<RolePermission>().Update(new RolePermission { GuildId = guild.Id, RoleType = (RoleType)roleType });
            }
            _dbContext.Set<RolePermission>().Update(new RolePermission { GuildId = guild.Id, RoleType = RoleType.Shogun, RoleNames = roleNames });
            _dbContext.SaveChanges();
        }
        public string AddRole(string roleName, RoleType roleType, ulong guildId)
        {
            var role = _dbContext.Set<RoleName>().Where(x => x.Role == roleName).FirstOrDefault() ?? new RoleName { Id = 0, Role = roleName };
            var permission = _dbContext.Set<RolePermission>().FirstOrDefault(x => x.RoleType == roleType && x.GuildId == guildId);
            if (permission == null)
            {
                permission = new RolePermission
                {
                    GuildId = guildId,
                    RoleType = roleType
                };
                _dbContext.Set<RolePermission>().Add(permission);
                Log.Information($"Created RolePErmission of type {roleType} for guild {guildId}");
            }
            if (permission.RoleNames.Contains(role)) return $"Role {roleName} already added to {roleType}";
            permission.RoleNames.Add(role);
            _dbContext.Set<RoleName>().Update(role);
            _dbContext.SaveChanges();
            ReloadCache();
            return $"Role {roleName} added";
        }
        public string RemoveRole(string roleName, RoleType roleType, ulong guildId)
        {
            var role = _dbContext.Set<RoleName>().Include(x => x.RolePermissions).Where(x => x.Role == roleName).FirstOrDefault();
            if (role != null)
            {
                var permission = role.RolePermissions.Where(x => x.RoleType == roleType && x.GuildId == guildId).FirstOrDefault();
                if (permission == null) return $"Role {roleName} not in permission group {roleType}";
                permission.RoleNames.Remove(role);
                _dbContext.SaveChanges();
                ReloadCache();
                return $"Role {roleName} removed from permission group {roleType} succesfully";
            }
            return $"No role with name {roleName}";
        }

        private void ReloadCache()
        {
            _cacheService.permissions.Clear();
            _cacheService.permissions = _dbContext.Set<RolePermission>().Include(x => x.RoleNames).AsNoTracking().ToList();
        }
    }
}
