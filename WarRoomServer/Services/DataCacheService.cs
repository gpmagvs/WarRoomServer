using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Equipment;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;
using WarRoomServer.Hubs;
using WarRoomServer.View;

namespace WarRoomServer.Services
{
    public class DataCacheService
    {


        public enum AGV_DATA_USE_TO
        {
            /// <summary>
            /// 及時狀態數據
            /// </summary>
            STATUS_DATA,
            /// <summary>
            /// 稼動率
            /// </summary>
            UTIZATION_DATA
        }

        private readonly IMemoryCache _memoryCache;
        private readonly List<FieldInfo> _fields;
        private readonly WarRoomDbContext _warRoomDbContext;
        public DataCacheService(IMemoryCache memoryCache, WarRoomDbContext warRoomDbContext)
        {
            _memoryCache = memoryCache;
            _warRoomDbContext = warRoomDbContext;
            _fields = _warRoomDbContext.Fields.ToList();
        }

        internal void StoreAGVRealTimeData(FieldInfo field, List<AGVStatus> agvRTData)
        {
            _memoryCache.Set(field.ID, agvRTData);
        }


        internal void StoreAGVViewData(FieldInfo field, string agvName, AGVStatusViewData data)
        {
            string key = GenKeyOfAGV(field, agvName, AGV_DATA_USE_TO.STATUS_DATA);
            _memoryCache.Set(key, data);
        }


        internal List<FieldInfo> GetFields()
        {
            return _fields;
        }

        internal List<AGVStatus> GetAGVRealTimeDataOfField(int floor, string fieldName)
        {
            FieldInfo? _field = GetFieldInfo(floor, fieldName);
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

        /// <summary>
        /// 儲存AGV的即時嫁動率圖表數據
        /// </summary>
        /// <param name="field"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void StoreAGVUtilizationViewData(FieldInfo field, string name, AGVUtilizationViewData data)
        {
            string key = GenKeyOfAGV(field, name, AGV_DATA_USE_TO.UTIZATION_DATA);
            _memoryCache.Set(key, data);
        }

        internal AGVUtilizationViewData GetAGVUtilizationViewData(int floor, string fieldName, string agvName)
        {
            FieldInfo? _field = GetFieldInfo(floor, fieldName);
            if (_field == null)
                return new AGVUtilizationViewData();

            string key = GenKeyOfAGV(_field, agvName, AGV_DATA_USE_TO.UTIZATION_DATA);
            return _memoryCache.Get<AGVUtilizationViewData>(key);
        }

        internal AGVStatusViewData GetAGVData(int floor, string fieldName, string eqName)
        {
            FieldInfo? _field = GetFieldInfo(floor, fieldName);
            if (_field == null)
                return new AGVStatusViewData();
            string key = GenKeyOfAGV(_field, eqName, AGV_DATA_USE_TO.STATUS_DATA);
            return _memoryCache.Get<AGVStatusViewData>(key);
        }

        private string GenKeyOfAGV(FieldInfo field, string agvName, AGV_DATA_USE_TO useTO)
        {
            return $"{field.ID}-{agvName}-{useTO.ToString()}";
        }
        private FieldInfo GetFieldInfo(int floor, string fieldName)
        {
            return _fields.FirstOrDefault(field => field.Floor == floor && field.Name == fieldName);
        }
        internal async Task<List<FieldInfoView>> GetFieldsInfoOrUpdate()
        {
            string _key = "FieldsInfo";
            List<FieldInfoView> result = _memoryCache.Get<List<FieldInfoView>>(_key);
            if (result == null)
            {

                List<FieldInfo> _fieldInfos = await _warRoomDbContext.Fields.ToListAsync();

                List<Task<FieldInfoView>> tasks = new List<Task<FieldInfoView>>();

                foreach (var item in _fieldInfos)
                {
                    Task<FieldInfoView> collectInfoTask = Task.Run(async () =>
                    {
                        FieldInfoView fieldInfoView = new FieldInfoView()
                        {
                            Floor = item.Floor,
                            Name = item.Name,
                        };

                        List<EquipmentInfo> eqInfos = await _GetEquipmentsInfoFromFieldDatabase(item.DataBaseName);
                        fieldInfoView.Equipments = eqInfos;
                        return fieldInfoView;
                    });

                    tasks.Add(collectInfoTask);
                }
                Task.WaitAll(tasks.ToArray());
                result = tasks.Select(tk => tk.Result).ToList();
                _memoryCache.Set(_key, result);
            }
            return result;
        }



        private async Task<List<EquipmentInfo>> _GetEquipmentsInfoFromFieldDatabase(string dataBaseName)
        {
            List<EquipmentInfo> equipmentInfos = new List<EquipmentInfo>();

            try
            {

                AGVSDbContext dbContext = GetFieldAGVDbContext(dataBaseName);
                List<AGVStatus> _agvStates = await dbContext.EQStatus_AGV.ToListAsync();
                List<RackStatus> _rackStates = await dbContext.EQStatus_Rack.ToListAsync();
                List<MainEQStatus> _mainEqStates = await dbContext.EQStatus_MainEQ.ToListAsync();

                equipmentInfos.AddRange(_agvStates.Select(status => new EquipmentInfo
                {
                    EqType = EquipmentInfo.EQ_TYPE.AGV,
                    Name = status.Name
                }));
                equipmentInfos.AddRange(_rackStates.Select(status => new EquipmentInfo
                {
                    EqType = EquipmentInfo.EQ_TYPE.WIP,
                    Name = status.Name
                }));
                equipmentInfos.AddRange(_mainEqStates.Select(status => new EquipmentInfo
                {
                    EqType = EquipmentInfo.EQ_TYPE.MAIN_EQ,
                    Name = status.Name
                }));



            }
            catch (Exception ex)
            {
            }
            return equipmentInfos;
        }

        private AGVSDbContext GetFieldAGVDbContext(string dataBaseName)
        {
            string _connectstring = $"Server=localhost;Database={dataBaseName};User Id=sa;Password=12345678;";
            var optionsBuilder = new DbContextOptionsBuilder<AGVSDbContext>();
            optionsBuilder.UseSqlServer(_connectstring);
            AGVSDbContext _context = new AGVSDbContext(optionsBuilder.Options, true);
            return _context;
        }

    }
}
