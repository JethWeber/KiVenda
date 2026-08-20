using FluentAssertions;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Clientes;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Core.Tests.Cadastros;

public class EntidadesBasicasTests
{
    [Fact]
    public void Cliente_Deve_Exigir_Nome()
    {
        var acao = () => new Cliente(nome: "  ");

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cliente_Telefone_E_Opcional()
    {
        var cliente = new Cliente("Maria João");

        cliente.Telefone.Should().BeNull();
    }

    [Fact]
    public void Fornecedor_Deve_Exigir_Nome()
    {
        var acao = () => new Fornecedor(nome: "");

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Categoria_Deve_Exigir_Nome()
    {
        var acao = () => new Categoria(nome: "   ");

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void UnidadeMedida_Deve_Exigir_Codigo_E_Nome()
    {
        var acao = () => new UnidadeMedida(codigo: "", nome: "Grama");

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void LogAuditoria_Deve_Exigir_Utilizador_Acao_E_Entidade_Afetada()
    {
        var acao = () => new LogAuditoria(Guid.Empty, "Venda realizada", "Venda");

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void LogAuditoria_Valido_Deve_Registar_Data_Automaticamente()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var log = new LogAuditoria(Guid.NewGuid(), "Alterou preço", "Produto", Guid.NewGuid(), dadosAntes: "650 Kz", dadosDepois: "700 Kz");

        log.DataHora.Should().BeOnOrAfter(antes);
        log.DadosAntes.Should().Be("650 Kz");
        log.DadosDepois.Should().Be("700 Kz");
    }
}
