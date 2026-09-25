using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Empresas;
using KiVenda.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoEmpresaViewModel : ViewModelBase
{
    private Guid? _empresaId;

    [ObservableProperty] private string _nomeComercial = string.Empty;
    [ObservableProperty] private string _razaoSocial = string.Empty;
    [ObservableProperty] private string _nif = string.Empty;
    [ObservableProperty] private string _telefone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _endereco = string.Empty;
    [ObservableProperty] private string _municipio = string.Empty;
    [ObservableProperty] private string _provincia = string.Empty;
    [ObservableProperty] private string _website = string.Empty;
    [ObservableProperty] private byte[]? _logo;
    [ObservableProperty] private string? _logoMimeType;
    [ObservableProperty] private string _mensagem = "Nenhuma empresa configurada.";
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private bool _aGuardar;

    public bool TemEmpresa => _empresaId.HasValue;
    public bool TemLogo => Logo is { Length: > 0 };
    public string EstadoLogo => TemLogo ? "Logótipo carregado." : "Sem logótipo.";

    public ConfiguracaoEmpresaViewModel() => _ = CarregarAsync();

    public async Task CarregarAsync()
    {
        ACarregar = true;
        try
        {
            await using var scope = App.Services.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ObterEmpresaUseCase>();
            var empresa = await useCase.ExecutarAsync();

            if (empresa is null)
            {
                Limpar();
                Mensagem = "Nenhuma empresa configurada. Preencha os dados para criar o cadastro.";
            }
            else
            {
                _empresaId = empresa.Id;
                NomeComercial = empresa.NomeComercial;
                RazaoSocial = empresa.RazaoSocial ?? string.Empty;
                Nif = empresa.Nif ?? string.Empty;
                Telefone = empresa.Telefone ?? string.Empty;
                Email = empresa.Email ?? string.Empty;
                Endereco = empresa.Endereco ?? string.Empty;
                Municipio = empresa.Municipio ?? string.Empty;
                Provincia = empresa.Provincia ?? string.Empty;
                Website = empresa.Website ?? string.Empty;
                Logo = empresa.Logo;
                LogoMimeType = empresa.LogoMimeType;
                Mensagem = "Dados da empresa carregados.";
            }

            OnPropertyChanged(nameof(TemEmpresa));
            OnPropertyChanged(nameof(TemLogo));
            OnPropertyChanged(nameof(EstadoLogo));
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível carregar a empresa: {ex.Message}";
        }
        finally { ACarregar = false; }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeComercial))
        {
            Mensagem = "O nome comercial é obrigatório.";
            return;
        }

        AGuardar = true;
        try
        {
            await using var scope = App.Services.CreateAsyncScope();

            if (_empresaId is null)
            {
                var useCase = scope.ServiceProvider.GetRequiredService<CriarEmpresaUseCase>();
                _empresaId = await useCase.ExecutarAsync(CriarCommand());
                Mensagem = "Empresa criada com sucesso.";
            }
            else
            {
                var useCase = scope.ServiceProvider.GetRequiredService<EditarEmpresaUseCase>();
                await useCase.ExecutarAsync(EditarCommand(_empresaId.Value));
                Mensagem = "Dados da empresa atualizados com sucesso.";
            }

            OnPropertyChanged(nameof(TemEmpresa));
        }
        catch (DomainException ex)
        {
            Mensagem = ex.Message;
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao guardar empresa: {ex.Message}";
        }
        finally { AGuardar = false; }
    }

    [RelayCommand]
    private async Task RemoverAsync()
    {
        try
        {
            await using var scope = App.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<RemoverEmpresaUseCase>().ExecutarAsync();
            Limpar();
            Mensagem = "Cadastro da empresa removido.";
            OnPropertyChanged(nameof(TemEmpresa));
            OnPropertyChanged(nameof(TemLogo));
            OnPropertyChanged(nameof(EstadoLogo));
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao remover empresa: {ex.Message}";
        }
    }

    public void DefinirLogo(byte[] bytes, string mimeType)
    {
        Logo = bytes;
        LogoMimeType = mimeType;
        OnPropertyChanged(nameof(TemLogo));
        OnPropertyChanged(nameof(EstadoLogo));
    }

    public void RemoverLogoLocal()
    {
        Logo = null;
        LogoMimeType = null;
        OnPropertyChanged(nameof(TemLogo));
        OnPropertyChanged(nameof(EstadoLogo));
    }

    private CriarEmpresaCommand CriarCommand() =>
        new(NomeComercial, Vazio(RazaoSocial), Vazio(Nif), Vazio(Telefone), Vazio(Email),
            Vazio(Endereco), Vazio(Municipio), Vazio(Provincia), Vazio(Website), Logo, LogoMimeType);

    private EditarEmpresaCommand EditarCommand(Guid id) =>
        new(id, NomeComercial, Vazio(RazaoSocial), Vazio(Nif), Vazio(Telefone), Vazio(Email),
            Vazio(Endereco), Vazio(Municipio), Vazio(Provincia), Vazio(Website), Logo, LogoMimeType);

    private static string? Vazio(string valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private void Limpar()
    {
        _empresaId = null;
        NomeComercial = RazaoSocial = Nif = Telefone = Email = Endereco = Municipio = Provincia = Website = string.Empty;
        Logo = null;
        LogoMimeType = null;
    }
}
