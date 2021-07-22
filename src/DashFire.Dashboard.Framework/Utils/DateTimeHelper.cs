using System;

namespace DashFire.Dashboard.Framework.Utils
{
    public static class DateTimeHelper
    {
        public static long ToLongDate(this DateTime dateTime)
        {
            return long.Parse(dateTime.ToString("yyyyMMdd"));
        }
    }
}
