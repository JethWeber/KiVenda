using KiVenda.Core.Caixa;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class MovimentoCaixaConfiguration : IEntityTypeConfiguration<MovimentoCaixa>
{
    public void Configure(EntityTypeBuilder<MovimentoCaixa> builder)
    {
        builder.ToTable("MovimentosCaixa");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Valor).HasPrecision(18, 4);
        builder.Property(m => m.Descricao).HasMaxLength(300);
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(m => m.EhEntrada); // calculado em memória

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(m => m.UtilizadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Venda>()
            .WithMany()
            .HasForeignKey(m => m.OrigemVendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.SessaoCaixaId);
        builder.HasIndex(m => m.Data);
    }
}
