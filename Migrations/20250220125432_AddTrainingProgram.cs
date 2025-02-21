using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainingPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SendNumber = table.Column<int>(type: "integer", nullable: false),
                    PositiveLocation = table.Column<int>(type: "integer", nullable: false),
                    NegativeLocation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPrograms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrainingProgramId",
                table: "Sessions",
                column: "TrainingProgramId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId",
                table: "Sessions",
                column: "TrainingProgramId",
                principalTable: "TrainingPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "TrainingPrograms");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrainingProgramId",
                table: "Sessions");
        }
    }
}
