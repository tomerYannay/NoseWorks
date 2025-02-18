using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyFirstMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameTrainingToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DogId = table.Column<int>(type: "integer", nullable: false),
                    Trainer = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumberOfSends = table.Column<int>(type: "integer", nullable: false),
                    ContainerType = table.Column<string>(type: "text", nullable: false),
                    SendX = table.Column<bool>(type: "boolean", nullable: false),
                    TrainingProgramId = table.Column<int>(type: "integer", nullable: false),
                    FinalResults = table.Column<List<string>>(type: "text[]", nullable: false),
                    DPrimeScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
