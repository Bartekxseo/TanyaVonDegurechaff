using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using TD.DataAccess;
using TD.Services.Cache;
using Serilog;
using System.Timers;

namespace TD.Bot.HostedServices
{
    public class TimedService : IHostedService, IDisposable
    {
        private readonly CacheService _cacheService;
        private readonly TDDbContext _dbContext;
        private readonly Publisher _publisher;
        private readonly DiscordSocketClient _client;

        public bool logNextPublisher = true;
        public bool logNextComp = true;
        private System.Threading.Timer? _publisherTimer;
        public bool started = false;
        public TimedService(
            CacheService cacheService, TDDbContext dbContext,
            Publisher publisher, DiscordSocketClient client)
        {
            _cacheService = cacheService;
            _dbContext = dbContext;
            _publisher = publisher;
            _client = client;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (started)
            {
                Log.Error("Service already started");
                return Task.CompletedTask;
            }
            started = true;
            Log.Information("Hosted service started");
            _publisherTimer = new System.Threading.Timer(CheckTimedEntities, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
        private async void CheckTimedEntities(object? state)
        {
            if (logNextPublisher) Log.Information("Publisher check started");
            var buttonEvents = _publisher.Events.Where(x => x.EventType == EventType.Button && DateTime.Now - x.CreatedAt > TimeSpan.FromSeconds(60)).ToList();
            var editEvents = _publisher.Events.Where(x => x.EventType == EventType.MessageEdit && DateTime.Now - x.CreatedAt > TimeSpan.FromSeconds(30)).ToList();
            try
            {
                foreach (var buttonEvent in buttonEvents)
                {
                    Log.Debug($"Disposing event for message {buttonEvent.MessageId} and of type {buttonEvent.EventType}");
                    buttonEvent.ClearListeners();
                    _publisher.Events.Remove(buttonEvent);
                }
                foreach (var editEvent in editEvents)
                {
                    Log.Debug($"Disposing event for message {editEvent.MessageId} and of type {editEvent.EventType}");
                    editEvent.ClearListeners();
                    _publisher.Events.Remove(editEvent);
                }

            }
            catch (Exception ex)
            {
                BetterLog.Exception(ex, "Error in timed service");
            }
            if (logNextPublisher) { Log.Information("Publisher check ended"); logNextPublisher = false; }

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!started)
            {
                Log.Error("Service already stopped");
                return Task.CompletedTask;
            }
            started = false;
            Log.Information("Hosted service stopped");
            _publisherTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _publisherTimer?.Dispose();
        }
    }
}
