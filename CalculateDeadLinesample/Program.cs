// See https://aka.ms/new-console-template for more information
using CalculateDeadLinesample;

List<WorkingHourSetting> workingHourSettings = new List<WorkingHourSetting>()
{
    new WorkingHourSetting { Day = DayOfWeek.Sunday },
    new WorkingHourSetting { Day = DayOfWeek.Monday, WorkingHourStart = 8, WorkingHourEnd = 18, BreakHourStart = 12, BreakHourEnd = 13 },
    new WorkingHourSetting { Day = DayOfWeek.Tuesday, WorkingHourStart = 8, WorkingHourEnd = 18, BreakHourStart = 12, BreakHourEnd = 13 },
    new WorkingHourSetting { Day = DayOfWeek.Wednesday , WorkingHourStart = 8, WorkingHourEnd = 18, BreakHourStart = 12, BreakHourEnd = 13},
    new WorkingHourSetting { Day = DayOfWeek.Thursday , WorkingHourStart = 8, WorkingHourEnd = 18, BreakHourStart = 12, BreakHourEnd = 13},
    new WorkingHourSetting { Day = DayOfWeek.Friday, WorkingHourStart = 8, WorkingHourEnd = 18, BreakHourStart = 12, BreakHourEnd = 13 },
    new WorkingHourSetting { Day = DayOfWeek.Saturday , WorkingHourStart = 8, WorkingHourEnd = 12}
};

List<HolidayDateSetting> holidayDateSettings = new List<HolidayDateSetting>()
{
    new HolidayDateSetting { Day = 30, Month = 4 },
    new HolidayDateSetting { Day = 1, Month = 5 },
    new HolidayDateSetting { Day = 24, Month = 7 },
    new HolidayDateSetting { Day = 25, Month = 7 },
    new HolidayDateSetting { Day = 26, Month = 7 }
};

//string startDateStr = "2023-07-23 09:00";
//int minutes = 5 * 60 + 30; 

// Endtime in break time => failed?

string _continue = "y";

while(_continue == "y")
{
    Console.WriteLine("Please enter the start date and time (yyyy-MM-dd HH:mm):");
    string startDateStr = Console.ReadLine();

    Console.WriteLine("Please enter the number of minutes needed to finish:");
    int minutes = int.Parse(Console.ReadLine());

    DateTime startDate;
    if (DateTime.TryParse(startDateStr, out startDate))
    {
        startDate = VerifyStartDate(startDate);
        Console.WriteLine("Start time at : " + startDate.ToString("yyyy-MM-dd HH:mm"));


        Console.WriteLine("Start processing....");
        DateTime endDate = CalculateEndTime(startDate, minutes);

        // Check if exist holiday in first period
        var totalHolidays = TotalHolidays(startDate, endDate);
        if(totalHolidays > 0)
        {
            endDate = endDate.AddDays(totalHolidays);
        }

        // extend if endDate is holiday
        while (IsHoliday(endDate))
        {
            endDate = endDate.AddDays(1);
        }

        Console.WriteLine("The end date and time is: " + endDate.ToString("yyyy-MM-dd HH:mm"));
    }
    else
    {
        Console.WriteLine("Invalid start date and time format.");
    }

    Console.WriteLine("Do you want to continue? (y / n)");
    _continue = Console.ReadLine();
}

return;


#region calculate next endtime

int TotalHolidays(DateTime startTime, DateTime endTime)
{
    var days = 0;
    while(startTime <= endTime)
    {
        days += IsHoliday(startTime) ? 1 : 0;
        startTime = startTime.AddDays(1);
    }

    return days;
}

/// <summary>
/// Calculate next endtime base on starttime, minutes, woringday, holiday and weekend
/// </summary>
/// <param name="startDate"></param>
/// <param name="minutes"></param>
/// <returns></returns>
DateTime CalculateEndTime(DateTime startDate, int minutes)
{
    DateTime endDate = startDate;

    while (minutes > 0)
    {
        // check if startDate in not in working hour, working day
        var dayOfWeekSetting = workingHourSettings.FirstOrDefault(r => r.Day == endDate.DayOfWeek);

        // Set the start and end hours and the lunch break for the current working day
        DateTime startWorkingHour = CombineWithHour(endDate, dayOfWeekSetting.WorkingHourStart);
        DateTime endWorkingHour = CombineWithHour(endDate, dayOfWeekSetting.WorkingHourEnd);
        DateTime breakStart = CombineWithHour(endDate, dayOfWeekSetting.BreakHourStart);
        DateTime breakEnd = CombineWithHour(endDate, dayOfWeekSetting.BreakHourEnd);
        double hoursBreak = 0;// dayOfWeekSetting.BreakHourEnd - dayOfWeekSetting.BreakHourStart;

        if (startDate <= breakStart && endDate.AddMinutes(minutes) >= breakEnd)
            hoursBreak = dayOfWeekSetting.BreakHourEnd - dayOfWeekSetting.BreakHourStart;


        // Check if the current day is a working day (Monday to Friday)
        if (endDate.DayOfWeek != DayOfWeek.Saturday && endDate.DayOfWeek != DayOfWeek.Sunday)
        {
            // Calculate the remaining minutes for the current working day
            TimeSpan remainingWorkingMinutes = endWorkingHour - endDate;
            if (startDate <= breakStart && endDate.AddMinutes(minutes) >= breakEnd)
                remainingWorkingMinutes = remainingWorkingMinutes - (breakEnd - breakStart);

            if (remainingWorkingMinutes.TotalMinutes >= minutes)
            {
                // If the remaining minutes are enough to finish, add the minutes and return
                endDate = endDate.AddMinutes(minutes);
                
                endDate = endDate.AddHours(hoursBreak);

                minutes = 0;
            }
            else
            {
                // If the remaining minutes are not enough, calculate the next working day
                minutes -= (int)remainingWorkingMinutes.TotalMinutes;
                endDate = endDate.AddDays(1).Date;

                // Set the start hour of the next working day
                endDate = endDate.AddHours(CombineWithHour(endDate, dayOfWeekSetting.WorkingHourStart).Hour);
            }
        }
        else if (endDate.DayOfWeek == DayOfWeek.Saturday)
        {
            // Calculate the remaining minutes for Saturday
            TimeSpan remainingWorkingMinutes = endWorkingHour - endDate;
            if (startDate <= breakStart && endDate.AddMinutes(minutes) >= breakEnd)
                remainingWorkingMinutes = remainingWorkingMinutes - (breakEnd - breakStart);

            if (remainingWorkingMinutes.TotalMinutes >= minutes)
            {
                // If the remaining minutes are enough to finish on Saturday, add the minutes and return
                endDate = endDate.AddMinutes(minutes);
                // if (endDate > breakStart)
                
                endDate = endDate.AddHours(hoursBreak);

                minutes = 0;
            }
            else
            {
                // If the remaining minutes are not enough, calculate the next working day
                minutes -= (int)remainingWorkingMinutes.TotalMinutes;
                endDate = endDate.AddDays(2).Date;

                // Set the start hour of the next working day
                endDate = endDate.AddHours(CombineWithHour(endDate, dayOfWeekSetting.WorkingHourStart).Hour);
            }
        }
        else
        {
            // If it's a Sunday, move to the next Monday and set the start hour to 8:00 AM
            endDate = endDate.AddDays(8 - (int)endDate.DayOfWeek).Date.AddHours(dayOfWeekSetting.WorkingHourStart);
        }
    }

    return endDate;
}

DateTime VerifyStartDate(DateTime startDate)
{    
    var dayOfWeekSetting = workingHourSettings.FirstOrDefault(r => r.Day == startDate.DayOfWeek);

    // Set the start and end hours and the lunch break for the current working day
    DateTime startWorkingDay = CombineWithHour(startDate, dayOfWeekSetting.WorkingHourStart);
    DateTime endWorkingDay = CombineWithHour(startDate, dayOfWeekSetting.WorkingHourEnd);
    DateTime breakStart = CombineWithHour(startDate, dayOfWeekSetting.BreakHourStart);
    DateTime breakEnd = CombineWithHour(startDate, dayOfWeekSetting.BreakHourEnd);

    // if it is during break time
    if(startDate > breakStart && startDate < breakEnd) { return breakEnd; }

    // if it is before start working day
    if (startDate < startWorkingDay) { return startWorkingDay; }

    // if it is after working day
    if (startDate > endWorkingDay) 
    {
        var nextStartDate = startDate;
        // get start date for the next working day
        do
        {
            nextStartDate = nextStartDate.AddDays(1).Date;
        }
        while (nextStartDate.DayOfWeek == DayOfWeek.Sunday);

        dayOfWeekSetting = workingHourSettings.FirstOrDefault(r => r.Day == nextStartDate.DayOfWeek);
        startWorkingDay = CombineWithHour(nextStartDate, dayOfWeekSetting.WorkingHourStart);

        return startWorkingDay;
    }

    return startDate;
}

DateTime CombineWithHour(DateTime date, double hours)
{
    return date.Date.AddHours(hours);
}

bool IsHoliday(DateTime date)
{
    return holidayDateSettings.Any(r => r.Day == date.Day && r.Month == date.Month);
}

#endregion