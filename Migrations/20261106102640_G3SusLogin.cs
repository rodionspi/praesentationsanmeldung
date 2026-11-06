using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace praesentationsanmeldung.Migrations
{
    /// <inheritdoc />
    public partial class G3SusLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "G3Sus",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswortHash",
                table: "G3Sus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_G3Sus_Email",
                table: "G3Sus",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_G3Sus_Email",
                table: "G3Sus");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "G3Sus");

            migrationBuilder.DropColumn(
                name: "PasswortHash",
                table: "G3Sus");
        }
    }
}
