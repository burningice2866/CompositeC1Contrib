using System;

namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    /// <summary>
    /// An implementation of the Cron scheduler.
    /// The cron expression provides quite a bit of flexibility
    /// when you combine all five entries to create a cron expression. 
    /// For example:
    /// “0 * * * *” – Triggered at the top of every hour.
    /// “0 0 * * *” – Triggered at 12 AM every day.
    /// “0 0 1 * *” – Triggered at 12 AM the 1st of every month.
    /// “0 0 1 1 *” – Triggered at 12 AM the 1st of January of every year.
    /// “0 0 1 1 0” – Triggered at 12 AM the 1st of January when it falls on a Sunday.
    /// “*/30 * * * *” – Triggered at the top and bottom of the hour.
    /// “0 0 */2 * *” – Triggered every other day at 12 AM.
    /// “0 3 * * 6” – Triggered at 3 AM every Saturday.
    /// Each the cron entry can hold a string (no spaces allowed) of characters. 
    /// Some examples of valid entries are the following:
    /// “*” – Any valid value can cause a trigger. The validity of the values is checked to ensure that the resulting date / time exists. For example the day of the month is checked against the current month & year to ensure it is a valid day (including leap years).
    /// “3” – Only a single digit (in this case a three) can cause a trigger.
    /// “0,30” – A list of digits (in this case a zero and a thirty) can cause a trigger.
    /// “1-10” – A range of digits (in this case the numbers 1 through 10).
    /// “*/2” – Only even digits in the valid range.
    /// “0-30/10” – Only 0,10,20,30 will cause triggers.
    /// </summary>
    public class CronSchedule
    {
        private readonly MinutesCronSchedule _minutes;
        private readonly HoursCronSchedule _hours;
        private readonly DaysCronSchedule _days;
        private readonly MonthsCronSchedule _months;
        private readonly DaysOfWeekCronSchedule _daysOfWeek;

        public static CronSchedule Parse(string cronExpression)
        {
            if (string.IsNullOrEmpty(cronExpression))
            {
                throw new ArgumentNullException("cronExpression");
            }
            string[] parts = cronExpression.Split(' ');
            if (parts.Length != 5)
            {
                throw new ArgumentException("cronExpression parts", "cronExpression");
            }
            return Parse(parts[0], parts[1], parts[2], parts[3], parts[4]);
        }

        public static CronSchedule Parse(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            if (string.IsNullOrEmpty(minutes))
            {
                throw new ArgumentException("minutes");
            }
            if (string.IsNullOrEmpty(hours))
            {
                throw new ArgumentException("hours");
            }
            if (string.IsNullOrEmpty(days))
            {
                throw new ArgumentException("days");
            }
            if (string.IsNullOrEmpty(months))
            {
                throw new ArgumentException("months");
            }
            if (string.IsNullOrEmpty(daysOfWeek))
            {
                throw new ArgumentException("daysOfWeek");
            }
            return new CronSchedule(minutes, hours, days, months, daysOfWeek);
        }

        public int MinValue
        {
            get
            {
                if (_minutes.Values.Count > 1)
                {
                    return 60;
                }

                if (_hours.Values.Count > 1)
                {
                    return 60 * 60;
                }

                if (_days.Values.Count > 1)
                {
                    return 24 * 60 * 60;
                }

                return 28 * 86400;
            }
        }

        private CronSchedule(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            _minutes = new MinutesCronSchedule(minutes);
            _hours = new HoursCronSchedule(hours);
            _days = new DaysCronSchedule(days);
            _months = new MonthsCronSchedule(months);
            _daysOfWeek = new DaysOfWeekCronSchedule(daysOfWeek); // 0 = Sunday
        }

        public bool GetNext(DateTime start, out DateTime next)
        {
            return GetNext(start, DateTime.MaxValue, out next);
        }

        public bool GetNext(DateTime start, DateTime end, out DateTime next)
        {
            // Initialize the next output
            //
            next = DateTime.MinValue;

            // Don't want to select the actual start date.
            //
            DateTime baseSearch = start.AddMinutes(1.0);
            int baseMinute = baseSearch.Minute;
            int baseHour = baseSearch.Hour;
            int baseDay = baseSearch.Day;
            int baseMonth = baseSearch.Month;
            int baseYear = baseSearch.Year;

            // Get the next minute value
            //
            int minute = _minutes.Next(baseMinute);
            if (minute == CronScheduleBase.RolledOver)
            {
                // We need to roll forward to the next hour.
                //
                minute = _minutes.First;
                baseHour++;
                // Don't need to worry about baseHour>23 case because
                //	that will roll off our list in the next step.
            }

            // Get the next hour value
            //
            int hour = _hours.Next(baseHour);
            if (hour == CronScheduleBase.RolledOver)
            {
                // Roll forward to the next day.
                //
                //minute = _minutes.First;
                hour = _hours.First;
                baseDay++;
                // Don't need to worry about baseDay>31 case because
                //	that will roll off our list in the next step.
            }
            else if (hour > baseHour)
            {
                // Original hour must not have been in the list.
                //	Reset the minutes.
                //
                minute = _minutes.First;
            }

            // Get the next day value.
            //
            int day = _days.Next(baseDay);
            if (day == CronScheduleBase.RolledOver)
            {
                // Roll forward to the next month
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                baseMonth++;
                // Need to worry about rolling over to the next year here
                //	because we need to know the number of days in a month
                //	and that is year dependent (leap year).
                //
                if (baseMonth > 12)
                {
                    // Roll over to next year.
                    //
                    baseMonth = 1;
                    baseYear++;
                }
            }
            else if (day > baseDay)
            {
                // Original day no in the value list...reset.
                //
                minute = _minutes.First;
                hour = _hours.First;
            }
            while (day > DateTime.DaysInMonth(baseYear, baseMonth))
            {
                // Have a value for the day that is not a valid day
                //	in the current month. Move to the next month.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                baseMonth++;
                // This original month could not be December because
                //	it can handle the maximum value of days (31). So
                //	we do not have to worry about baseMonth == 13 case.
            }

            // Get the next month value.
            //
            int month = _months.Next(baseMonth);
            if (month == CronScheduleBase.RolledOver)
            {
                // Roll forward to the next year.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                month = _months.First;
                baseYear++;
            }
            else if (month > baseMonth)
            {
                // Original month not in the value list...reset.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
            }
            while (day > DateTime.DaysInMonth(baseYear, month))
            {
                // Have a value for the day that is not a valid day
                //	in the current month. Move to the next month.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                month = _months.Next(month + 1);
                if (month == CronScheduleBase.RolledOver)
                {
                    // Roll forward to the next year.
                    //
                    minute = _minutes.First;
                    hour = _hours.First;
                    day = _days.First;
                    month = _months.First;
                    baseYear++;
                }
            }

            // Is the date / time we found beyond the end search constraint?
            //
            DateTime suggested = new DateTime(baseYear, month, day, hour, minute, 0, 0);
            if (suggested >= end)
            {
                return false;
            }

            // Does the date / time we found satisfy the day of the week constraint?
            //
            if (_daysOfWeek.Values.Contains((int)suggested.DayOfWeek))
            {
                // We have a good date.
                //
                next = suggested;
                return true;
            }

            // We need to recursively look for a date in the future. Because this
            //	search resulted in a day that does not satisfy the day of week 
            //	constraint, start the search on the next day.
            //
            return GetNext(new DateTime(baseYear, month, day, 23, 59, 0, 0), out next);
        }

        public override string ToString()
        {
            return _minutes.Expression + " " +
                   _hours.Expression + " " +
                   _days.Expression + " " +
                   _months.Expression + " " +
                   _daysOfWeek.Expression;
        }

    }
}
