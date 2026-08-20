using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Valor).HasPrecision(18, 4);
        builder.Property(p => p.Metodo).HasConversion<string>().HasMaxLength(20);
    }
}
