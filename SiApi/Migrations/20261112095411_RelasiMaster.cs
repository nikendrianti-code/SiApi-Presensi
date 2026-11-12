using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiApi.Migrations
{
    /// <inheritdoc />
    public partial class RelasiMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaMahasiswa",
                table: "Presensi");

            migrationBuilder.DropColumn(
                name: "NamaMataKuliah",
                table: "Presensi");

            migrationBuilder.AlterColumn<string>(
                name: "NimMahasiswa",
                table: "Presensi",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "JadwalKuliah",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaMataKuliah = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JadwalKuliah", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mahasiswa",
                columns: table => new
                {
                    Nim = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mahasiswa", x => x.Nim);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Presensi_JadwalKuliahId",
                table: "Presensi",
                column: "JadwalKuliahId");

            migrationBuilder.CreateIndex(
                name: "IX_Presensi_NimMahasiswa",
                table: "Presensi",
                column: "NimMahasiswa");

            migrationBuilder.AddForeignKey(
                name: "FK_Presensi_JadwalKuliah_JadwalKuliahId",
                table: "Presensi",
                column: "JadwalKuliahId",
                principalTable: "JadwalKuliah",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Presensi_Mahasiswa_NimMahasiswa",
                table: "Presensi",
                column: "NimMahasiswa",
                principalTable: "Mahasiswa",
                principalColumn: "Nim",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presensi_JadwalKuliah_JadwalKuliahId",
                table: "Presensi");

            migrationBuilder.DropForeignKey(
                name: "FK_Presensi_Mahasiswa_NimMahasiswa",
                table: "Presensi");

            migrationBuilder.DropTable(
                name: "JadwalKuliah");

            migrationBuilder.DropTable(
                name: "Mahasiswa");

            migrationBuilder.DropIndex(
                name: "IX_Presensi_JadwalKuliahId",
                table: "Presensi");

            migrationBuilder.DropIndex(
                name: "IX_Presensi_NimMahasiswa",
                table: "Presensi");

            migrationBuilder.AlterColumn<string>(
                name: "NimMahasiswa",
                table: "Presensi",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
    }
}
