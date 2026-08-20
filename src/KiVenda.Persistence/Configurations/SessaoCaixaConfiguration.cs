using KiVenda.Core.Caixa;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class SessaoCaixaConfiguration : IEntityTypeConfiguration<SessaoCaixa>
{
    public void Configure(EntityTypeBuilder<SessaoCaixa> builder)
    {
        builder.ToTable("SessoesCaixa");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaldoInicial).HasPrecision(18, 4);
        builder.Property(s => s.SaldoFinalInformado).HasPrecision(18, 4);
        builder.Property(s => s.Divergencia).HasPrecision(18, 4);
        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(s => s.TotalEntradas); // calculado em memória a partir dos movimentos
        builder.Ignore(s => s.TotalSaidas); // calculado em memória a partir dos movimentos
        builder.Ignore(s => s.SaldoCalculado); // calculado em memória (SaldoInicial + Entradas - Saídas)

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(s => s.UtilizadorAberturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(s => s.UtilizadorFechoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Movimentos)
            .WithOne()
            .HasForeignKey(m => m.SessaoCaixaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Movimentos)
            .HasField("_movimentos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // O MVP assume um único caixa aberto de cada vez (ver Fase 5/7);
        // este índice ajuda o repositório a localizar rapidamente a
        // sessão aberta, sem impor unicidade a nível de base de dados
        // (a regra "só pode haver uma aberta" é aplicada na Application).
        builder.HasIndex(s => s.Estado);
        builder.HasIndex(s => s.DataAbertura);
    }
}
