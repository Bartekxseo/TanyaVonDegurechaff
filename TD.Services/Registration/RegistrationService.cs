using Discord;
using TD.DataAccess;

namespace TD.Services.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly TDDbContext _dbContext;
        public RegistrationService(TDDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void RegisterNodeManager(IUser manager, string node)
        {

        }

        public void RemoveNodeManager(IUser manager, string node)
        {
            throw new NotImplementedException();
        }

        public void UpdateNodeManager(IUser manager, string node)
        {
            throw new NotImplementedException();
        }
    }
}
