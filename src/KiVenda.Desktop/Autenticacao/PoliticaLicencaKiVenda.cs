using WeberTech.Licensing.Enums;
using WeberTech.Licensing.Models;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop.Autenticacao;

public enum AcessoLicenca
{
    Liberado,
    Aviso,
    Tolerancia,
    Bloqueado
}

public sealed record EstadoLicencaKiVenda(
    AcessoLicenca Acesso,
    LicenseStatus Estado,
    int DiasRestantes,
    string Titulo,
    string Mensagem)
{
    public bool MostrarBanner => Acesso is AcessoLicenca.Aviso or AcessoLicenca.Tolerancia or AcessoLicenca.Bloqueado;
    public bool Bloqueado => Acesso == AcessoLicenca.Bloqueado;
}

public static class PoliticaLicencaKiVenda
{
    private const int DiasAviso = 14;
    private const int DiasTolerancia = 7;

    public static EstadoLicencaKiVenda Avaliar()
    {
        LicenseInfo? info = Licensing.GetLicenseInfo();
        LicenseStatus estado = Licensing.CurrentStatus;

        if (estado == LicenseStatus.Valid)
        {
            if (info?.ExpiresAt is not { } expiracao)
                return new(AcessoLicenca.Liberado, estado, -1, string.Empty, string.Empty);

            int dias = (expiracao.Date - DateTime.UtcNow.Date).Days;

            if (dias > DiasAviso)
                return new(AcessoLicenca.Liberado, estado, dias, string.Empty, string.Empty);

            if (dias >= 0)
                return new(
                    AcessoLicenca.Aviso,
                    estado,
                    dias,
                    "A licença do KiVenda está a expirar",
                    $"A sua licença expira em {dias} dia(s), em {expiracao.ToLocalTime():dd/MM/yyyy}. Renove antes da data de expiração para evitar a interrupção do sistema.");

            // O SDK marca como Expired durante esta fase; a política aplica a tolerância.
            int diasTolerancia = DiasTolerancia - Math.Max(0, (DateTime.UtcNow.Date - expiracao.Date).Days);
            if (diasTolerancia > 0)
                return new(
                    AcessoLicenca.Tolerancia,
                    LicenseStatus.Expired,
                    diasTolerancia,
                    "Período de tolerância da licença",
                    $"A licença expirou em {expiracao.ToLocalTime():dd/MM/yyyy}. Restam {diasTolerancia} dia(s) de tolerância para renovar.");

            return Bloqueado(expiracao);
        }

        if (estado == LicenseStatus.Expired && info?.ExpiresAt is { } expiracaoExpirada)
        {
            int diasDesdeExpiracao = Math.Max(0, (DateTime.UtcNow.Date - expiracaoExpirada.Date).Days);
            int toleranciaRestante = DiasTolerancia - diasDesdeExpiracao;

            if (toleranciaRestante > 0)
                return new(
                    AcessoLicenca.Tolerancia,
                    estado,
                    toleranciaRestante,
                    "Período de tolerância da licença",
                    $"A licença expirou em {expiracaoExpirada.ToLocalTime():dd/MM/yyyy}. Restam {toleranciaRestante} dia(s) de tolerância para renovar.");

            return Bloqueado(expiracaoExpirada);
        }

        return estado switch
        {
            LicenseStatus.NotFound => new(AcessoLicenca.Bloqueado, estado, -1,
                "Licença não encontrada",
                "O KiVenda não possui uma licença ativa. Abra Configurações → Licença para ativar este computador."),
            LicenseStatus.Invalid => new(AcessoLicenca.Bloqueado, estado, -1,
                "Licença inválida",
                "O ficheiro de licença é inválido ou foi adulterado. Importe uma licença válida em Configurações → Licença."),
            LicenseStatus.ProductMismatch => new(AcessoLicenca.Bloqueado, estado, -1,
                "Licença incompatível",
                "A licença instalada não pertence a esta versão do KiVenda."),
            LicenseStatus.MachineMismatch => new(AcessoLicenca.Bloqueado, estado, -1,
                "Máquina não autorizada",
                "A licença instalada pertence a outra máquina. Solicite uma nova ativação."),
            _ => new(AcessoLicenca.Bloqueado, estado, -1,
                "Licença indisponível",
                "O KiVenda não pode continuar sem uma licença válida.")
        };
    }

    private static EstadoLicencaKiVenda Bloqueado(DateTime expiracao) =>
        new(
            AcessoLicenca.Bloqueado,
            LicenseStatus.Expired,
            0,
            "Licença expirada",
            $"A licença expirou em {expiracao.ToLocalTime():dd/MM/yyyy} e o período de tolerância terminou. Renove a licença em Configurações → Licença.");
}
