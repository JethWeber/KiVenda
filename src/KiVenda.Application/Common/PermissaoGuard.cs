using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Common;

/// <summary>
/// Ponto único onde os casos de uso verificam permissão, para nunca
/// duplicarem a lógica de "quem pode o quê" — essa lógica já vive em
/// <see cref="Permissoes"/> (Fase 1); isto é só o "cofre" que a aplica.
/// </summary>
public static class PermissaoGuard
{
    public static void Exigir(IContextoAutenticacao contexto, Acao acao)
    {
        if (!Permissoes.Permite(contexto.Perfil, acao))
        {
            throw new PermissaoNegadaException(acao);
        }
    }
}
