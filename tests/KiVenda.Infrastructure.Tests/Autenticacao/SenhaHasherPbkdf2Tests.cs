using FluentAssertions;
using KiVenda.Infrastructure.Autenticacao;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Autenticacao;

public class SenhaHasherPbkdf2Tests
{
    [Fact]
    public void GerarHash_Nao_Deve_Devolver_A_Senha_Em_Claro()
    {
        var hasher = new SenhaHasherPbkdf2();

        var hash = hasher.GerarHash("minhaSenha123");

        hash.Should().NotBe("minhaSenha123");
        hash.Should().NotContain("minhaSenha123");
    }

    [Fact]
    public void GerarHash_Deve_Produzir_Hashes_Diferentes_Para_A_Mesma_Senha()
    {
        // O salt é aleatório a cada chamada, por isso o mesmo texto
        // produz sempre um hash diferente — essencial para impedir
        // ataques de rainbow table.
        var hasher = new SenhaHasherPbkdf2();

        var hash1 = hasher.GerarHash("minhaSenha123");
        var hash2 = hasher.GerarHash("minhaSenha123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verificar_Com_A_Senha_Correta_Deve_Devolver_Verdadeiro()
    {
        var hasher = new SenhaHasherPbkdf2();
        var hash = hasher.GerarHash("minhaSenha123");

        hasher.Verificar("minhaSenha123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verificar_Com_Senha_Errada_Deve_Devolver_Falso()
    {
        var hasher = new SenhaHasherPbkdf2();
        var hash = hasher.GerarHash("minhaSenha123");

        hasher.Verificar("senhaErrada", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("formato-invalido")]
    [InlineData("100000.saltnaobase64!!!.hash")]
    [InlineData("")]
    public void Verificar_Com_Hash_Em_Formato_Invalido_Deve_Devolver_Falso_Sem_Lancar_Excecao(string hashInvalido)
    {
        var hasher = new SenhaHasherPbkdf2();

        var resultado = hasher.Verificar("qualquerSenha", hashInvalido);

        resultado.Should().BeFalse();
    }
}
