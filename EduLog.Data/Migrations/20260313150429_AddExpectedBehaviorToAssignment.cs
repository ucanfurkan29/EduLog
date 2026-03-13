using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedBehaviorToAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedBehavior",
                table: "Assignments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedBehavior",
                table: "Assignments");
        }
    }
}
