using KiVenda.Core.Enums;

namespace KiVenda.Application.Abstractions.Auth;

/// <summary>
/// Representa o utilizador autenticado na sessão atual da aplicação
/// Desktop. Implementado pela Infrastructure/Desktop (Fase 4/5, a
/// partir do login local), e injetado nos casos de uso que precisam de
/// saber "quem está a fazer isto" — tanto para associar vendas/movimentos
/// ao utilizador certo, como para verificar permissões via
/// <see cref="KiVenda.Core.Utilizadores.Permissoes"/>.
/// </summary>
public interface IContextoAutenticacao
{
    Guid UtilizadorId { get; }

    PerfilUtilizador Perfil { get; }
}
