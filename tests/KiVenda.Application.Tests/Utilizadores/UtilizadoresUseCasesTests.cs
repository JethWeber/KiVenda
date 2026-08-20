using FluentAssertions;
using KiVenda.Application.Exceptions;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using Xunit;

namespace KiVenda.Application.Tests.Utilizadores;

public class UtilizadoresUseCasesTests
{
    [Fact]
    public async Task CriarUtilizador_Gerente_Deve_Guardar_Hash_Nao_A_Senha_Em_Claro()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var useCase = new CriarUtilizadorUseCase(new InMemoryUnitOfWork(db), contexto, new FakeSenhaHasher());

        await useCase.ExecutarAsync(new CriarUtilizadorCommand("Maria João", "maria", "senha-super-secreta", PerfilUtilizador.Atendente));

        var utilizador = db.Utilizadores.Single();
        utilizador.PasswordHash.Should().NotBe("senha-super-secreta");
        utilizador.PasswordHash.Should().Be("hash:senha-super-secreta");
    }

    [Fact]
    public async Task CriarUtilizador_Atendente_Nao_Deve_Poder_Criar_Outro_Utilizador()
    {
        var db = new InMemoryDatabase();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var useCase = new CriarUtilizadorUseCase(new InMemoryUnitOfWork(db), contexto, new FakeSenhaHasher());

        var acao = async () => await useCase.ExecutarAsync(new CriarUtilizadorCommand("João", "joao", "123", PerfilUtilizador.Atendente));

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }

    [Fact]
    public async Task AutenticarUtilizador_Com_Credenciais_Corretas_Deve_Devolver_Utilizador()
    {
        var db = new InMemoryDatabase();
        var hasher = new FakeSenhaHasher();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        await new CriarUtilizadorUseCase(new InMemoryUnitOfWork(db), contexto, hasher)
            .ExecutarAsync(new CriarUtilizadorCommand("Maria João", "maria", "senha123", PerfilUtilizador.Gerente));

        var autenticar = new AutenticarUtilizadorUseCase(new InMemoryUnitOfWork(db), hasher);
        var resultado = await autenticar.ExecutarAsync(new AutenticarUtilizadorCommand("maria", "senha123"));

        resultado.Nome.Should().Be("Maria João");
        resultado.Perfil.Should().Be(PerfilUtilizador.Gerente);
    }

    [Fact]
    public async Task AutenticarUtilizador_Com_Password_Errada_Deve_Falhar_Com_Mensagem_Generica()
    {
        var db = new InMemoryDatabase();
        var hasher = new FakeSenhaHasher();
        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        await new CriarUtilizadorUseCase(new InMemoryUnitOfWork(db), contexto, hasher)
            .ExecutarAsync(new CriarUtilizadorCommand("Maria João", "maria", "senha123", PerfilUtilizador.Gerente));

        var autenticar = new AutenticarUtilizadorUseCase(new InMemoryUnitOfWork(db), hasher);
        var acao = async () => await autenticar.ExecutarAsync(new AutenticarUtilizadorCommand("maria", "senha-errada"));

        await acao.Should().ThrowAsync<DomainException>().WithMessage("Utilizador ou password inválidos.");
    }

    [Fact]
    public async Task AutenticarUtilizador_Inexistente_Deve_Falhar_Com_A_Mesma_Mensagem_Generica()
    {
        var db = new InMemoryDatabase();
        var autenticar = new AutenticarUtilizadorUseCase(new InMemoryUnitOfWork(db), new FakeSenhaHasher());

        var acao = async () => await autenticar.ExecutarAsync(new AutenticarUtilizadorCommand("inexistente", "qualquer"));

        await acao.Should().ThrowAsync<DomainException>().WithMessage("Utilizador ou password inválidos.");
    }

    [Fact]
    public async Task AlterarPassword_Da_Propria_Conta_Nao_Exige_Permissao_Especial()
    {
        var db = new InMemoryDatabase();
        var hasher = new FakeSenhaHasher();
        var contextoGerente = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var idAtendente = Guid.NewGuid();

        await new CriarUtilizadorUseCase(uow, contextoGerente, hasher)
            .ExecutarAsync(new CriarUtilizadorCommand("João", "joao", "inicial123", PerfilUtilizador.Atendente));
        var joao = db.Utilizadores.Single();

        var contextoJoao = new FakeContextoAutenticacao { UtilizadorId = joao.Id, Perfil = PerfilUtilizador.Atendente };
        var alterarPassword = new AlterarPasswordUseCase(uow, contextoJoao, hasher);

        await alterarPassword.ExecutarAsync(new AlterarPasswordCommand(joao.Id, "novaSenha456"));

        joao.PasswordHash.Should().Be("hash:novaSenha456");
    }

    [Fact]
    public async Task AlterarPassword_De_Outro_Utilizador_Exige_Permissao_CriarUtilizadores()
    {
        var db = new InMemoryDatabase();
        var hasher = new FakeSenhaHasher();
        var contextoGerente = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);

        await new CriarUtilizadorUseCase(uow, contextoGerente, hasher)
            .ExecutarAsync(new CriarUtilizadorCommand("João", "joao", "inicial123", PerfilUtilizador.Atendente));
        var joao = db.Utilizadores.Single();

        var contextoOutroAtendente = new FakeContextoAutenticacao { UtilizadorId = Guid.NewGuid(), Perfil = PerfilUtilizador.Atendente };
        var alterarPassword = new AlterarPasswordUseCase(uow, contextoOutroAtendente, hasher);

        var acao = async () => await alterarPassword.ExecutarAsync(new AlterarPasswordCommand(joao.Id, "tentativa"));

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }
}
