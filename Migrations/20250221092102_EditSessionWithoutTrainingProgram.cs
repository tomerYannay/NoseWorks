using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class EditSessionWithoutTrainingProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId1",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrainingProgramId1",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TrainingProgramId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TrainingProgramId1",
                table: "Sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainingProgramId",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrainingProgramId1",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrainingProgramId1",
                table: "Sessions",
                column: "TrainingProgramId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId1",
                table: "Sessions",
                column: "TrainingProgramId1",
                principalTable: "TrainingPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
