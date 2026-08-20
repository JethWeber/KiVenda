using KiVenda.Core.Auditoria;
using KiVenda.Core.Caixa;
using KiVenda.Core.Clientes;
using KiVenda.Core.Compras;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;

namespace KiVenda.Application.Tests.Fakes;

/// <summary>
/// Estado partilhado em memória usado pelos repositórios fake nos
/// testes da Application. A Application não deve depender da
/// Persistence real (EF Core) para os seus próprios testes — isso
/// mantém esta suíte rápida e focada exclusivamente na orquestração dos
/// casos de uso, não no mapeamento relacional (já coberto pelos testes
/// de integração da Fase 2).
/// </summary>
public sealed class InMemoryDatabase
{
    public List<Categoria> Categorias { get; } = new();
    public List<UnidadeMedida> UnidadesMedida { get; } = new();
    public List<Produto> Produtos { get; } = new();
    public List<MovimentoStock> MovimentosStock { get; } = new();
    public List<Cliente> Clientes { get; } = new();
    public List<Fornecedor> Fornecedores { get; } = new();
    public List<Compra> Compras { get; } = new();
    public List<Venda> Vendas { get; } = new();
    public List<SessaoCaixa> SessoesCaixa { get; } = new();
    public List<Utilizador> Utilizadores { get; } = new();
    public List<LogAuditoria> LogsAuditoria { get; } = new();
}
