using KiVenda.Core.Caixa;
using KiVenda.Core.Clientes;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiVenda.Persistence.Configurations;

public sealed class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("Vendas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Desconto).HasPrecision(18, 4);
        builder.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(v => v.Subtotal); // calculado em memória a partir dos itens
        builder.Ignore(v => v.Total); // calculado em memória (Subtotal - Desconto)
        builder.Ignore(v => v.TotalPago); // calculado em memória a partir dos pagamentos
        builder.Ignore(v => v.LucroEstimado); // calculado em memória a partir dos itens

        builder.HasOne<Utilizador>()
            .WithMany()
            .HasForeignKey(v => v.UtilizadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SessaoCaixa>()
            .WithMany()
            .HasForeignKey(v => v.SessaoCaixaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Itens: ItemVenda não conhece o Id da venda no domínio -> FK sombra
        // (ver ItemVendaConfiguration), mesmo padrão usado em Compra/ItemCompra.
        builder.HasMany(v => v.Itens)
            .WithOne()
            .HasForeignKey("VendaId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Pagamentos: Pagamento JÁ tem VendaId explícito no domínio (permite
        // pagamento misto sem ambiguidade), por isso a FK aqui é direta.
        builder.HasMany(v => v.Pagamentos)
            .WithOne()
            .HasForeignKey(p => p.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.Pagamentos)
            .HasField("_pagamentos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(v => v.Data);
        builder.HasIndex(v => v.UtilizadorId);
        builder.HasIndex(v => v.SessaoCaixaId);
    }
}
