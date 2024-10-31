
using WarRoomServer.Data.Contexts;

namespace WarRoomServer.Services
{
    public class DataBaseMigrateService : IHostedService
    {
        WarRoomDbContext dbContext;
        public DataBaseMigrateService(IServiceScopeFactory factory)
        {
            this.dbContext = factory.CreateScope().ServiceProvider.GetRequiredService<WarRoomDbContext>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            dbContext.Database.EnsureCreated();
            await dbContext.SaveChangesAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {


        }
    }
}
