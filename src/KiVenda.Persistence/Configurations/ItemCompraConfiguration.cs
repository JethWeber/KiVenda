using KiVenda.Core.Compras;
using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

/// <summary>
/// <see cref="ItemCompra"/> não tem, no domínio, uma propriedade
/// "CompraId" (o Core só conhece a relação a partir de <see cref="Compra.Itens"/>).
/// A ligação relacional é feita aqui através de uma propriedade sombra
/// ("CompraId"), configurada em <see cref="CompraConfiguration"/> — uma
/// forma de manter o domínio limpo sem abrir mão de uma FK real na base
/// de dados.
/// </summary>
public sealed class ItemCompraConfiguration : IEntityTypeConfiguration<ItemCompra>
{
    public void Configure(EntityTypeBuilder<ItemCompra> builder)
    {
        builder.ToTable("ItensCompra");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantidadeNaApresentacao).HasPrecision(18, 4);
        builder.Property(i => i.QuantidadeUnidadeBase).HasPrecision(18, 4);
        builder.Property(i => i.CustoTotalItem).HasPrecision(18, 4);

        builder.Ignore(i => i.CustoUnitarioUnidadeBase); // calculado em memória, não persistido

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
