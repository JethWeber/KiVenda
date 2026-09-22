using FluentAssertions;
using KiVenda.Application.Auditoria;
using KiVenda.Application.Exceptions;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Enums;
using Xunit;

namespace KiVenda.Application.Tests.Auditoria;

public class ConsultarAuditoriaUseCaseTests
{
    [Fact]
    public async Task Gerente_Deve_Consultar_Auditoria_Com_Filtros()
    {
        var db = new InMemoryDatabase();
        var utilizadorId = Guid.NewGuid();
        var outroUtilizadorId = Guid.NewGuid();

        db.LogsAuditoria.Add(new LogAuditoria(utilizadorId, "Alterou preço", "Produto", Guid.NewGuid()));
        db.LogsAuditoria.Add(new LogAuditoria(outroUtilizadorId, "Sangria de caixa", "SessaoCaixa", Guid.NewGuid()));

        var contexto = new FakeContextoAutenticacao
        {
            UtilizadorId = utilizadorId,
            Perfil = PerfilUtilizador.Gerente
        };

        var useCase = new ConsultarAuditoriaUseCase(new InMemoryUnitOfWork(db), contexto);

        var resultado = await useCase.ExecutarAsync(
            new ConsultarAuditoriaQuery(UtilizadorId: utilizadorId, Acao: "Alterou preço"));

        resultado.Should().ContainSingle();
        resultado[0].UtilizadorId.Should().Be(utilizadorId);
        resultado[0].Acao.Should().Be("Alterou preço");
    }

    [Fact]
    public async Task Atendente_Nao_Deve_Consultar_Auditoria()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var useCase = new ConsultarAuditoriaUseCase(new InMemoryUnitOfWork(db), contexto);

        var acao = () => useCase.ExecutarAsync(new ConsultarAuditoriaQuery());

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }

    [Fact]
    public async Task Deve_Rejeitar_Periodo_Invertido()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var useCase = new ConsultarAuditoriaUseCase(new InMemoryUnitOfWork(db), contexto);

        var inicio = DateTime.UtcNow;
        var acao = () => useCase.ExecutarAsync(
            new ConsultarAuditoriaQuery(De: inicio, Ate: inicio.AddMinutes(-1)));

        await acao.Should().ThrowAsync<KiVenda.Core.Exceptions.DomainException>();
    }
}
