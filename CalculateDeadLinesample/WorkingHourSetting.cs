namespace CalculateDeadLinesample
{
    public class WorkingHourSetting 
    {
        public int Id { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeOnly WorkingHourStart { get; set; }
        public TimeOnly WorkingHourEnd { get; set; }

        public TimeOnly BreakHourStart { get; set; }
        public TimeOnly BreakHourEnd { get; set; }

        public bool IsOffWholeDay { get; set; }
    }
}
