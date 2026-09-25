using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiVenda.Persistence.Migrations;

public partial class CriarEmpresa : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Empresa",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                NomeComercial = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Nif = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                Telefone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                Endereco = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                Municipio = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Provincia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Website = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Logo = table.Column<byte[]>(type: "BLOB", nullable: true),
                LogoMimeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Empresa", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Empresa");
    }
}
