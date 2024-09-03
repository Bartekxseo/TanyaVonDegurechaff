using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace TD.Bot.Handlers
{
    public class InteractionHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceScopeFactory _services;
        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceScopeFactory services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services.CreateScope().ServiceProvider);

            _commands.SlashCommandExecuted += HandleSlashCommandResult;
            _client.InteractionCreated += HandleInteraction;

            _client.Ready += CreateSlashCommands;
        }
        private async Task CreateSlashCommands()
        {
            await _commands.RegisterCommandsGloballyAsync();
        }
        private async Task HandleInteraction(SocketInteraction interaction)
        {
            //var scope = _services.CreateScope();
            var ctx = new SocketInteractionContext(_client, interaction);
            try
            {
                string commandName;
                if (interaction is ISlashCommandInteraction slashCommandInteraction)
                {
                    commandName = slashCommandInteraction.Data.Name;
                }
                else
                {
                    if (interaction is IComponentInteraction componentInteraction)
                    {
                        commandName = componentInteraction.Data.CustomId.ToString();
                    }
                    else
                    {
                        if (interaction is IUserCommandInteraction userCommandInteraction)
                        {
                            commandName = userCommandInteraction.Data.Name;
                        }
                        else
                        {
                            if (interaction is IMessageCommandInteraction messageCommandInteraction)
                            {
                                commandName = messageCommandInteraction.Data.Name;
                            }
                            else
                            {
                                if (interaction is IAutocompleteInteraction autocompleteInteraction)
                                {
                                    commandName = autocompleteInteraction.Data.CommandName;
                                }
                                else
                                {

                                    if (interaction is IModalInteraction modalInteraction)
                                    {
                                        commandName = modalInteraction.Data.CustomId.ToString();
                                    }
                                    else
                                    {
                                        commandName = "Unknown";
                                    }
                                }

                            }


                        }

                    }

                }

                Log.Debug($"User {ctx.User.Username} ({ctx.User.Id}) executed slash command {commandName} in {ctx.Channel.Name} ({ctx.Channel.Id})");
                await ctx.Interaction.DeferAsync();
                await _commands.ExecuteCommandAsync(ctx, _services.CreateScope().ServiceProvider);
            }
            catch (Exception ex)
            {
                BetterLog.Exception(ex);
                await ctx.Interaction.ModifyOriginalResponseAsync(x => x.Content = ex.Message);
            }
        }
        private async Task HandleSlashCommandResult(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Content = $"Unmet Precondition: {arg3.ErrorReason}");
                        break;
                    case InteractionCommandError.UnknownCommand:
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Content = "Unknown command");
                        break;
                    case InteractionCommandError.BadArgs:
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Content = "Invalid number or arguments");
                        break;
                    case InteractionCommandError.Exception:
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Content = $"Command exception: {arg3.ErrorReason}");
                        break;
                    case InteractionCommandError.Unsuccessful:
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Content = "Command could not be executed");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
