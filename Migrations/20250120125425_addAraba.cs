using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBGoreWebApp.Migrations
{
    /// <inheritdoc />
    public partial class addAraba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArabaAciklama",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaCekis",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaFiyat",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaKasaTipi",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaKoltukSayisi",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaMarka",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaModel",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaMotorGucu",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaMotorHacmi",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaOzellikler",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaRenk",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaResim",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaSeri",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaTur",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaVites",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ArabaYakit",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "IlanTarihi",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "SatildiMi",
                table: "Arabalar");

            migrationBuilder.RenameColumn(
                name: "ArabaModelYili",
                table: "Arabalar",
                newName: "YilID");

            migrationBuilder.RenameColumn(
                name: "ArabaKm",
                table: "Arabalar",
                newName: "VersiyonID");

            migrationBuilder.AddColumn<int>(
                name: "AdresKonumu",
                table: "Arabalar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "Arabalar",
                type: "varchar(1)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MarkaID",
                table: "Arabalar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModelID",
                table: "Arabalar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Ozellikleri",
                table: "Arabalar",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VersiyonAdi",
                table: "Arabalar",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdresKonumu",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "MarkaID",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "ModelID",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "Ozellikleri",
                table: "Arabalar");

            migrationBuilder.DropColumn(
                name: "VersiyonAdi",
                table: "Arabalar");

            migrationBuilder.RenameColumn(
                name: "YilID",
                table: "Arabalar",
                newName: "ArabaModelYili");

            migrationBuilder.RenameColumn(
                name: "VersiyonID",
                table: "Arabalar",
                newName: "ArabaKm");

            migrationBuilder.AddColumn<string>(
                name: "ArabaAciklama",
                table: "Arabalar",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaCekis",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "ArabaFiyat",
                table: "Arabalar",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ArabaKasaTipi",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaKoltukSayisi",
                table: "Arabalar",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaMarka",
                table: "Arabalar",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaModel",
                table: "Arabalar",
                type: "varchar(55)",
                maxLength: 55,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaMotorGucu",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaMotorHacmi",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaOzellikler",
                table: "Arabalar",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaRenk",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaResim",
                table: "Arabalar",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaSeri",
                table: "Arabalar",
                type: "varchar(55)",
                maxLength: 55,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaTur",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaVites",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArabaYakit",
                table: "Arabalar",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "IlanTarihi",
                table: "Arabalar",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "SatildiMi",
                table: "Arabalar",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
