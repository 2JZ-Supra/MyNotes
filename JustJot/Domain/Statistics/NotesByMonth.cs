namespace Domain.Statistics
{
    public record class NotesByMonth
    {
        public required int Year { get; set; }
        public required int Month { get; set; }
        public required int Total { get; set; }

        public string GetMonthName()
        {
            return new DateTime(Year, Month, 1).ToString("MMMM");
        }
    }
}