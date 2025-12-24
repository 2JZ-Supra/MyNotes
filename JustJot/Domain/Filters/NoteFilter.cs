namespace Domain.Filters
{
    public record NoteFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsFavorite { get; set; }
    }
}
