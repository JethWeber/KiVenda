using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Application.Abstractions.Auth;
using KiVenda.Core.Enums;

namespace KiVenda.Desktop.Autenticacao;

/// <summary>
/// Implementação concreta de <see cref="IContextoAutenticacao"/> (contrato
/// definido pela Application, Fase 3) para o Desktop: guarda o
/// utilizador autenticado em memória durante a execução da aplicação,
/// sem qualquer persistência entre sessões — sair da aplicação obriga
/// sempre a novo login (Secção 3: sem login online, mas com login local
/// obrigatório a cada arranque).
///
/// Registada como singleton no composition root (App.axaml.cs), para
/// que todos os casos de uso resolvidos durante a sessão vejam sempre o
/// mesmo utilizador autenticado.
/// </summary>
public sealed partial class SessaoUtilizadorAtual : ObservableObject, IContextoAutenticacao
{
    [ObservableProperty]
    private bool _autenticado;

    [ObservableProperty]
    private string _nome = string.Empty;

    public Guid UtilizadorId { get; private set; }

    public PerfilUtilizador Perfil { get; private set; }

    public void IniciarSessao(Guid utilizadorId, string nome, PerfilUtilizador perfil)
    {
        UtilizadorId = utilizadorId;
        Nome = nome;
        Perfil = perfil;
        Autenticado = true;
    }

    public void TerminarSessao()
    {
        UtilizadorId = Guid.Empty;
        Nome = string.Empty;
        Autenticado = false;
    }
}
