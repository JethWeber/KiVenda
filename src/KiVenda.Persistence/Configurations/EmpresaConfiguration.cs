using KiVenda.Core.Empresas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeComercial).IsRequired().HasMaxLength(150);
        builder.Property(e => e.RazaoSocial).HasMaxLength(200);
        builder.Property(e => e.Nif).HasMaxLength(30);
        builder.Property(e => e.Telefone).HasMaxLength(50);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.Endereco).HasMaxLength(250);
        builder.Property(e => e.Municipio).HasMaxLength(100);
        builder.Property(e => e.Provincia).HasMaxLength(100);
        builder.Property(e => e.Website).HasMaxLength(200);
        builder.Property(e => e.Logo).HasColumnType("BLOB");
        builder.Property(e => e.LogoMimeType).HasMaxLength(50);
    }
}
