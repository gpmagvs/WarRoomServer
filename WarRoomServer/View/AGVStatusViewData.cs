using AGVSystemCommonNet6.Equipment;

namespace WarRoomServer.View
{
    public class AGVStatusViewData : AGVStatus
    {
        public string OnlineState { get; set; } = "OFFLINE";
        public RunningTaskState RunningTask { get; set; } = new RunningTaskState();
        public DailyTaskExecuteStatistics DailyTaskStatistics { get; set; } = new DailyTaskExecuteStatistics();
        public AlarmStates Alarm { get; set; } = new AlarmStates();
        public AGVUtilizationViewData UtilizationViewData { get; set; } = new AGVUtilizationViewData();
    }

    public class RunningTaskState
    {
        public string TaskID { get; set; } = "AA";
        public string TaskType { get; set; } = "搬運";
        public string FromStationName { get; set; } = "FA";
        public string ToStationName { get; set; } = "TTT";
    }

    public class DailyTaskExecuteStatistics
    {
        public int TotalTaskNum { get; set; } = 0;
        public int TransferTaskNum { get; set; } = 0;
        public int ChargeTaskNum { get; set; } = 0;
        public int TotalSuccessNum { get; set; } = 0;
        public int TotalFailureNum { get; set; } = 0;
        public double SuccessRate => TotalTaskNum == 0 ? 0 : Math.Round((double)TotalSuccessNum / TotalTaskNum * 100.0, 1);
    }

    public class AlarmStates
    {
        public List<Alarm> CurrentAlarms { get; set; } = new List<Alarm>();

        public class Alarm
        {
            public DateTime Time { get; set; } = DateTime.MinValue;
            public int AlarmCode { get; set; } = 0;
            public string AlarmType { get; set; } = "";

            public string AlarmMessage { get; set; } = "";
        }

    }

}
