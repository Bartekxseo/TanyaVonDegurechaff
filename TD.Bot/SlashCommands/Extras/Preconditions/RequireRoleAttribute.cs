using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TD.Domain.Enums;
using TD.Services.Cache;
using System.Configuration;
using System.Data;

namespace TD.Bot.SlashCommands.Extras.Preconditions
{
    public class RequireRoleTypeAttribute : PreconditionAttribute
    {
        private readonly RoleType _type;
        public RequireRoleTypeAttribute(RoleType type) => _type = type;
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.User.Id == 229594973446733826) return Task.FromResult(PreconditionResult.FromSuccess());
            var permitedUsers = ConfigurationManager.AppSettings.Get("permitedUsers");
            if (permitedUsers != null && permitedUsers.Contains(context.User.Id.ToString())) return Task.FromResult(PreconditionResult.FromSuccess());
            if (context.User is SocketGuildUser gUser)
            {
                if (gUser.Roles.Any(x => x.Permissions.Administrator)) return Task.FromResult(PreconditionResult.FromSuccess());
                var permissions = services.GetRequiredService<CacheService>().permissions.ToList();
                var adminPerms = permissions.FirstOrDefault(x => x.RoleType == RoleType.Shogun && context.Guild.Id == x.GuildId);
                if (adminPerms != null && adminPerms.RoleNames.Any())
                {
                    var roleNames = adminPerms.RoleNames.Select(x => x.Role);
                    if (gUser.Roles.Any(r => roleNames.Contains(r.Name))) return Task.FromResult(PreconditionResult.FromSuccess());
                }
                var permitedRoles = permissions.Where(x => x.RoleType == _type && context.Guild.Id == x.GuildId).FirstOrDefault();
                if (permitedRoles != null && permitedRoles.RoleNames.Any())
                {
                    var roleNames = permitedRoles.RoleNames.Select(x => x.Role);
                    if (gUser.Roles.Any(r => roleNames.Contains(r.Name)))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    else
                        return Task.FromResult(PreconditionResult.FromError($"You dont't have the role required to run this command."));
                }
                else
                    return Task.FromResult(PreconditionResult.FromError($"This server has not set up role permissions"));
            }
            return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
