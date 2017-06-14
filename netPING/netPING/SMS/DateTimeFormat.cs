using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netPING
{
    public static class DateTimeFormat
    {
        public static string formatNormalString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
