namespace KiVenda.Infrastructure.Licenciamento;

public sealed record ResultadoValidacaoLicenca(
    bool Valida,
    string? MensagemErro,
    string? NomeCliente,
    DateTime? DataAtivacao,
    DateTime? DataExpiracao);
