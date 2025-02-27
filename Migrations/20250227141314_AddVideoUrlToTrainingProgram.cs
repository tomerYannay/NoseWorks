using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoUrlToTrainingProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "TrainingPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "TrainingPrograms");
        }
    }
}
