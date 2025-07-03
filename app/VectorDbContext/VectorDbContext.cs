using app.Models.Embeddings;
using Microsoft.EntityFrameworkCore;

namespace app.VectorDbContext;

public class VectorDbContext(DbContextOptions<VectorDbContext> options) : DbContext(options)
{
    public DbSet<Embeddings> Embeddings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
    }
}