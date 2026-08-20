using KiVenda.Core.Fornecedores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(f => f.Telefone)
            .HasMaxLength(30);

        builder.Property(f => f.ProdutosFornecidos)
            .HasMaxLength(500);

        builder.HasIndex(f => f.Nome);
    }
}
