using FluentAssertions;
using KiVenda.Application.Caixa;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Application.Vendas;
using KiVenda.Core.Caixa;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using KiVenda.Core.Vendas;
using Xunit;

namespace KiVenda.Application.Tests.Vendas;

public class FinalizarVendaUseCaseTests
{
    private sealed record Ambiente(InMemoryDatabase Db, InMemoryUnitOfWork Uow, FakeContextoAutenticacao Contexto, Produto Produto, ApresentacaoProduto Apresentacao1Kg, SessaoCaixa Sessao);

    private static async Task<Ambiente> CriarAmbienteComVendaEmAndamentoAsync(decimal quantidadeVendidaKg = 2)
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidade.Id, precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);
        var apresentacao1kg = produto.AdicionarApresentacao("1 kg", 1000m);
        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), contexto.UtilizadorId);
        db.Produtos.Add(produto);

        var sessao = new SessaoCaixa(contexto.UtilizadorId, saldoInicial: 20000m);
        db.SessoesCaixa.Add(sessao);

        var uow = new InMemoryUnitOfWork(db);

        var iniciarVenda = new IniciarVendaUseCase(uow, contexto);
        var vendaId = await iniciarVenda.ExecutarAsync(new IniciarVendaCommand());

        var adicionarItem = new AdicionarItemVendaUseCase(uow, contexto);
        await adicionarItem.ExecutarAsync(new AdicionarItemVendaCommand(vendaId, produto.Id, apresentacao1kg.Id, quantidadeVendidaKg));

        return new Ambiente(db, uow, contexto, produto, apresentacao1kg, sessao);
    }

    [Fact]
    public async Task FinalizarVenda_Deve_Dar_Saida_De_Stock_Registar_Entrada_De_Caixa_E_Auditoria()
    {
        var ambiente = await CriarAmbienteComVendaEmAndamentoAsync(quantidadeVendidaKg: 2); // total esperado: 3000 Kz, 2000 g

        var venda = ambiente.Db.Vendas.Single();
        var useCase = new FinalizarVendaUseCase(ambiente.Uow, ambiente.Contexto);

        var recibo = await useCase.ExecutarAsync(new FinalizarVendaCommand(
            venda.Id,
            new[] { new PagamentoCommand(MetodoPagamento.Dinheiro, 3000m) }));

        // 1. Stock deu saída (25000 - 2000 = 23000)
        ambiente.Produto.EstoqueAtual.Should().Be(23000m);
        ambiente.Db.MovimentosStock.Should().ContainSingle(m => m.Tipo == TipoMovimentoStock.Saida && m.OrigemId == venda.Id);

        // 2. Caixa recebeu a entrada da venda
        ambiente.Sessao.SaldoCalculado.Should().Be(23000m); // 20000 + 3000

        // 3. Auditoria da venda foi registada
        ambiente.Db.LogsAuditoria.Should().ContainSingle(l => l.Acao == "Venda realizada" && l.EntidadeId == venda.Id);

        // 4. Recibo devolvido está coerente
        recibo.Total.Should().Be(3000m);
        recibo.Itens.Should().ContainSingle(i => i.ProdutoNome == "Açúcar" && i.ApresentacaoNome == "1 kg");
    }

    [Fact]
    public async Task FinalizarVenda_Com_Pagamento_Insuficiente_Nao_Deve_Alterar_Stock_Nem_Caixa()
    {
        var ambiente = await CriarAmbienteComVendaEmAndamentoAsync(quantidadeVendidaKg: 2); // total 3000 Kz
        var venda = ambiente.Db.Vendas.Single();
        var useCase = new FinalizarVendaUseCase(ambiente.Uow, ambiente.Contexto);

        var acao = async () => await useCase.ExecutarAsync(new FinalizarVendaCommand(
            venda.Id,
            new[] { new PagamentoCommand(MetodoPagamento.Dinheiro, 1000m) }));

        await acao.Should().ThrowAsync<DomainException>().WithMessage("*Pagamento insuficiente*");

        // Nada deve ter sido alterado: a validação do Core falha antes de
        // qualquer efeito colateral (saída de stock, entrada de caixa).
        ambiente.Produto.EstoqueAtual.Should().Be(25000m);
        ambiente.Sessao.SaldoCalculado.Should().Be(20000m);
        ambiente.Db.MovimentosStock.Should().BeEmpty();
    }

    [Fact]
    public async Task FinalizarVenda_Deve_Calcular_Lucro_A_Partir_Do_Custo_Medio_Ponderado()
    {
        // Preço de venda 1,5 Kz/g, custo médio ponderado 1 Kz/g (única
        // entrada no setup) -> lucro esperado por grama: 0,5 Kz.
        var ambiente = await CriarAmbienteComVendaEmAndamentoAsync(quantidadeVendidaKg: 2); // 2000 g
        var venda = ambiente.Db.Vendas.Single();
        var useCase = new FinalizarVendaUseCase(ambiente.Uow, ambiente.Contexto);

        var recibo = await useCase.ExecutarAsync(new FinalizarVendaCommand(
            venda.Id,
            new[] { new PagamentoCommand(MetodoPagamento.Dinheiro, 3000m) }));

        recibo.LucroEstimado.Should().Be(1000m); // 2000g * (1.5 - 1.0)
    }
}
