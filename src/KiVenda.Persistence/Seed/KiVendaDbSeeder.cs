using KiVenda.Core.Enums;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Seed;

/// <summary>
/// Semeia os dados mínimos necessários para o fluxo de primeira
/// execução ("instalar e vender em 5 minutos" — Secção 3 da
/// documentação funcional): unidades de medida padrão, uma categoria
/// genérica e o utilizador Gerente inicial.
///
/// O hash da password NÃO é calculado aqui — essa é responsabilidade da
/// Infrastructure (Fase 4, serviço de hashing); este seeder só persiste
/// o hash já calculado que lhe é passado, para o Persistence não ter de
/// conhecer o algoritmo de hashing usado.
/// </summary>
public static class KiVendaDbSeeder
{
    public static async Task SeedAsync(KiVendaDbContext context, string senhaGerentePadraoHash, CancellationToken cancellationToken = default)
    {
        await SeedUnidadesMedidaAsync(context, cancellationToken);
        await SeedCategoriaGeralAsync(context, cancellationToken);
        await SeedUtilizadorGerenteAsync(context, senhaGerentePadraoHash, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUnidadesMedidaAsync(KiVendaDbContext context, CancellationToken cancellationToken)
    {
        var existentes = await context.UnidadesMedida
            .Select(u => u.Codigo)
            .ToListAsync(cancellationToken);

        var padrao = new (string Codigo, string Nome)[]
        {
            (UnidadeMedida.Padrao.Unidade, "Unidade"),
            (UnidadeMedida.Padrao.Grama, "Grama"),
            (UnidadeMedida.Padrao.Mililitro, "Mililitro"),
        };

        foreach (var (codigo, nome) in padrao)
        {
            if (!existentes.Contains(codigo))
            {
                await context.UnidadesMedida.AddAsync(new UnidadeMedida(codigo, nome), cancellationToken);
            }
        }
    }

    private static async Task SeedCategoriaGeralAsync(KiVendaDbContext context, CancellationToken cancellationToken)
    {
        var existe = await context.Categorias.AnyAsync(c => c.Nome == "Geral", cancellationToken);

        if (!existe)
        {
            await context.Categorias.AddAsync(new Categoria("Geral"), cancellationToken);
        }
    }

    private static async Task SeedUtilizadorGerenteAsync(KiVendaDbContext context, string senhaGerentePadraoHash, CancellationToken cancellationToken)
    {
        var existeGerente = await context.Utilizadores.AnyAsync(u => u.Perfil == PerfilUtilizador.Gerente, cancellationToken);

        if (!existeGerente)
        {
            var gerente = new Utilizador("Gerente", "gerente", senhaGerentePadraoHash, PerfilUtilizador.Gerente);
            await context.Utilizadores.AddAsync(gerente, cancellationToken);
        }
    }
}
