using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace estimateProjectx.Migrations
{
    public partial class enabledindentitynullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_AspNetUsers_IdentityUserId",
                table: "Vote");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "Vote",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_AspNetUsers_IdentityUserId",
                table: "Vote",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_AspNetUsers_IdentityUserId",
                table: "Vote");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "Vote",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_AspNetUsers_IdentityUserId",
                table: "Vote",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
