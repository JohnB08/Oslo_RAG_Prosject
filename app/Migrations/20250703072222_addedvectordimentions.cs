using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class addedvectordimentions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Embedding",
                table: "Embeddings",
                newName: "embedding");

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding",
                table: "Embeddings",
                type: "vector(384)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "embedding",
                table: "Embeddings",
                newName: "Embedding");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "Embeddings",
                type: "vector",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(384)",
                oldNullable: true);
        }
    }
}
