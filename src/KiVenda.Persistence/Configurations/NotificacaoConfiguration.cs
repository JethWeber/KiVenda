using KiVenda.Core.Notificacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("Notificacoes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Tipo).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Titulo).IsRequired().HasMaxLength(150);
        builder.Property(n => n.Mensagem).IsRequired().HasMaxLength(500);
        builder.Property(n => n.DataCriacao).IsRequired();
        builder.Property(n => n.Lida).IsRequired();
        builder.HasIndex(n => new { n.UtilizadorId, n.DataCriacao });
        builder.HasIndex(n => new { n.UtilizadorId, n.Lida });
        builder.HasOne<KiVenda.Core.Utilizadores.Utilizador>()
            .WithMany()
            .HasForeignKey(n => n.UtilizadorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
