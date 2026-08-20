using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Application.Utilizadores;

public sealed record AutenticarUtilizadorCommand(string NomeUtilizador, string Senha);

public sealed record UtilizadorAutenticadoDto(Guid UtilizadorId, string Nome, PerfilUtilizador Perfil);

/// <summary>
/// Fluxo de login local (Secção 3: sem servidor, sem internet). Não
/// depende de <see cref="IContextoAutenticacao"/> — é precisamente este
/// caso de uso que a Desktop (Fase 5/6) usa para construir esse
/// contexto depois de autenticar. Devolve sempre a mesma mensagem de
/// erro genérica para nome de utilizador inexistente, utilizador
/// inativo ou password incorreta, para não revelar qual delas falhou.
/// </summary>
public sealed class AutenticarUtilizadorUseCase(IUnitOfWork uow, ISenhaHasher senhaHasher)
{
    private const string MensagemErroGenerica = "Utilizador ou password inválidos.";

    public async Task<UtilizadorAutenticadoDto> ExecutarAsync(AutenticarUtilizadorCommand comando, CancellationToken cancellationToken = default)
    {
        var utilizador = await uow.Utilizadores.ObterPorNomeUtilizadorAsync(comando.NomeUtilizador, cancellationToken);

        if (utilizador is null || !utilizador.Ativo)
        {
            throw new DomainException(MensagemErroGenerica);
        }

        if (!senhaHasher.Verificar(comando.Senha, utilizador.PasswordHash))
        {
            throw new DomainException(MensagemErroGenerica);
        }

        return new UtilizadorAutenticadoDto(utilizador.Id, utilizador.Nome, utilizador.Perfil);
    }
}
