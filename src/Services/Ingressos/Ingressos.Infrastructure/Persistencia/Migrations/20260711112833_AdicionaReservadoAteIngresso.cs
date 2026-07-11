using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ingressos.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaReservadoAteIngresso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservadoAte",
                table: "Ingressos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservadoAte",
                table: "Ingressos");
        }
    }
}
