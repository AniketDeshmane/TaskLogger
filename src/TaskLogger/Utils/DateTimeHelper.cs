using System;

namespace TaskLogger.Utils
{
    public static class DateTimeHelper
    {
        public static bool IsWeekend()
        {
            var today = DateTime.Today.DayOfWeek;
            return today == DayOfWeek.Saturday || today == DayOfWeek.Sunday;
        }

        public static string GetFormattedDate()
        {
            return DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        public static string GetFormattedTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
