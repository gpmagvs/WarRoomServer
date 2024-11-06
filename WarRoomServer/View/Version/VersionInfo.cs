namespace WarRoomServer.View.Version
{
    public class VersionInfo
    {
        public string Name { get; private set; } = "";
        public string Version { get; private set; } = "";

        public VersionInfo(string Name, string Version)
        {
            this.Name = Name;
            this.Version = Version;
        }

    }
}
