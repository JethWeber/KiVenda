using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class MovimentoStockConfiguration : IEntityTypeConfiguration<MovimentoStock>
{
    public void Configure(EntityTypeBuilder<MovimentoStock> builder)
    {
        builder.ToTable("MovimentosStock");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Quantidade).HasPrecision(18, 4);
        builder.Property(m => m.CustoUnitarioUnidadeBase).HasPrecision(18, 6);
        builder.Property(m => m.Motivo).HasMaxLength(300);

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(m => m.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(m => m.UtilizadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice dedicado por produto + data: usado tanto para paginar o
        // histórico (ConsultarMovimentosStock) como para recalcular o
        // estoque materializado a partir do zero (RecalcularEstoqueMaterializado)
        // — ver Fase 2 do plano de implementação.
        builder.HasIndex(m => new { m.ProdutoId, m.Data });

        builder.HasIndex(m => m.OrigemId);
    }
}
