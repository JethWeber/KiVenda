using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Application.Exceptions;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Utilizadores;

public sealed record AlterarPasswordCommand(Guid UtilizadorId, string NovaSenha);

/// <summary>
/// Um utilizador pode sempre alterar a própria password, sem precisar
/// de nenhuma permissão especial. Alterar a password de OUTRO
/// utilizador é uma operação de gestão e exige
/// <see cref="Acao.CriarUtilizadores"/> (Gerente), pela mesma lógica de
/// quem pode criar/gerir contas.
/// </summary>
public sealed class AlterarPasswordUseCase(IUnitOfWork uow, IContextoAutenticacao contexto, ISenhaHasher senhaHasher)
{
    public async Task ExecutarAsync(AlterarPasswordCommand comando, CancellationToken cancellationToken = default)
    {
        var alterandoAPropria = comando.UtilizadorId == contexto.UtilizadorId;

        if (!alterandoAPropria && !Permissoes.Permite(contexto.Perfil, Acao.CriarUtilizadores))
        {
            throw new PermissaoNegadaException(Acao.CriarUtilizadores);
        }

        var utilizador = await uow.Utilizadores.ObterPorIdAsync(comando.UtilizadorId, cancellationToken)
            ?? throw new DomainException("Utilizador não encontrado.");

        var novoHash = senhaHasher.GerarHash(comando.NovaSenha);
        utilizador.AlterarPasswordHash(novoHash);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
