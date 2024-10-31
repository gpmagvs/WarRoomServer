namespace WarRoomServer.Hubs
{
    public abstract class ClientRequestState
    {
        public string connnectionID { get; set; } = "";
        public CancellationTokenSource cancelToskenSource { get; set; } = new CancellationTokenSource();

    }
}
