using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using KiVenda.Desktop.Autenticacao;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// Ecrã mostrado imediatamente depois de um login bem-sucedido.
/// Provisório — a shell definitiva (menu lateral com os 10 módulos,
/// navegação entre eles) é construída na Fase 6. Serve aqui para provar
/// de ponta a ponta que: (1) o login funciona, (2) a sessão fica
/// disponível para toda a aplicação, e (3) a UI consegue esconder
/// opções com base no perfil, consultando sempre
/// <see cref="Permissoes"/> — nunca duplicando a regra localmente.
/// </summary>
public partial class BemVindoViewModel : ViewModelBase
{
    private readonly SessaoUtilizadorAtual _sessao;

    public string NomeUtilizador { get; }

    public string Perfil { get; }

    /// <summary>
    /// Exemplo do padrão a repetir na Fase 6 para cada item do menu:
    /// a UI nunca decide sozinha quem vê o quê — pergunta sempre à
    /// mesma matriz de permissões usada pelos casos de uso.
    /// </summary>
    public bool PodeAcederConfiguracoes { get; }

    public bool PodeGerirCaixa { get; }

    public bool PodeAcederRelatorios { get; }

    public event EventHandler? SessaoTerminada;

    public BemVindoViewModel(UtilizadorAutenticadoDto utilizador, SessaoUtilizadorAtual sessao)
    {
        _sessao = sessao;

        NomeUtilizador = utilizador.Nome;
        Perfil = utilizador.Perfil == PerfilUtilizador.Gerente ? "Gerente" : "Atendente";

        PodeAcederConfiguracoes = Permissoes.Permite(utilizador.Perfil, Acao.ConfigurarSistema);
        PodeGerirCaixa = Permissoes.Permite(utilizador.Perfil, Acao.GerirCaixa);
        PodeAcederRelatorios = Permissoes.Permite(utilizador.Perfil, Acao.AcederRelatorios);
    }

    [RelayCommand]
    private void TerminarSessao()
    {
        _sessao.TerminarSessao();
        SessaoTerminada?.Invoke(this, EventArgs.Empty);
    }
}
