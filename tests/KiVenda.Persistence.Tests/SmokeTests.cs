using FluentAssertions;
using Xunit;

namespace KiVenda.Persistence.Tests;

/// <summary>
/// Testes de fumo (smoke tests) da Fase 0. Serão substituídos pelos
/// testes de integração com SQLite na Fase 2.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Projeto_KiVenda_Persistence_Tests_Compila_E_Executa()
    {
        var resultado = 1 + 1;

        resultado.Should().Be(2);
    }
}
