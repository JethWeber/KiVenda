using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using Xunit;

namespace KiVenda.Core.Tests.Utilizadores;

public class PermissoesTests
{
    [Theory]
    [InlineData(Acao.ConfigurarSistema)]
    [InlineData(Acao.CadastrarProdutos)]
    [InlineData(Acao.AjustarStock)]
    [InlineData(Acao.AcederRelatorios)]
    [InlineData(Acao.CriarUtilizadores)]
    [InlineData(Acao.RealizarBackup)]
    [InlineData(Acao.RegistarCompras)]
    [InlineData(Acao.GerirCaixa)]
    [InlineData(Acao.FazerVenda)]
    [InlineData(Acao.ConsultarProdutosStockClientes)]
    public void Gerente_Deve_Poder_Executar_Todas_As_Acoes(Acao acao)
    {
        Permissoes.Permite(PerfilUtilizador.Gerente, acao).Should().BeTrue();
    }

    [Theory]
    [InlineData(Acao.FazerVenda, true)]
    [InlineData(Acao.ConsultarProdutosStockClientes, true)]
    [InlineData(Acao.ConfigurarSistema, false)]
    [InlineData(Acao.CadastrarProdutos, false)]
    [InlineData(Acao.AjustarStock, false)]
    [InlineData(Acao.AcederRelatorios, false)]
    [InlineData(Acao.CriarUtilizadores, false)]
    [InlineData(Acao.RealizarBackup, false)]
    [InlineData(Acao.RegistarCompras, false)]
    [InlineData(Acao.GerirCaixa, false)]
    public void Atendente_So_Deve_Poder_Vender_E_Consultar(Acao acao, bool esperado)
    {
        Permissoes.Permite(PerfilUtilizador.Atendente, acao).Should().Be(esperado);
    }

    [Fact]
    public void Utilizador_PodeExecutar_Deve_Respeitar_O_Perfil()
    {
        var gerente = new Utilizador("Maria", "maria", "hash-qualquer", PerfilUtilizador.Gerente);
        var atendente = new Utilizador("João", "joao", "hash-qualquer", PerfilUtilizador.Atendente);

        gerente.PodeExecutar(Acao.CriarUtilizadores).Should().BeTrue();
        atendente.PodeExecutar(Acao.CriarUtilizadores).Should().BeFalse();
    }

    [Fact]
    public void Utilizador_Inativo_Nunca_Deve_Poder_Executar_Nenhuma_Acao()
    {
        var gerente = new Utilizador("Maria", "maria", "hash-qualquer", PerfilUtilizador.Gerente);
        gerente.Inativar();

        gerente.PodeExecutar(Acao.FazerVenda).Should().BeFalse();
    }
}
