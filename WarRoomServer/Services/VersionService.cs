using AGVSystemCommonNet6.DATABASE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using WarRoomServer.Data;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;
using WarRoomServer.View.Version;

namespace WarRoomServer.Services
{
    public class VersionService
    {
        private readonly WarRoomDbContext warRoomDb;
        private readonly IMemoryCache memoryCache;
        public VersionService(IMemoryCache memoryCache, WarRoomDbContext dbContext)
        {
            warRoomDb = dbContext;
            this.memoryCache = memoryCache;
        }

        internal async Task<List<AGVSFieldVersionInfo>> GetAllVersions()
        {
            string cacheKey = "FieldsVersion";

            if (memoryCache.TryGetValue<List<AGVSFieldVersionInfo>>(cacheKey, out List<AGVSFieldVersionInfo>? fieldVersionInfo))
            {
                return fieldVersionInfo;
            }

            Dictionary<FieldInfo, AGVSDbContext> fieldDbContexts = await warRoomDb.GetFieldsDbContext();
            List<Task<AGVSFieldVersionInfo>> tasks = new List<Task<AGVSFieldVersionInfo>>();

            foreach (var item in fieldDbContexts)
            {
                FieldInfo field = item.Key;
                AGVSDbContext agvsDb = item.Value;
                Task<AGVSFieldVersionInfo> task = GetFieldVersionInfo(field, agvsDb);
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            List<AGVSFieldVersionInfo> result = tasks.Select(tk => tk.Result).ToList();
            memoryCache.Set<List<AGVSFieldVersionInfo>>(cacheKey, result, TimeSpan.FromSeconds(30));
            return result;
        }


        private async Task<AGVSFieldVersionInfo> GetFieldVersionInfo(FieldInfo field, AGVSDbContext fieldDbContext)
        {
            using (fieldDbContext)
            {
                try
                {
                    AGVSystemCommonNet6.Sys.AGVSSystemStatus? sysStatus = await fieldDbContext.SysStatus.AsNoTracking().FirstOrDefaultAsync();
                    if (sysStatus == null)
                    {
                        return new AGVSFieldVersionInfo(field.Floor, field.Name, "1.0.0");
                    }
                    string fieldName = field.Name;
                    string fieldVersion = sysStatus.Version;
                    AGVSFieldVersionInfo fieldVersionInfo = new AGVSFieldVersionInfo(field.Floor, fieldName, fieldVersion);

                    //Get Vehicleversions 

                    List<VersionInfo> vehiclesVersions = fieldDbContext.AgvStates.AsNoTracking().Select(agv => new VersionInfo(agv.AGV_Name, agv.AppVersion)).ToList();
                    fieldVersionInfo.VehiclesVersions = vehiclesVersions;

                    return fieldVersionInfo;
                }
                catch (Exception)
                {
                    return new AGVSFieldVersionInfo(field.Floor, field.Name, "1.0.0");
                }
            }

        }

    }
}
