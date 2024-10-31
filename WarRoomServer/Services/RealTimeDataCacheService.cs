
using AGVSystemCommonNet6.DATABASE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;

namespace WarRoomServer.Services
{
    public class RealTimeDataCacheService : BackgroundService
    {
        WarRoomDbContext dbContext;
        DataCacheService memoryCacheService;
        public RealTimeDataCacheService(IServiceScopeFactory factory)
        {
            dbContext = factory.CreateScope().ServiceProvider.GetRequiredService<WarRoomDbContext>();
            memoryCacheService = factory.CreateScope().ServiceProvider.GetRequiredService<DataCacheService>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Data.Entities.FieldInfo> fields = dbContext.Fields.ToList();
            foreach (Data.Entities.FieldInfo field in fields)
            {
                Task.Run(async () => await GetRealTimeDataAndCache(field));
            }
            await Task.Delay(1000, stoppingToken);
        }

        private async Task GetRealTimeDataAndCache(FieldInfo field)
        {
            string DataBaseName = field.DataBaseName;
            string _connectstring = $"Server=localhost;Database={DataBaseName};User Id=sa;Password=12345678;";
            var optionsBuilder = new DbContextOptionsBuilder<AGVSDbContext>();
            optionsBuilder.UseSqlServer(_connectstring);
            AGVSDbContext _context = new AGVSDbContext(optionsBuilder.Options, true);

            while (true)
            {
                try
                {

                    List<AGVSystemCommonNet6.Equipment.AGVStatus> agvRTData = _context.EQStatus_AGV.ToList();

                    // Cache the real-time data
                    memoryCacheService.StoreAGVRealTimeData(field, agvRTData);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                await Task.Delay(1000);
            }
        }
    }
}
