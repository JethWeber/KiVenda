using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Utilizadores;

public sealed record CriarUtilizadorCommand(string Nome, string NomeUtilizador, string Senha, PerfilUtilizador Perfil);

public sealed class CriarUtilizadorUseCase(IUnitOfWork uow, IContextoAutenticacao contexto, ISenhaHasher senhaHasher)
{
    public async Task<Guid> ExecutarAsync(CriarUtilizadorCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CriarUtilizadores);

        var existente = await uow.Utilizadores.ObterPorNomeUtilizadorAsync(comando.NomeUtilizador, cancellationToken);
        if (existente is not null)
        {
            throw new DomainException($"Já existe um utilizador com o nome de utilizador \"{comando.NomeUtilizador}\".");
        }

        var hash = senhaHasher.GerarHash(comando.Senha);
        var utilizador = new Utilizador(comando.Nome, comando.NomeUtilizador, hash, comando.Perfil);

        await uow.Utilizadores.AdicionarAsync(utilizador, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return utilizador.Id;
    }
}
