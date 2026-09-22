using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Application.Exceptions;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Auditoria;

public sealed record ConsultarAuditoriaQuery(
    Guid? UtilizadorId = null,
    string? EntidadeAfetada = null,
    string? Acao = null,
    DateTime? De = null,
    DateTime? Ate = null,
    int Pagina = 1,
    int TamanhoPagina = 50);

public sealed class ConsultarAuditoriaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<LogAuditoria>> ExecutarAsync(
        ConsultarAuditoriaQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!Permissoes.Permite(contexto.Perfil, Acao.ConfigurarSistema))
        {
            throw new PermissaoNegadaException(Acao.ConfigurarSistema);
        }

        if (query.Pagina < 1)
        {
            throw new DomainException("A página da auditoria deve ser maior que zero.");
        }

        if (query.TamanhoPagina is < 1 or > 200)
        {
            throw new DomainException("O tamanho da página da auditoria deve estar entre 1 e 200.");
        }

        if (query.De.HasValue && query.Ate.HasValue && query.De > query.Ate)
        {
            throw new DomainException("O início do período não pode ser posterior ao fim.");
        }

        return await uow.LogsAuditoria.ListarAsync(
            query.UtilizadorId,
            query.EntidadeAfetada,
            query.Acao,
            query.De,
            query.Ate,
            query.Pagina,
            query.TamanhoPagina,
            cancellationToken);
    }
}
