using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NoseWorks.Migrations
{
    /// <inheritdoc />
    public partial class RenameSendToTrialAndUpdateAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sends");

            migrationBuilder.RenameColumn(
                name: "SendNumber",
                table: "TrainingPrograms",
                newName: "TrialNumber");

            migrationBuilder.RenameColumn(
                name: "SendX",
                table: "Sessions",
                newName: "TrialX");

            migrationBuilder.RenameColumn(
                name: "NumberOfSends",
                table: "Sessions",
                newName: "NumberOfTrials");

            migrationBuilder.CreateTable(
                name: "Trials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    SelectedLocation = table.Column<int>(type: "integer", nullable: false),
                    TargetScent = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trials", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trials");

            migrationBuilder.RenameColumn(
                name: "TrialNumber",
                table: "TrainingPrograms",
                newName: "SendNumber");

            migrationBuilder.RenameColumn(
                name: "TrialX",
                table: "Sessions",
                newName: "SendX");

            migrationBuilder.RenameColumn(
                name: "NumberOfTrials",
                table: "Sessions",
                newName: "NumberOfSends");

            migrationBuilder.CreateTable(
                name: "Sends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Result = table.Column<string>(type: "text", nullable: false),
                    SelectedLocation = table.Column<int>(type: "integer", nullable: false),
                    TargetScent = table.Column<string>(type: "text", nullable: false),
                    TrainingId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sends", x => x.Id);
                });
        }
    }
}
