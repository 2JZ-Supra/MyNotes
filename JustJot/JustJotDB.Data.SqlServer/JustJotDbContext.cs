using Domain;
using Microsoft.EntityFrameworkCore;

namespace JustJotDB.Data.SqlServer
{
    public class JustJotDbContext : DbContext
    {
        public JustJotDbContext(DbContextOptions<JustJotDbContext> options) : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .HasMany(n => n.Categories)
                .WithMany(c => c.Notes)
                .UsingEntity<Dictionary<string, object>>(
                    "NoteCategory",
                    j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                    j => j.HasOne<Note>().WithMany().HasForeignKey("NoteId"),
                    j =>
                    {
                        j.ToTable("NoteCategories");
                        j.HasKey("NoteId", "CategoryId");
                    });
        }
    }
}