using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KiVenda.Infrastructure.Licenciamento;

/// <summary>
/// Implementação de referência: valida o ficheiro de licença verificando
/// uma assinatura RSA sobre o payload (ver <see cref="EnvelopeLicenca"/>
/// para o aviso sobre o formato real da Weber Tech ainda não estar
/// disponível para esta implementação).
///
/// Recebe a chave pública via construtor (nunca hardcoded), para que a
/// composição final (Fase 6, Desktop) possa injetar a chave pública
/// real da Weber Tech assim que estiver disponível, sem alterar esta
/// classe.
/// </summary>
public sealed class ServicoLicenciamentoRsa : IServicoLicenciamento
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    private readonly string _caminhoFicheiroLicenca;
    private readonly RSA _chavePublica;

    public ServicoLicenciamentoRsa(string caminhoFicheiroLicenca, RSA chavePublicaWeberTech)
    {
        _caminhoFicheiroLicenca = caminhoFicheiroLicenca;
        _chavePublica = chavePublicaWeberTech;
    }

    public async Task<ResultadoValidacaoLicenca> ValidarLicencaAtualAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_caminhoFicheiroLicenca))
        {
            return Reprovada("Nenhuma licença ativada nesta instalação.");
        }

        var conteudo = await File.ReadAllTextAsync(_caminhoFicheiroLicenca, cancellationToken);
        return ValidarConteudo(conteudo);
    }

    public async Task<ResultadoValidacaoLicenca> AtivarLicencaAsync(string caminhoFicheiroLicenca, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoFicheiroLicenca))
        {
            return Reprovada("Ficheiro de licença não encontrado.");
        }

        var conteudo = await File.ReadAllTextAsync(caminhoFicheiroLicenca, cancellationToken);
        var resultado = ValidarConteudo(conteudo);

        if (resultado.Valida)
        {
            var diretorio = Path.GetDirectoryName(_caminhoFicheiroLicenca);
            if (!string.IsNullOrEmpty(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }

            File.Copy(caminhoFicheiroLicenca, _caminhoFicheiroLicenca, overwrite: true);
        }

        return resultado;
    }

    private ResultadoValidacaoLicenca ValidarConteudo(string conteudoJson)
    {
        EnvelopeLicenca? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<EnvelopeLicenca>(conteudoJson, OpcoesJson);
        }
        catch (JsonException)
        {
            return Reprovada("Ficheiro de licença corrompido ou em formato inválido.");
        }

        if (envelope is null)
        {
            return Reprovada("Ficheiro de licença corrompido ou em formato inválido.");
        }

        byte[] payloadBytes;
        byte[] assinaturaBytes;
        try
        {
            payloadBytes = Convert.FromBase64String(envelope.PayloadBase64);
            assinaturaBytes = Convert.FromBase64String(envelope.AssinaturaBase64);
        }
        catch (FormatException)
        {
            return Reprovada("Ficheiro de licença corrompido ou em formato inválido.");
        }

        var assinaturaValida = _chavePublica.VerifyData(payloadBytes, assinaturaBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!assinaturaValida)
        {
            return Reprovada("Assinatura da licença inválida — o ficheiro pode ter sido alterado.");
        }

        PayloadLicenca? payload;
        try
        {
            payload = JsonSerializer.Deserialize<PayloadLicenca>(Encoding.UTF8.GetString(payloadBytes), OpcoesJson);
        }
        catch (JsonException)
        {
            return Reprovada("Payload da licença corrompido.");
        }

        if (payload is null)
        {
            return Reprovada("Payload da licença corrompido.");
        }

        if (payload.DataExpiracao.HasValue && payload.DataExpiracao.Value < DateTime.UtcNow)
        {
            return new ResultadoValidacaoLicenca(false, "Licença expirada.", payload.NomeCliente, payload.DataAtivacao, payload.DataExpiracao);
        }

        return new ResultadoValidacaoLicenca(true, null, payload.NomeCliente, payload.DataAtivacao, payload.DataExpiracao);
    }

    private static ResultadoValidacaoLicenca Reprovada(string mensagem) => new(false, mensagem, null, null, null);
}
