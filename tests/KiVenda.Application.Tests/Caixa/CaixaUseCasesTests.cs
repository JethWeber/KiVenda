using FluentAssertions;
using KiVenda.Application.Caixa;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using Xunit;

namespace KiVenda.Application.Tests.Caixa;

public class CaixaUseCasesTests
{
    [Fact]
    public async Task AbrirCaixa_Deve_Criar_Sessao_Quando_Nao_Ha_Nenhuma_Aberta()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var useCase = new AbrirCaixaUseCase(new InMemoryUnitOfWork(db), contexto);

        var sessaoId = await useCase.ExecutarAsync(new AbrirCaixaCommand(20000m));

        db.SessoesCaixa.Should().ContainSingle(s => s.Id == sessaoId && s.SaldoInicial == 20000m);
    }

    [Fact]
    public async Task AbrirCaixa_Deve_Rejeitar_Segunda_Sessao_Enquanto_A_Primeira_Estiver_Aberta()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new AbrirCaixaUseCase(uow, contexto);
        await useCase.ExecutarAsync(new AbrirCaixaCommand(20000m));

        var acao = async () => await useCase.ExecutarAsync(new AbrirCaixaCommand(5000m));

        await acao.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task FecharCaixa_Deve_Calcular_Divergencia_E_Registar_Auditoria()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);

        var abrir = new AbrirCaixaUseCase(uow, contexto);
        await abrir.ExecutarAsync(new AbrirCaixaCommand(20000m));

        var suprimento = new RegistarSuprimentoUseCase(uow, contexto);
        await suprimento.ExecutarAsync(new RegistarSuprimentoCommand(50000m, "Reforço de troco"));

        var fechar = new FecharCaixaUseCase(uow, contexto);
        var resultado = await fechar.ExecutarAsync(new FecharCaixaCommand(69500m)); // esperado 70000

        resultado.Divergencia.Should().Be(-500m);
        db.LogsAuditoria.Should().ContainSingle(l => l.Acao == "Fechou caixa");
    }

    [Fact]
    public async Task RegistarSangria_Deve_Rejeitar_Valor_Maior_Que_O_Saldo_Atual()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);

        await new AbrirCaixaUseCase(uow, contexto).ExecutarAsync(new AbrirCaixaCommand(10000m));

        var sangria = new RegistarSangriaUseCase(uow, contexto);
        var acao = async () => await sangria.ExecutarAsync(new RegistarSangriaCommand(20000m));

        await acao.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Atendente_Nao_Deve_Conseguir_Abrir_Caixa()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var useCase = new AbrirCaixaUseCase(new InMemoryUnitOfWork(db), contexto);

        var acao = async () => await useCase.ExecutarAsync(new AbrirCaixaCommand(1000m));

        await acao.Should().ThrowAsync<Exceptions.PermissaoNegadaException>();
    }
}
