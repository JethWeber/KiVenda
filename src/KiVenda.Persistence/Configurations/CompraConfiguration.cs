using KiVenda.Core.Compras;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compras");

        builder.HasKey(c => c.Id);

        builder.Ignore(c => c.CustoTotal); // calculado em memória a partir dos itens

        builder.HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(c => c.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(c => c.UtilizadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey("CompraId") // FK sombra: ItemCompra não conhece o Id da Compra no domínio (ver ItemCompraConfiguration).
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.Data);
        builder.HasIndex(c => c.FornecedorId);
    }
}
