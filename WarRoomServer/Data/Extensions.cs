using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WarRoomServer.View;
using static WarRoomServer.View.AlarmStates;

namespace WarRoomServer.Data
{
    public static class Extensions
    {
        public static async Task<string> GetAGVOnlineState(this AGVSDbContext context, string AGVName)
        {
            AGVSystemCommonNet6.clsAGVStateDto? agvState = context.AgvStates.AsNoTracking().FirstOrDefault(a => a.AGV_Name == AGVName);
            if (agvState == null)
                return "OFFLINE";
            return agvState.OnlineStatus.ToString();
        }

        /// <summary>
        /// 取得AGV在指定的日期之任務執行狀態
        /// </summary>
        /// <param name="context"></param>
        /// <param name="AGVName"></param>
        /// <returns></returns>
        public static async Task<DailyTaskExecuteStatistics> GetAGVDailyTaskExecuteStatistics(this AGVSDbContext context, string AGVName, DateTime date)
        {
            DateTime fromTime = new DateTime(date.Year, date.Month, date.Day);
            DateTime toTime = fromTime.AddDays(1);

            List<AGVSystemCommonNet6.AGVDispatch.clsTaskDto> tasks = await context.Tasks.AsNoTracking()
                                                                                        .Where(tk => tk.RecieveTime >= fromTime && tk.RecieveTime <= toTime)
                                                                                        .Where(tk => tk.DesignatedAGVName == AGVName)
                                                                                        .ToListAsync();

            var carryAndChargeTasks = tasks.Where(tk => tk.Action == ACTION_TYPE.Carry || tk.Action == ACTION_TYPE.Charge);
            int totalTaskCount = carryAndChargeTasks.Count();
            int totalTransferCount = carryAndChargeTasks.Count(tk => tk.Action == ACTION_TYPE.Carry);
            int totalChargeTaskCount = carryAndChargeTasks.Count(tk => tk.Action == ACTION_TYPE.Charge);
            int totalSuccessCount = carryAndChargeTasks.Count(tk => tk.State == TASK_RUN_STATUS.ACTION_FINISH || tk.State == TASK_RUN_STATUS.NAVIGATING || tk.State == TASK_RUN_STATUS.WAIT);
            int totalFailureCount = carryAndChargeTasks.Count(tk => tk.State == TASK_RUN_STATUS.CANCEL || tk.State == TASK_RUN_STATUS.FAILURE);

            return new DailyTaskExecuteStatistics()
            {
                TotalTaskNum = totalTaskCount,
                TransferTaskNum = totalTransferCount,
                ChargeTaskNum = totalChargeTaskCount,
                TotalFailureNum = totalFailureCount,
                TotalSuccessNum = totalSuccessCount,
            };
        }

        /// <summary>
        /// 取得AGV在指定的日期之任務執行狀態
        /// </summary>
        /// <param name="context"></param>
        /// <param name="AGVName"></param>
        /// <returns></returns>
        public static async Task<AlarmStates> GetAGVAlarmStatesStatistics(this AGVSDbContext context, string AGVName, DateTime date)
        {
            DateTime fromTime = new DateTime(date.Year, date.Month, date.Day);
            DateTime toTime = fromTime.AddDays(1);

            List<AGVSystemCommonNet6.Alarm.clsAlarmDto> alarms = await context.SystemAlarms.AsNoTracking()
                                                                                           .OrderByDescending(al => al.Time)
                                                                                           .Where(alarm_ => alarm_.Equipment_Name == AGVName)
                                                                                           .Take(30).ToListAsync();

            List<AlarmStates.Alarm> currentALarms = alarms.Select(al => new AlarmStates.Alarm
            {
                Time = al.Time,
                AlarmCode = al.AlarmCode,
                AlarmMessage = al.Description,
                AlarmType = al.Level.ToString()
            }).ToList();

            return new AlarmStates()
            {
                CurrentAlarms = currentALarms
            };
        }

        public static async Task<RunningTaskState> GetAGVRunningTaskState(this AGVSDbContext context, string AGVName)
        {
            var state = new RunningTaskState()
            {
                TaskID = "-",
                FromStationName = "-",
                TaskType = "-",
                ToStationName = "-"
            };

            AGVSystemCommonNet6.clsAGVStateDto? agvState = context.AgvStates.AsNoTracking().FirstOrDefault(a => a.AGV_Name == AGVName);
            if (agvState == null)
                return state;

            AGVSystemCommonNet6.AGVDispatch.clsTaskDto? orderr = context.Tasks.FirstOrDefault(tk => tk.TaskName == agvState.TaskName);

            if (orderr != null)
            {
                state.TaskID = orderr.TaskName;
                state.TaskType = orderr.ActionName;
                state.FromStationName = orderr.From_Station;
                state.ToStationName = orderr.To_Station;
            }

            return state;
        }


        public static async Task<AGVUtilizationViewData> GetAGVUtilizationDataToday(this AGVSDbContext dbContext, string agvName)
        {

            DateTime fromTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime toTime = fromTime.AddDays(1);

            List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> datas = await dbContext.RealTimeAvailabilitys.AsNoTracking()
                                                                                                                    .Where(data => data.StartTime >= fromTime && data.StartTime <= toTime)
                                                                                                                    .Where(data => data.AGVName == agvName)
                                                                                                                    .ToListAsync();
            IEnumerable<object[]> chartDatas = datas.Select(data => new object[4] { data.StartTime.getTime(), data.EndTime.getTime(), 0, data.Main_Status.GetColorOfAGVSTatus() });
            AGVUtilizationViewData viewData = new AGVUtilizationViewData
            {
                data = chartDatas.ToList()
            };
            return viewData;
        }


        /// <summary>
        /// Returns the stored time value in milliseconds since midnight, January 1, 1970 UTC.
        /// </summary>
        /// <returns></returns>
        private static long getTime(this DateTime time)
        {
            long milliseconds = (long)(time.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return milliseconds;
        }

        private static string GetColorOfAGVSTatus(this AGVSystemCommonNet6.clsEnums.MAIN_STATUS status)
        {
            string color = "grey";
            switch (status)
            {
                case AGVSystemCommonNet6.clsEnums.MAIN_STATUS.IDLE:
                    color = "orange";
                    break;
                case AGVSystemCommonNet6.clsEnums.MAIN_STATUS.RUN:
                    color = "lime";
                    break;
                case AGVSystemCommonNet6.clsEnums.MAIN_STATUS.DOWN:
                    color = "red";
                    break;
                case AGVSystemCommonNet6.clsEnums.MAIN_STATUS.Charging:
                    color = "blue";
                    break;
                case AGVSystemCommonNet6.clsEnums.MAIN_STATUS.Unknown:
                    break;
                default:
                    break;
            }
            return color;
        }
    }
}
