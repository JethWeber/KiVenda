using FluentAssertions;
using KiVenda.Application.Produtos;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Application.Tests.Produtos;

public class LocalizarProdutoPorCodigoUseCaseTests
{
    private static (InMemoryDatabase Db, InMemoryUnitOfWork Uow, FakeContextoAutenticacao Contexto, Categoria Categoria, UnidadeMedida Unidade)
        CriarAmbiente(PerfilUtilizador perfil = PerfilUtilizador.Atendente)
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);

        var contexto = new FakeContextoAutenticacao { Perfil = perfil };
        return (db, new InMemoryUnitOfWork(db), contexto, categoria, unidade);
    }

    private static Produto CriarAcucar(InMemoryDatabase db, Categoria categoria, UnidadeMedida unidade)
    {
        // Código de barras do produto + apresentações com EAN próprio.
        var produto = new Produto(
            nome: "Açúcar",
            codigoInterno: "PRD-AC01",
            categoriaId: categoria.Id,
            unidadeBaseId: unidade.Id,
            precoVendaPorUnidadeBase: 1.5m,
            stockMinimo: 5000m,
            codigoBarras: "5601234567890");

        produto.AdicionarApresentacao("1 kg", fatorConversaoParaUnidadeBase: 1000m, codigoBarras: "5601234567891");
        produto.AdicionarApresentacao("Saco 25 kg", fatorConversaoParaUnidadeBase: 25000m, codigoBarras: "5601234567892");

        db.Produtos.Add(produto);
        return produto;
    }

    [Fact]
    public async Task Deve_Resolver_Por_Codigo_Barras_Do_Produto()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente();
        var produto = CriarAcucar(db, categoria, unidade);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("5601234567890"));

        resultado.Should().NotBeNull();
        resultado!.Produto.Id.Should().Be(produto.Id);
        resultado.Origem.Should().Be(OrigemCodigoResolvido.CodigoBarrasProduto);
        var apresentacaoPadrao = produto.Apresentacoes.Single(a => a.FatorConversaoParaUnidadeBase == 1m);
        resultado.ApresentacaoId.Should().Be(apresentacaoPadrao.Id);
    }

    [Fact]
    public async Task Deve_Resolver_Apresentacao_Especifica_Quando_Codigo_E_Dela()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente();
        var produto = CriarAcucar(db, categoria, unidade);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("5601234567891"));

        resultado.Should().NotBeNull();
        resultado!.Produto.Id.Should().Be(produto.Id);
        resultado.Origem.Should().Be(OrigemCodigoResolvido.CodigoBarrasApresentacao);
        var apresentacao1kg = produto.Apresentacoes.Single(a => a.Nome == "1 kg");
        resultado.ApresentacaoId.Should().Be(apresentacao1kg.Id);
        resultado.NomeApresentacao.Should().Be("1 kg");
    }

    [Fact]
    public async Task Deve_Resolver_Saco_25kg_Por_Codigo_De_Apresentacao()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente();
        var produto = CriarAcucar(db, categoria, unidade);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("5601234567892"));

        resultado.Should().NotBeNull();
        resultado!.Origem.Should().Be(OrigemCodigoResolvido.CodigoBarrasApresentacao);
        resultado.NomeApresentacao.Should().Be("Saco 25 kg");
        resultado.ApresentacaoId.Should().Be(produto.Apresentacoes.Single(a => a.Nome == "Saco 25 kg").Id);
    }

    [Fact]
    public async Task Deve_Cair_Para_Codigo_Interno_Quando_Nao_Ha_Codigo_Barras()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente();
        var produto = new Produto("Sal", "PRD-SAL1", categoria.Id, unidade.Id, 0.5m, 1000m);
        db.Produtos.Add(produto);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("PRD-SAL1"));

        resultado.Should().NotBeNull();
        resultado!.Produto.Id.Should().Be(produto.Id);
        resultado.Origem.Should().Be(OrigemCodigoResolvido.CodigoInterno);
        resultado.ApresentacaoId.Should().Be(produto.Apresentacoes.Single(a => a.FatorConversaoParaUnidadeBase == 1m).Id);
    }

    [Fact]
    public async Task Deve_Devolver_Null_Quando_Codigo_Inexistente()
    {
        var (_, uow, contexto, _, _) = CriarAmbiente();
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("CODIGO-INEXISTENTE"));

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Deve_Devolver_Null_Quando_Codigo_Vazio()
    {
        var (_, uow, contexto, _, _) = CriarAmbiente();
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("   "));

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Nao_Deve_Devolver_Produto_Inativo()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente();
        var produto = CriarAcucar(db, categoria, unidade);
        produto.Inativar();
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("5601234567890"));

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Atendente_Deve_Conseguir_Localizar()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente(PerfilUtilizador.Atendente);
        CriarAcucar(db, categoria, unidade);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("PRD-AC01"));

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task Gerente_Deve_Conseguir_Localizar()
    {
        var (db, uow, contexto, categoria, unidade) = CriarAmbiente(PerfilUtilizador.Gerente);
        CriarAcucar(db, categoria, unidade);
        var useCase = new LocalizarProdutoPorCodigoUseCase(uow, contexto);

        var resultado = await useCase.ExecutarAsync(new LocalizarProdutoPorCodigoQuery("5601234567890"));

        resultado.Should().NotBeNull();
    }
}
