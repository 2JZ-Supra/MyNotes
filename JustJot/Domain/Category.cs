namespace Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public virtual List<Note> Notes { get; set; } = new List<Note>();

        public Category() { }
    }
}