using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KiVenda.Infrastructure.Licenciamento;

/// <summary>
/// Gera pares de chaves e ficheiros de licença assinados **apenas para
/// desenvolvimento e testes** — nunca deve ser usado para emitir
/// licenças reais a clientes (essa responsabilidade é do sistema de
/// licenciamento corporativo da Weber Tech, fora do âmbito deste
/// projeto). Existe para que <see cref="ServicoLicenciamentoRsa"/> possa
/// ser testado de ponta a ponta sem depender da chave privada real.
/// </summary>
public static class FerramentasLicencaDeTeste
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    public static RSA CriarParDeChaves() => RSA.Create(2048);

    /// <summary>Gera o conteúdo (string JSON) de um ficheiro `.wta` de teste, assinado com a chave privada fornecida.</summary>
    public static string AssinarLicencaDeTeste(RSA chavePrivada, string nomeCliente, DateTime? dataExpiracao = null)
    {
        var payload = new PayloadLicenca(nomeCliente, DateTime.UtcNow, dataExpiracao);
        var payloadJson = JsonSerializer.Serialize(payload, OpcoesJson);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

        var assinatura = chavePrivada.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var envelope = new EnvelopeLicenca(Convert.ToBase64String(payloadBytes), Convert.ToBase64String(assinatura));

        return JsonSerializer.Serialize(envelope, OpcoesJson);
    }
}
