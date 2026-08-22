namespace KiVenda.Infrastructure.Licenciamento;

/// <summary>
/// Formato de ficheiro `.wta` usado por esta implementação de
/// referência: um envelope JSON com o payload da licença (codificado em
/// Base64) e a respetiva assinatura RSA (também em Base64), permitindo
/// verificar que o payload não foi alterado sem conhecer a chave
/// privada da Weber Tech.
///
/// ⚠️ Este é um formato de referência, criado porque o formato real do
/// `.wta` da Weber Tech está "documentado em separado" (Secção 9 da
/// documentação funcional) e não estava disponível para esta
/// implementação. Antes de qualquer distribuição a clientes reais, este
/// modelo deve ser confirmado/substituído pelo formato oficial.
/// </summary>
internal sealed record EnvelopeLicenca(string PayloadBase64, string AssinaturaBase64);

internal sealed record PayloadLicenca(string NomeCliente, DateTime DataAtivacao, DateTime? DataExpiracao);
