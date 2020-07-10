using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Ferretto.VW.CommonUtils
{
    public static class DateTimeExtensions
    {
        #region Methods

        public static void SetAsUtcSystemTime(this DateTimeOffset dateTime)
        {
            var time = new SYSTEMTIME(dateTime);
            var success = SetSystemTime(ref time);

            if (!success)
            {
                throw new Win32Exception((int)GetLastError());
            }
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        private static extern bool SetSystemTime(ref SYSTEMTIME time);

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort Year;

            public ushort Month;

            public ushort DayOfWeek;

            public ushort Day;

            public ushort Hour;

            public ushort Minute;

            public ushort Second;

            public ushort Milliseconds;

            public SYSTEMTIME(DateTimeOffset dateTimeOffset)
            {
                var dateTime = dateTimeOffset.ToUniversalTime();
                this.Year = (ushort)dateTime.Year;
                this.Month = (ushort)dateTime.Month;
                this.DayOfWeek = (ushort)dateTime.DayOfWeek;
                this.Day = (ushort)dateTime.Day;
                this.Hour = (ushort)dateTime.Hour;
                this.Minute = (ushort)dateTime.Minute;
                this.Second = (ushort)dateTime.Second;
                this.Milliseconds = (ushort)dateTime.Millisecond;
            }
        }

        #endregion
    }
}
