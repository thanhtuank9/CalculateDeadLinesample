namespace CalculateDeadLinesample
{
    public class WorkingHourSetting 
    {
        public int Id { get; set; }

        public DayOfWeek Day { get; set; }

        public double WorkingHourStart { get; set; }
        public double WorkingHourEnd { get; set; }

        public double BreakHourStart { get; set; }
        public double BreakHourEnd { get; set; }

        public bool IsOffWholeDay { get; set; }
    }
}
