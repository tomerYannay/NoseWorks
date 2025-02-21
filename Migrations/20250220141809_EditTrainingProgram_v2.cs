using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class EditTrainingProgram_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Dogs_DogId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_DogId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrainingProgramId",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "TrainingPrograms",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId1",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrainingProgramId1",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "TrainingPrograms");

            migrationBuilder.DropColumn(
                name: "TrainingProgramId1",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_DogId",
                table: "Sessions",
                column: "DogId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrainingProgramId",
                table: "Sessions",
                column: "TrainingProgramId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Dogs_DogId",
                table: "Sessions",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_TrainingPrograms_TrainingProgramId",
                table: "Sessions",
                column: "TrainingProgramId",
                principalTable: "TrainingPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
