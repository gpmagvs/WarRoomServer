using AGVSystemCommonNet6.Equipment;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WarRoomServer.Services;

namespace WarRoomServer.Hubs
{
    public class EquipmentStatusHub : HubAbsract
    {
        DataCacheService _dataCacheService;
        public EquipmentStatusHub(DataCacheService dataCacheService) : base()
        {
            _dataCacheService = dataCacheService;
        }
        public override ConcurrentDictionary<string, ClientRequestState> clientStates { get; set; } = new ConcurrentDictionary<string, ClientRequestState>();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // 获取查询参数
            string floor = httpContext.Request.Query["floor"].ToString();
            string field = httpContext.Request.Query["field"].ToString();
            string equipmentName = httpContext.Request.Query["equipmentName"].ToString();
            int.TryParse(floor, out int _floor);
            EquipmentStatusHubClientRequest _clientState = new EquipmentStatusHubClientRequest()
            {
                connnectionID = Context.ConnectionId,
                cancelToskenSource = new CancellationTokenSource(),
                Floor = _floor,
                FieldName = field,
                EquipmentName = equipmentName
            };
            clientStates.TryAdd(Context.ConnectionId, _clientState);

            await Task.Delay(500);
            await base.OnConnectedAsync();
            await SendEquipmentData(_clientState);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        private async Task SendEquipmentData(EquipmentStatusHubClientRequest clientState)
        {
            while (!clientState.cancelToskenSource.IsCancellationRequested)
            {
                object _data = GetEquipmentData(clientState);
                await Clients.Client(clientState.connnectionID).SendAsync("EquipmentStatusData", _data);
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
            AGVStatus agvFakeData = _dataCacheService.GetAGVRealTimeData(clientState.Floor, clientState.FieldName, clientState.EquipmentName);
            return new
            {
                Floor = clientState.Floor,
                Field = clientState.FieldName,
                EquipmentName = clientState.EquipmentName,
                Data = agvFakeData
            };
        }

        public class EquipmentStatusHubClientRequest : ClientRequestState
        {
            public int Floor { get; set; } = 3;
            public string FieldName { get; set; } = "AOI";

            public string EquipmentName { get; set; } = "AGV_001";
        }

    }
}
