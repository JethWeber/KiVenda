using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.CodigoInterno)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.CodigoBarras)
            .HasMaxLength(30);

        builder.Property(p => p.FotoUrl)
            .HasMaxLength(500);

        builder.Property(p => p.PrecoVendaPorUnidadeBase).HasPrecision(18, 4);
        builder.Property(p => p.StockMinimo).HasPrecision(18, 4);
        builder.Property(p => p.EstoqueAtual).HasPrecision(18, 4);
        builder.Property(p => p.CustoMedioPonderado).HasPrecision(18, 4);

        builder.HasIndex(p => p.CodigoInterno).IsUnique();
        builder.HasIndex(p => p.CodigoBarras).IsUnique().HasFilter("[CodigoBarras] IS NOT NULL");
        builder.HasIndex(p => p.Nome);
        builder.HasIndex(p => p.Ativo);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnidadeMedida>()
            .WithMany()
            .HasForeignKey(p => p.UnidadeBaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Coleção de apresentações comerciais: propriedade só de leitura
        // (IReadOnlyCollection), apoiada pelo campo privado "_apresentacoes".
        // EF Core já reconheceria isto por convenção, mas é declarado
        // explicitamente para deixar claro o padrão usado em todo o Core.
        builder.HasMany(p => p.Apresentacoes)
            .WithOne()
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Apresentacoes)
            .HasField("_apresentacoes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
