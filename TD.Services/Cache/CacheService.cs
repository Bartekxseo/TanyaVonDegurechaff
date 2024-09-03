using AutoMapper;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TD.DataAccess;
using TD.Domain.Entities;
using Serilog;

namespace TD.Services.Cache
{
    public class CacheService
    {
        public List<Prefix> prefixes = new List<Prefix>();
        public string AppPath = string.Empty;
        public List<RolePermission> permissions = new List<RolePermission>();
        private readonly TDDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly DiscordSocketClient _socketClient;

        public bool IsInitialised = false;
        public CacheService(TDDbContext dbContext, IMapper mapper, DiscordSocketClient socketClient)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _socketClient = socketClient;
        }
        public async Task RecreateEntities()
        {
        }
    }
}
