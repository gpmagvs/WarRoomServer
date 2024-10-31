using Microsoft.AspNetCore.SignalR;

namespace WarRoomServer.Hubs
{
    public class TestHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // 获取查询参数
            var userId = httpContext.Request.Query["userId"].ToString();
            var authToken = httpContext.Request.Query["authToken"].ToString();

            // 处理查询参数
            Console.WriteLine($"User ID: {userId}, Auth Token: {authToken}");

            return base.OnConnectedAsync();
        }
        public async Task SendMessage(string user, string message)
        {
            if (message == "fetch-data")
            {
                //await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
            }
            //return Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
