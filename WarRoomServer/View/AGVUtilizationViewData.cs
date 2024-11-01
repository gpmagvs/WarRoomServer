namespace WarRoomServer.View
{
    public class AGVUtilizationViewData
    {
        //frontend need:
        //
        // data: [
        //          [new Date('2023-10-01 09:00').getTime(), new Date('2023-10-01 09:10').getTime(), 0, 'orange'],
        //          [new Date('2023-10-01 09:10').getTime(), new Date('2023-10-01 09:20').getTime(), 0, 'red'],
        //          [new Date('2023-10-01 09:30').getTime(), new Date('2023-10-01 11:20').getTime(), 0, 'lime'],
        //          [new Date('2023-10-01 13:10').getTime(), new Date('2023-10-01 20:00').getTime(), 0, 'blue'],
        //          [new Date('2023-10-01 22:10').getTime(), new Date('2023-10-01 23:50').getTime(), 0, 'red'],
        // ]
        public List<object[]> data { get; set; } = new List<object[]>();

    }
}
