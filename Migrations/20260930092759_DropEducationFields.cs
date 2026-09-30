using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerManagementPractiseCS.Migrations
{
    /// <inheritdoc />
    public partial class DropEducationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldOfStudy",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "ResultType",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "ResultScale",
                table: "Educations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FieldOfStudy",
                table: "Educations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultType",
                table: "Educations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ResultScale",
                table: "Educations",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
