using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStarterCodeToAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StarterCode",
                table: "Assignments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StarterCode",
                table: "Assignments");
        }
    }
}
