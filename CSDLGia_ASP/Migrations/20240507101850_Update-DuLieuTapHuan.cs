﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSDLGia_ASP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDuLieuTapHuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HuongDanSuDung");

            migrationBuilder.CreateTable(
                name: "DuLieuTapHuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChucNang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileMau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STTSapxep = table.Column<int>(type: "int", nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuLieuTapHuan", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuLieuTapHuan");

            migrationBuilder.CreateTable(
                name: "HuongDanSuDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileMau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STTSapxep = table.Column<int>(type: "int", nullable: false),
                    TenChucNang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuongDanSuDung", x => x.Id);
                });
        }
    }
}