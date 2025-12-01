using Domain;
using Microsoft.EntityFrameworkCore;

namespace NotesComeback.Data.SqlServer
{
    public class NotesDbContext : DbContext
    {
        public NotesDbContext(DbContextOptions<NotesDbContext> options)
            : base(options) { }

        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many Note ↔ Category
            modelBuilder.Entity<Note>()
                .HasMany(n => n.Categories)
                .WithMany();
        }
    }
}
