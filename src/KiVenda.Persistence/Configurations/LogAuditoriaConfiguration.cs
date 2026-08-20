using KiVenda.Core.Auditoria;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("LogsAuditoria");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Acao)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.EntidadeAfetada)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.DadosAntes).HasMaxLength(1000);
        builder.Property(l => l.DadosDepois).HasMaxLength(1000);

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(l => l.UtilizadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.DataHora);
        builder.HasIndex(l => new { l.EntidadeAfetada, l.EntidadeId });
    }
}
