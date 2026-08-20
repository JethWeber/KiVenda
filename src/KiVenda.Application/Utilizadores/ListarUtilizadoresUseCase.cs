using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Utilizadores;

public sealed record ListarUtilizadoresQuery(bool ApenasAtivos = true);

public sealed record UtilizadorDto(Guid Id, string Nome, string NomeUtilizador, PerfilUtilizador Perfil, bool Ativo);

public sealed class ListarUtilizadoresUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<UtilizadorDto>> ExecutarAsync(ListarUtilizadoresQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CriarUtilizadores);

        var utilizadores = await uow.Utilizadores.ListarAsync(query.ApenasAtivos, cancellationToken);

        return utilizadores
            .Select(u => new UtilizadorDto(u.Id, u.Nome, u.NomeUtilizador, u.Perfil, u.Ativo))
            .ToList();
    }
}
