using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePresensiIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Presensi_NimMahasiswa",
                table: "Presensi");

            migrationBuilder.CreateIndex(
                name: "IX_Presensi_NimMahasiswa_JadwalKuliahId",
                table: "Presensi",
                columns: new[] { "NimMahasiswa", "JadwalKuliahId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Presensi_NimMahasiswa_JadwalKuliahId",
                table: "Presensi");

            migrationBuilder.CreateIndex(
                name: "IX_Presensi_NimMahasiswa",
                table: "Presensi",
                column: "NimMahasiswa");
        }
    }
}
