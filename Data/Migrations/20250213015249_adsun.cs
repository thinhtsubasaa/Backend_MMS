using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class adsun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Adsuns",
                table: "Adsuns");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Adsuns",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Id_Adsun",
                table: "Adsuns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adsuns",
                table: "Adsuns",
                column: "Id_Adsun");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Adsuns",
                table: "Adsuns");

            migrationBuilder.DropColumn(
                name: "Id_Adsun",
                table: "Adsuns");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Adsuns",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adsuns",
                table: "Adsuns",
                column: "Id");
        }
    }
}
