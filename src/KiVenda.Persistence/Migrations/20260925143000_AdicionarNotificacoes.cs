using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiVenda.Persistence.Migrations;

public partial class AdicionarNotificacoes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notificacoes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                UtilizadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Mensagem = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                Lida = table.Column<bool>(type: "INTEGER", nullable: false),
                DataLeitura = table.Column<DateTime>(type: "TEXT", nullable: true),
                CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notificacoes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Notificacoes_Utilizadores_UtilizadorId",
                    column: x => x.UtilizadorId,
                    principalTable: "Utilizadores",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Notificacoes_UtilizadorId_DataCriacao",
            table: "Notificacoes",
            columns: new[] { "UtilizadorId", "DataCriacao" });

        migrationBuilder.CreateIndex(
            name: "IX_Notificacoes_UtilizadorId_Lida",
            table: "Notificacoes",
            columns: new[] { "UtilizadorId", "Lida" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Notificacoes");
}
