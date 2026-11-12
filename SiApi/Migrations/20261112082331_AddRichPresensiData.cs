using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRichPresensiData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keterangan",
                table: "Presensi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaMahasiswa",
                table: "Presensi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaMataKuliah",
                table: "Presensi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keterangan",
                table: "Presensi");

            migrationBuilder.DropColumn(
                name: "NamaMahasiswa",
                table: "Presensi");

            migrationBuilder.DropColumn(
                name: "NamaMataKuliah",
                table: "Presensi");
        }
    }
}
