using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace app.Models.Embeddings;

public class Embeddings
{
    [Key]
    public int Id { get; set; }
    public string? Text { get; set; }
    [Column("embedding", TypeName = "vector(384)")]
    public Vector? Embedding { get; set; }
}