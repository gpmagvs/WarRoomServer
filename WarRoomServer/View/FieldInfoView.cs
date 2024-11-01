namespace WarRoomServer.View
{

    public class FieldInfoView
    {
        public int Floor { get; set; } = 0;
        public string Name { get; set; } = "";
        public List<EquipmentInfo> Equipments { get; set; } = new List<EquipmentInfo>();
    }

    public class EquipmentInfo
    {
        public enum EQ_TYPE
        {
            AGV, CHARGE_STATION, WIP, MAIN_EQ
        }
        public string Name { get; set; } = "";

        public EQ_TYPE EqType { get; set; } = EQ_TYPE.MAIN_EQ;

    }
}
