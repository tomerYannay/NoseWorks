using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoUrlToTrial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "TrainingPrograms");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Trials",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Trials");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "TrainingPrograms",
                type: "text",
                nullable: true);
        }
    }
}
