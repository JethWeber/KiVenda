using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("UnidadesMedida");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Codigo)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Codigo).IsUnique();
    }
}
