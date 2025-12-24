namespace Domain
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public virtual List<Category> Categories { get; set; } = new List<Category>();
        public DateTime CreatedAt { get; private set; }

        public Note()
        {
            CreatedAt = DateTime.Now;
        }

        public string CategoriesString =>
    Categories != null ? string.Join(", ", Categories.Select(c => c.Name)) : "Без категории";

    }
}