using FluentAssertions;
using KiVenda.Application.Relatorios;
using KiVenda.Application.Exceptions;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Produtos;
using KiVenda.Core.Vendas;
using Xunit;

namespace KiVenda.Application.Tests.Relatorios;

public class RelatoriosUseCasesTests
{
    [Fact]
    public async Task RelatorioDiario_Deve_Agregar_Vendas_Finalizadas()
    {
        var db = CriarBaseComVendaFinalizada(quantidade: 2, preco: 100m);
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new GerarRelatorioDiarioUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new GerarRelatorioDiarioQuery(DateOnly.FromDateTime(DateTime.UtcNow)));

        resultado.NumeroDeVendas.Should().Be(1);
        resultado.TotalVendido.Should().Be(200m);
        resultado.ProdutosVendidos.Should().ContainSingle();
        resultado.ProdutosVendidos[0].QuantidadeUnidadeBase.Should().Be(2m);
    }

    [Fact]
    public async Task RelatorioMensal_Deve_Agregar_Receita_E_Produtos_Mais_Vendidos()
    {
        var db = CriarBaseComVendaFinalizada(quantidade: 3, preco: 50m);
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new GerarRelatorioMensalUseCase(uow, contexto);

        var agora = DateTime.UtcNow;
        var resultado = await useCase.ExecutarAsync(new GerarRelatorioMensalQuery(agora.Year, agora.Month));

        resultado.Receita.Should().Be(150m);
        resultado.ProdutosMaisVendidos.Should().ContainSingle();
        resultado.ProdutosMaisVendidos[0].QuantidadeUnidadeBase.Should().Be(3m);
    }

    [Fact]
    public async Task RelatorioStock_Deve_Separar_SemStock_De_StockBaixo()
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Unidade, "Unidade");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);

        var semStock = new Produto("Sem Stock", "PRD-001", categoria.Id, unidade.Id, 10m, 5m);
        var stockBaixo = new Produto("Stock Baixo", "PRD-002", categoria.Id, unidade.Id, 10m, 5m);
        stockBaixo.RegistarEntradaStock(3m, 30m, OrigemMovimentoStock.Compra, Guid.NewGuid(), Guid.NewGuid());

        db.Produtos.AddRange([semStock, stockBaixo]);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new GerarRelatorioStockUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync();

        resultado.ProdutosEmFalta.Should().ContainSingle(p => p.ProdutoNome == "Sem Stock");
        resultado.ProdutosComStockBaixo.Should().ContainSingle(p => p.ProdutoNome == "Stock Baixo");
        resultado.ProdutosComStockBaixo[0].QuantidadeApresentacao.Should().Be(3m);
    }

    [Fact]
    public async Task Relatorios_Devem_Bloquear_Atendente()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new GerarRelatorioStockUseCase(uow, contexto);

        var acao = () => useCase.ExecutarAsync();

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }

    private static InMemoryDatabase CriarBaseComVendaFinalizada(decimal quantidade, decimal preco)
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Unidade, "Unidade");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);

        var utilizadorId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        var produto = new Produto("Arroz", "PRD-AR01", categoria.Id, unidade.Id, preco, 1m);
        var venda = new Venda(utilizadorId, sessaoId);
        venda.AdicionarItem(produto, produto.Apresentacoes.First().Id, quantidade);
        venda.AdicionarPagamento(MetodoPagamento.Dinheiro, quantidade * preco);
        venda.Finalizar();

        db.Produtos.Add(produto);
        db.Vendas.Add(venda);
        return db;
    }
}