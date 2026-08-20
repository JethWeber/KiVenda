using FluentAssertions;
using KiVenda.Application.Stock;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Application.Tests.Stock;

public class RegistarAjusteStockUseCaseTests
{
    [Fact]
    public async Task Ajuste_Deve_Alterar_Estoque_E_Registar_Auditoria()
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var produto = new Produto("Arroz", "PRD-AR01", categoria.Id, unidade.Id, 1.2m, 2000m);
        produto.RegistarEntradaStock(10000m, 10000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), Guid.NewGuid());
        db.Produtos.Add(produto);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new RegistarAjusteStockUseCase(uow, contexto);

        await useCase.ExecutarAsync(new RegistarAjusteStockCommand(produto.Id, -300m, "Quebra na contagem física"));

        produto.EstoqueAtual.Should().Be(9700m);
        db.MovimentosStock.Should().ContainSingle(m => m.ProdutoId == produto.Id && m.Tipo == TipoMovimentoStock.Ajuste);
        db.LogsAuditoria.Should().ContainSingle(l => l.EntidadeAfetada == "Produto" && l.EntidadeId == produto.Id);
    }

    [Fact]
    public async Task Atendente_Nao_Deve_Conseguir_Ajustar_Stock()
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var produto = new Produto("Arroz", "PRD-AR01", categoria.Id, unidade.Id, 1.2m, 2000m);
        db.Produtos.Add(produto);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var useCase = new RegistarAjusteStockUseCase(new InMemoryUnitOfWork(db), contexto);

        var acao = async () => await useCase.ExecutarAsync(new RegistarAjusteStockCommand(produto.Id, -10m, "Quebra"));

        await acao.Should().ThrowAsync<Exceptions.PermissaoNegadaException>();
    }

    [Fact]
    public async Task Ajuste_Que_Deixaria_Stock_Negativo_Deve_Ser_Rejeitado_Pelo_Core()
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var produto = new Produto("Arroz", "PRD-AR01", categoria.Id, unidade.Id, 1.2m, 2000m);
        produto.RegistarEntradaStock(100m, 100m, OrigemMovimentoStock.Compra, Guid.NewGuid(), Guid.NewGuid());
        db.Produtos.Add(produto);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var useCase = new RegistarAjusteStockUseCase(new InMemoryUnitOfWork(db), contexto);

        var acao = async () => await useCase.ExecutarAsync(new RegistarAjusteStockCommand(produto.Id, -500m, "Contagem"));

        await acao.Should().ThrowAsync<DomainException>();
    }
}
