using KiVenda.Core.Auditoria;
using KiVenda.Core.Caixa;
using KiVenda.Core.Clientes;
using KiVenda.Core.Compras;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence;

/// <summary>
/// DbContext único do KiVenda Desktop, sobre SQLite local (sem servidor).
/// Os mapeamentos ficam em <c>Configurations/</c>, um ficheiro por
/// entidade, aplicados via <see cref="ApplyConfigurationsFromAssembly"/>.
/// </summary>
public sealed class KiVendaDbContext : DbContext
{
    public KiVendaDbContext(DbContextOptions<KiVendaDbContext> options) : base(options)
    {
    }

    // Produtos / Estoque
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ApresentacaoProduto> ApresentacoesProduto => Set<ApresentacaoProduto>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<MovimentoStock> MovimentosStock => Set<MovimentoStock>();

    // Clientes / Fornecedores
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    // Compras
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<ItemCompra> ItensCompra => Set<ItemCompra>();

    // Vendas
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    // Caixa
    public DbSet<SessaoCaixa> SessoesCaixa => Set<SessaoCaixa>();
    public DbSet<MovimentoCaixa> MovimentosCaixa => Set<MovimentoCaixa>();

    // Utilizadores / Auditoria
    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KiVendaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
