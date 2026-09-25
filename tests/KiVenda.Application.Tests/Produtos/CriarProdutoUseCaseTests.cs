using FluentAssertions;
using KiVenda.Application.Exceptions;
using KiVenda.Application.Produtos;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Application.Tests.Produtos;

public class CriarProdutoUseCaseTests
{
    private static (InMemoryDatabase Db, InMemoryUnitOfWork Uow, FakeContextoAutenticacao Contexto) CriarAmbiente(PerfilUtilizador perfil = PerfilUtilizador.Gerente)
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);

        var contexto = new FakeContextoAutenticacao { Perfil = perfil };
        return (db, new InMemoryUnitOfWork(db), contexto);
    }

    [Fact]
    public async Task Gerente_Deve_Conseguir_Criar_Produto_Com_Codigo_Interno_Automatico()
    {
        var (db, uow, contexto) = CriarAmbiente();
        var useCase = new CriarProdutoUseCase(uow, contexto);

        var comando = new CriarProdutoCommand(
            "Açúcar", db.Categorias[0].Id, db.UnidadesMedida[0].Id, 1.5m, 5000m);

        var produtoId = await useCase.ExecutarAsync(comando);

        db.Produtos.Should().ContainSingle(p =>
            p.Id == produtoId &&
            p.Nome == "Açúcar" &&
            p.CodigoInterno == "PRD-0000");
    }

    [Fact]
    public async Task Atendente_Nao_Deve_Conseguir_Criar_Produto()
    {
        var (db, uow, contexto) = CriarAmbiente(PerfilUtilizador.Atendente);
        var useCase = new CriarProdutoUseCase(uow, contexto);

        var comando = new CriarProdutoCommand(
            "Açúcar", db.Categorias[0].Id, db.UnidadesMedida[0].Id, 1.5m, 5000m);

        var acao = async () => await useCase.ExecutarAsync(comando);

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }

    [Fact]
    public async Task Deve_Gerar_Codigos_Internos_Sequenciais()
    {
        var (db, uow, contexto) = CriarAmbiente();
        var useCase = new CriarProdutoUseCase(uow, contexto);

        var primeiro = new CriarProdutoCommand(
            "Açúcar", db.Categorias[0].Id, db.UnidadesMedida[0].Id, 1.5m, 5000m);
        var segundo = new CriarProdutoCommand(
            "Açúcar Fino", db.Categorias[0].Id, db.UnidadesMedida[0].Id, 2m, 1000m);

        await useCase.ExecutarAsync(primeiro);
        await useCase.ExecutarAsync(segundo);

        db.Produtos.Should().HaveCount(2);
        db.Produtos.Select(p => p.CodigoInterno)
            .Should().BeEquivalentTo(["PRD-0000", "PRD-0001"]);
    }
}
