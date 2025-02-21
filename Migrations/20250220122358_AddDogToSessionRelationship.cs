using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class AddDogToSessionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_DogId",
                table: "Sessions",
                column: "DogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Dogs_DogId",
                table: "Sessions",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Dogs_DogId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_DogId",
                table: "Sessions");
        }
    }
}
