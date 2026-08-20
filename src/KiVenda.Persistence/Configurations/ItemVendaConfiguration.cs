using KiVenda.Core.Produtos;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

/// <summary>
/// Mesmo padrão de FK sombra usado em <see cref="ItemCompraConfiguration"/>:
/// <see cref="ItemVenda"/> não tem "VendaId" no domínio.
/// </summary>
public sealed class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("ItensVenda");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantidadeNaApresentacao).HasPrecision(18, 4);
        builder.Property(i => i.QuantidadeUnidadeBase).HasPrecision(18, 4);
        builder.Property(i => i.PrecoUnitarioUnidadeBase).HasPrecision(18, 6);
        builder.Property(i => i.CustoUnitarioUnidadeBase).HasPrecision(18, 6);

        builder.Ignore(i => i.ValorTotal); // calculado em memória
        builder.Ignore(i => i.LucroEstimado); // calculado em memória

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApresentacaoProduto>()
            .WithMany()
            .HasForeignKey(i => i.ApresentacaoProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
