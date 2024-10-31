using System.ComponentModel.DataAnnotations;

namespace WarRoomServer.Data.Entities
{
    public class FieldInfo
    {
        [Key]
        public string ID { get; set; }
        public int Floor { get; set; }
        public string Name { get; set; }
        public string DataBaseName { get; set; }

    }
}
