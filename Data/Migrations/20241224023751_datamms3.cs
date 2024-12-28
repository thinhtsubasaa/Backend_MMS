using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class datamms3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_statuss",
                table: "statuss");

            migrationBuilder.RenameTable(
                name: "statuss",
                newName: "Statuss");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statuss",
                table: "Statuss",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Statuss",
                table: "Statuss");

            migrationBuilder.RenameTable(
                name: "Statuss",
                newName: "statuss");

            migrationBuilder.AddPrimaryKey(
                name: "PK_statuss",
                table: "statuss",
                column: "Id");
        }
    }
}
