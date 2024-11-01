
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using WarRoomServer.Data;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;
using WarRoomServer.View;

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
                await Task.Delay(1000, stoppingToken);
                Task.Run(async () => await GetRealTimeDataAndCache(field));
            }
        }

        private async Task GetRealTimeDataAndCache(FieldInfo field)
        {
            string DataBaseName = field.DataBaseName;
            string _connectstring = $"Server=localhost;Database={DataBaseName};User Id=sa;Password=12345678;";
            var optionsBuilder = new DbContextOptionsBuilder<AGVSDbContext>();
            optionsBuilder.UseSqlServer(_connectstring);
            AGVSDbContext _context = new AGVSDbContext(optionsBuilder.Options, true);
            AGVAvavity(field, _context);
            AGVRealTimeDataFetch(field, _context);



            //稼動率
            void AGVAvavity(FieldInfo field, AGVSDbContext _context)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {

                            List<AGVStatus> agvRTData = _context.EQStatus_AGV.AsNoTracking().ToList();
                            // Cache the real-time data
                            foreach (AGVStatus agvData in agvRTData)
                            {
                                string agvName = agvData.Name;
                                AGVUtilizationViewData data = await _context.GetAGVUtilizationDataToday(agvName);
                                memoryCacheService.StoreAGVUtilizationViewData(field, agvData.Name, data);

                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                });
            }


            //
            void AGVRealTimeDataFetch(FieldInfo field, AGVSDbContext _context)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {

                            List<AGVStatus> agvRTData = _context.EQStatus_AGV.AsNoTracking().ToList();
                            // Cache the real-time data
                            foreach (var agvData in agvRTData)
                            {
                                AGVStatusViewData data = await GenAGVStatusViewData(agvData);

                                data.UtilizationViewData = memoryCacheService.GetAGVUtilizationViewData(field.Floor, field.Name, agvData.Name);

                                memoryCacheService.StoreAGVViewData(field, agvData.Name, data);
                            }

                            memoryCacheService.StoreAGVRealTimeData(field, agvRTData);
                        }
                        catch (Exception ex)
                        {

                        }

                        await Task.Delay(1000);
                    }
                });
            }
            async Task<AGVStatusViewData> GenAGVStatusViewData(AGVStatus realTimeData)
            {
                AGVStatusViewData data = JsonConvert.DeserializeObject<AGVStatusViewData>(JsonConvert.SerializeObject(realTimeData));
                string agvName = realTimeData.Name;
                data.OnlineState = await _context.GetAGVOnlineState(agvName);
                data.DailyTaskStatistics = await _context.GetAGVDailyTaskExecuteStatistics(agvName, DateTime.Now);
                data.Alarm = await _context.GetAGVAlarmStatesStatistics(agvName, new DateTime());
                data.RunningTask = await _context.GetAGVRunningTaskState(agvName);

                return data;
            }



        }




    }
}
