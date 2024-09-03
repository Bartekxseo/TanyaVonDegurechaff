using Discord;

namespace TD.Services.Registration
{
    public interface IRegistrationService
    {
        public void RegisterNodeManager(IUser manager, string node);
        public void RemoveNodeManager(IUser manager, string node);
        public void UpdateNodeManager(IUser manager, string node);
    }
}
