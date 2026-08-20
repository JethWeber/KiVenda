using FluentAssertions;
using Xunit;

namespace KiVenda.Application.Tests;

/// <summary>
/// Testes de fumo (smoke tests) da Fase 0. Serão substituídos pelos
/// testes reais dos casos de uso na Fase 3.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Projeto_KiVenda_Application_Tests_Compila_E_Executa()
    {
        var resultado = 1 + 1;

        resultado.Should().Be(2);
    }
}
