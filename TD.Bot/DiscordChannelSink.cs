using Discord;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace TD.Bot
{
    public class DiscordChannelSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;
        private readonly IMessageChannel _channel;
        public DiscordChannelSink(IFormatProvider? formatProvider, IMessageChannel channel)
        {
            _formatProvider = formatProvider;
            _channel = channel;
        }
        public async void Emit(LogEvent logEvent)
        {
            try
            {
                var message = logEvent.RenderMessage(_formatProvider);
                var log = $"[<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:f> {GetLevelShort(logEvent.Level)}] {message}";
                if (logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal)
                {
                    log = $"<@229594973446733826> {message}";
                }
                await _channel.SendMessageAsync(log);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in discord sink detected");
            }

        }

        public static string GetLevelShort(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Error:
                    return "ERR";
                case LogEventLevel.Warning:
                    return "WRN";
                case LogEventLevel.Information:
                    return "INF";
                case LogEventLevel.Verbose:
                    return "VRB";
                case LogEventLevel.Fatal:
                    return "FTL";
                case LogEventLevel.Debug:
                    return "DBG";
                default:
                    return string.Empty;
            }
        }
    }
    public static class DiscordChannelSinkExtensions
    {
        public static LoggerConfiguration DiscordChannelSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IMessageChannel channel,
                  IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new DiscordChannelSink(formatProvider, channel));
        }
    }

    public static class BetterLog
    {
        public static void Exception(Exception exception)
        {
            Log.Error(exception, "{exception}", exception);
        }
        public static void Exception(Exception exception,string extraMessage)
        {
            Log.Error(exception, extraMessage+"\n{exception}", exception);
        }
    }
}