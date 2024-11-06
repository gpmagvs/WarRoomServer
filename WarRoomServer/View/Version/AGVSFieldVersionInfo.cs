namespace WarRoomServer.View.Version
{
    public class AGVSFieldVersionInfo : VersionInfo
    {
        public int floor { get; private set; } = 0;
        public AGVSFieldVersionInfo(int floor, string Name, string Version) : base(Name, Version)
        {
            this.floor = floor;
        }

        public List<VersionInfo> VehiclesVersions { get; set; } = new List<VersionInfo>();
    }
}
