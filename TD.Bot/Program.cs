using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TD.Bot;
using TD.Bot.Handlers;
using TD.Bot.HostedServices;
using TD.DataAccess;
using TD.Domain.Entities;
using TD.Services;
using TD.Services.Cache;
using TD.Services.Embeds;
using TD.Services.Reactions;
using TD.Services.Registration;
using Serilog;
using System.Configuration;

public class Program
{
    static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }
    private readonly IServiceProvider _services;

    private Program()
    {
        _services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var connectionString = ConfigurationManager.AppSettings.Get("connectionString");
        var map = new ServiceCollection()
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton(x => new CommandService())
            .AddSingleton<PrefixHandler>()
            .AddSingleton<ReactionsHandler>()
            .AddSingleton<ButtonClickHandler>()
            .AddScoped<IKarutaEmbedSplicingService, KarutaEmbedSplicingService>()
            .AddScoped<IMessageEmbedCreatorService, MessageEmbedCreatorService>()
            .AddScoped<IReactionsService, ReactionsService>()
            .AddDbContext<TDDbContext>(options => options.UseSqlServer(connectionString))
            .AddSingleton<TimedService>()
            .AddScoped<IManagementService, ManagementService>()
            .AddScoped<IPermissionService, PermissionService>()
            .AddAutoMapper(typeof(AutomapperProfile));

        return map.BuildServiceProvider();
    }
    private static Task Logger(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Log.Error($"{message.Source}: {message.Message} {message.Exception}");
                break;
            case LogSeverity.Warning:
                Log.Warning($"{message.Source}: {message.Message} {message.Exception}");
                break;
            case LogSeverity.Info:
                Log.Information($"{message.Source}: {message.Message} {message.Exception}");
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Log.Debug($"{message.Source}: {message.Message} {message.Exception}");
                break;
        }
        return Task.CompletedTask;
    }

    private async Task MainAsync()
    {
        var _client = _services.GetRequiredService<DiscordSocketClient>();
        await InitCommands();
        var token = ConfigurationManager.AppSettings.Get("token");

        await _client.LoginAsync(TokenType.Bot, token);
        _client.Ready += InitCache;
        await _client.StartAsync();
        _client.Log += Logger;
        await Task.Delay(Timeout.Infinite);
    }
    private async Task InitCache()
    {
        var cache = _services.GetRequiredService<CacheService>();
        if (!cache.IsInitialised)
        {
            await Task.Run(async () =>
            {
                var workingDirectory = Environment.CurrentDirectory;
                var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.Parent?.FullName;
                var directory = string.IsNullOrEmpty(projectDirectory) ? workingDirectory : projectDirectory;
                var db = _services.GetRequiredService<TDDbContext>();
                cache.AppPath = directory;
                var client = _services.GetRequiredService<DiscordSocketClient>();
                await cache.RecreateEntities();
            });
            cache.IsInitialised = true;
        }
        InitLogger();
        await _services.GetRequiredService<TimedService>().StartAsync(new CancellationToken());
        return;

    }
    private void InitLogger()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.Parent?.FullName;
        var logFile = ConfigurationManager.AppSettings.Get("logFile");
        var loggingFolder = string.IsNullOrEmpty(projectDirectory) ? workingDirectory : projectDirectory;
        if (!Directory.Exists(loggingFolder + "/" + logFile))
            Directory.CreateDirectory(loggingFolder + "/" + logFile);
        //var logChannel = _services.GetRequiredService<DiscordSocketClient>().GetGuild(1104515583044755481).GetChannel(1199801399135973496) as IMessageChannel;
        //Log.Logger = new LoggerConfiguration()
        //.MinimumLevel.Debug()
        //.WriteTo.Console()
        //.WriteTo.File(loggingFolder + "/" + logFile + "/logs.log", rollingInterval: RollingInterval.Day)
        //.WriteTo.DiscordChannelSink(logChannel!)
        //.CreateLogger();
        Log.Debug(loggingFolder);
    }


    private async Task InitCommands()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.Parent?.FullName;
        var logFile = ConfigurationManager.AppSettings.Get("logFile");
        var loggingFolder = string.IsNullOrEmpty(projectDirectory) ? workingDirectory : projectDirectory;
        if (!Directory.Exists(loggingFolder + "/" + logFile))
            Directory.CreateDirectory(loggingFolder + "/" + logFile);
        Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.Console()
       .WriteTo.File(loggingFolder + "/" + logFile + "/logs.log", rollingInterval: RollingInterval.Day)
       .CreateLogger();
        var sCommands = _services.GetRequiredService<InteractionService>();
        sCommands.Log += Logger;
        var pCommands = _services.GetRequiredService<CommandService>();
        pCommands.Log += Logger;
        await _services.GetRequiredService<PrefixHandler>().InitializeAsync();
        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await _services.GetRequiredService<ReactionsHandler>().InitializeAsync();
        await _services.GetRequiredService<ButtonClickHandler>().InitialiseAsync();

    }




}
