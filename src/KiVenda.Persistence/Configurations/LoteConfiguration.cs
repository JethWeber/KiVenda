using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

/// <summary>
/// Mapeamento de <see cref="Lote"/>. A entidade já existe no schema
/// desde a Fase 2 (ver Nota de Revisão de Domínio — Estoque), mas não é
/// usada operacionalmente no MVP: nenhum caso de uso da Fase 3 cria
/// lotes; apenas <see cref="MovimentoStock.LoteId"/> a referencia, de
/// forma opcional, preparando o terreno para custeio por lote/FIFO
/// numa fase futura sem remodelação do schema.
/// </summary>
public sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("Lotes");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(l => l.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.ProdutoId, l.Codigo }).IsUnique();
    }
}
