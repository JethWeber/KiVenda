using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Utilizadores;

public sealed record DefinirPerfilCommand(Guid UtilizadorId, PerfilUtilizador NovoPerfil);

public sealed class DefinirPerfilUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(DefinirPerfilCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CriarUtilizadores);

        var utilizador = await uow.Utilizadores.ObterPorIdAsync(comando.UtilizadorId, cancellationToken)
            ?? throw new DomainException("Utilizador não encontrado.");

        var perfilAnterior = utilizador.Perfil;
        utilizador.DefinirPerfil(comando.NovoPerfil);

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(
                contexto.UtilizadorId,
                "Alterou perfil de utilizador",
                "Utilizador",
                utilizador.Id,
                dadosAntes: perfilAnterior.ToString(),
                dadosDepois: comando.NovoPerfil.ToString()),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
