using FluentAssertions;
using Xunit;

namespace KiVenda.Core.Tests;

/// <summary>
/// Testes de fumo (smoke tests) da Fase 0: garantem apenas que o projeto
/// de testes está corretamente referenciado e o pipeline de testes corre.
/// Serão substituídos pelos testes reais das entidades/regras de negócio
/// na Fase 1 e removidos.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Projeto_KiVenda_Core_Tests_Compila_E_Executa()
    {
        var resultado = 1 + 1;

        resultado.Should().Be(2);
    }
}
