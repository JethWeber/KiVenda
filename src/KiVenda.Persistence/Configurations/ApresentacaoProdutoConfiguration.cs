using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class ApresentacaoProdutoConfiguration : IEntityTypeConfiguration<ApresentacaoProduto>
{
    public void Configure(EntityTypeBuilder<ApresentacaoProduto> builder)
    {
        builder.ToTable("ApresentacoesProduto");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.CodigoBarras)
            .HasMaxLength(30);

        builder.Property(a => a.FatorConversaoParaUnidadeBase).HasPrecision(18, 6);

        builder.HasIndex(a => a.CodigoBarras).IsUnique().HasFilter("[CodigoBarras] IS NOT NULL");
        builder.HasIndex(a => new { a.ProdutoId, a.Ativa });
    }
}
