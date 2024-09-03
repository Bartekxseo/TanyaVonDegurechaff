using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TD.Services.Cache;


namespace TD.Bot.Handlers
{
    public class ButtonClickHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly Publisher _publisher;
        private readonly IServiceProvider _service;

        public ButtonClickHandler(DiscordSocketClient client, Publisher publisher, IServiceProvider service)
        {
            _client = client;
            _publisher = publisher;
            _service = service;
        }

        public Task InitialiseAsync()
        {
            _client.ButtonExecuted += ButtonHandler;
            return Task.CompletedTask;
        }


        public async Task ButtonHandler(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "confirmChangeButton":
                    _publisher.Events.FirstOrDefault(x => x.EventName == component.Data.CustomId && x.MessageId == component.Message.Id)?.Raise();
                    break;
                case "cancelChangeButton":
                    _publisher.Events.FirstOrDefault(x => x.EventName == component.Data.CustomId && x.MessageId == component.Message.Id)?.Raise();
                    break;
                case "confirmPage":
                    _publisher.Events.FirstOrDefault(x => x.EventName == component.Data.CustomId && x.MessageId == component.Message.Id)?.Raise();
                    break;
                default:
                    _publisher.Events.FirstOrDefault(x => x.EventName == component.Data.CustomId && x.MessageId == component.Message.Id)?.Raise();
                    break;
            }
            _ = Task.Run(() => DisposeOfEventAndMessage(component));

        }

        private async Task DisposeOfEventAndMessage(SocketMessageComponent component)
        {
            var events = _publisher.Events.Where(x => x.MessageId == component.Message.Id).ToList();
            var dispose = events.Any(x => x.DisposeMessage);
            foreach (var e in events)
            {
                e.ClearListeners();
                _publisher.Events.Remove(e);
            }
            if (dispose)
                await component.Message.DeleteAsync();
        }
    }
}
