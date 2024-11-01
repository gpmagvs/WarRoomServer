using AGVSystemCommonNet6.Equipment;
using EquipmentManagment.MainEquipment;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WarRoomServer.Data.Entities;
using WarRoomServer.Services;

namespace WarRoomServer.Hubs
{
    public class EquipmentStatusHub : Hub
    {
        DataCacheService _dataCacheService;
        public EquipmentStatusHub(DataCacheService dataCacheService) : base()
        {
            _dataCacheService = dataCacheService;
        }


        public static ConcurrentDictionary<string, EquipmentStatusHubClientRequest> clientStates { get; set; } = new ConcurrentDictionary<string, EquipmentStatusHubClientRequest>();



        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // 获取查询参数
            string floor = httpContext.Request.Query["floor"].ToString();
            string field = httpContext.Request.Query["field"].ToString();
            string equipmentName = httpContext.Request.Query["equipmentName"].ToString();
            string equipmentType = httpContext.Request.Query["eqType"].ToString();
            int.TryParse(floor, out int _floor);
            int.TryParse(equipmentType, out int _eqType);

            IClientProxy _client = Clients.Client(Context.ConnectionId);

            EquipmentStatusHubClientRequest _clientState = new EquipmentStatusHubClientRequest()
            {
                connnectionID = Context.ConnectionId,
                cancelToskenSource = new CancellationTokenSource(),
                Floor = _floor,
                FieldName = field,
                EquipmentName = equipmentName,
                EqType = Enum.GetValues<View.EquipmentInfo.EQ_TYPE>().Cast<View.EquipmentInfo.EQ_TYPE>().FirstOrDefault(v => ((int)v) == _eqType),
                client = _client
            };
            clientStates.TryAdd(Context.ConnectionId, _clientState);

            //await Task.Delay(500);
            _ = SendEquipmentData(_clientState);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (clientStates.TryRemove(Context.ConnectionId, out var _state))
                _state.cancelToskenSource.Cancel();
            return base.OnDisconnectedAsync(exception);
        }

        private async Task SendEquipmentData(EquipmentStatusHubClientRequest clientState)
        {
            while (!clientState.cancelToskenSource.IsCancellationRequested)
            {
                object _data = GetEquipmentData(clientState);
                await clientState.client.SendAsync("EquipmentStatusData", _data);
                try
                {
                    await Task.Delay(1000, clientState.cancelToskenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private object GetEquipmentData(EquipmentStatusHubClientRequest clientState)
        {
            object Data = new object();
            if (clientState.EqType == View.EquipmentInfo.EQ_TYPE.AGV)
            {
                Data = _dataCacheService.GetAGVData(clientState.Floor, clientState.FieldName, clientState.EquipmentName);
            }

            return new
            {
                Floor = clientState.Floor,
                Field = clientState.FieldName,
                EquipmentName = clientState.EquipmentName,
                Data = Data
            };
        }

        public class EquipmentStatusHubClientRequest : ClientRequestState
        {

            public int Floor { get; set; } = 3;
            public string FieldName { get; set; } = "AOI";

            public string EquipmentName { get; set; } = "AGV_001";

            public WarRoomServer.View.EquipmentInfo.EQ_TYPE EqType { get; set; } = View.EquipmentInfo.EQ_TYPE.AGV;
        }

    }
}
