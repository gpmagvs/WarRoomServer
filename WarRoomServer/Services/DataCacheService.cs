using AGVSystemCommonNet6.Equipment;
using Microsoft.Extensions.Caching.Memory;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;

namespace WarRoomServer.Services
{
    public class DataCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly List<FieldInfo> _fields;
        public DataCacheService(IMemoryCache memoryCache, WarRoomDbContext warRoomDbContext)
        {
            _memoryCache = memoryCache;
            _fields = warRoomDbContext.Fields.ToList();
        }

        internal void StoreAGVRealTimeData(FieldInfo field, List<AGVStatus> agvRTData)
        {
            _memoryCache.Set(field.ID, agvRTData);
        }

        internal List<FieldInfo> GetFields()
        {
            return _fields;
        }

        internal List<AGVStatus> GetAGVRealTimeDataOfField(int floor, string fieldName)
        {
            FieldInfo? _field = _fields.FirstOrDefault(field => field.Floor == floor && field.Name == fieldName);
            if (_field == null)
                return new List<AGVStatus>();
            return _memoryCache.Get<List<AGVStatus>>(_field.ID);
        }
        internal AGVStatus GetAGVRealTimeData(int floor, string fieldName, string eqName)
        {
            List<AGVStatus> agvDatas = GetAGVRealTimeDataOfField(floor, fieldName);
            var rtData = agvDatas.FirstOrDefault(agv => agv.Name == eqName);
            if (rtData == null)
                return new AGVStatus();
            return rtData;
        }
    }
}
