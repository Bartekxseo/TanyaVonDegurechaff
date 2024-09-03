using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TD.Bot.Commands.Extras.Preconditions;
using TD.Domain.Enums;
using TD.Services.Registration;


namespace TD.Bot.Commands.OtherCommands
{
    [RequireContext(ContextType.Guild)]
    public class ManagementCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IManagementService _managementService;
        public ManagementCommands(IManagementService managementService)
        {
            _managementService = managementService;
        }
        [RequireRoleType(RoleType.Shogun)]
        [Command("addPrefix")]
        [Alias("prefix", "setPrefix")]
        public Task AddPrefix(string prefix)
        {
            return Context.Message.ReplyAsync(_managementService.AddPrefix(prefix, Context.Guild.Id));
        }
        [RequireOwner]
        [Command("createRoles")]
        public Task CreateRoles()
        {
            _managementService.CreateRoles(Context.Guild);
            return Context.Message.ReplyAsync("Roles created succesfuly");
        }
        [RequireRoleType(RoleType.Shogun)]
        [Command("add role")]
        public Task AddRole(RoleType roleType, SocketRole role)
        {
            return Context.Message.ReplyAsync(_managementService.AddRole(role.Name, roleType, Context.Guild.Id));
        }
        [RequireRoleType(RoleType.Shogun)]
        [Command("remove role")]
        public Task RemoveRole(RoleType roleType, SocketRole role)
        {
            return Context.Message.ReplyAsync(_managementService.RemoveRole(role.Name, roleType, Context.Guild.Id));
        }

    }
}
