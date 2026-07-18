using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pagamento.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEmailClientePagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailCliente",
                table: "Pagamentos",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCliente",
                table: "Pagamentos");
        }
    }
}
