using System.Text.Json;
using System.Text.Json.Nodes;

namespace KiVenda.Infrastructure.Configuracao;

/// <summary>
/// Guarda todas as chaves de configuração num único ficheiro JSON local
/// (ex.: <c>%APPDATA%\KiVenda\configuracao.json</c>). Suficiente para o
/// volume e a frequência de escrita deste tipo de dado — não faz
/// sentido usar SQLite/EF Core só para isto.
///
/// Cada instância desta classe serializa o acesso ao ficheiro com um
/// <see cref="SemaphoreSlim"/> para evitar leituras/escritas
/// concorrentes corromperem o JSON (ex.: guardar a configuração do
/// scanner e do tema ao mesmo tempo).
/// </summary>
public sealed class ArmazenamentoConfiguracaoLocalJson : IArmazenamentoConfiguracaoLocal
{
    private readonly string _caminhoFicheiro;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public ArmazenamentoConfiguracaoLocalJson(string caminhoFicheiro)
    {
        _caminhoFicheiro = caminhoFicheiro;
    }

    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default)
    {
        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            var raiz = await CarregarAsync(cancellationToken);
            var no = raiz[chave];

            return no is null ? default : no.Deserialize<T>();
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public async Task GuardarAsync<T>(string chave, T valor, CancellationToken cancellationToken = default)
    {
        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            var raiz = await CarregarAsync(cancellationToken);
            raiz[chave] = JsonSerializer.SerializeToNode(valor);

            await GuardarNoDiscoAsync(raiz, cancellationToken);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public async Task RemoverAsync(string chave, CancellationToken cancellationToken = default)
    {
        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            var raiz = await CarregarAsync(cancellationToken);
            raiz.Remove(chave);

            await GuardarNoDiscoAsync(raiz, cancellationToken);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private async Task<JsonObject> CarregarAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_caminhoFicheiro))
        {
            return new JsonObject();
        }

        await using var stream = File.OpenRead(_caminhoFicheiro);

        // Ficheiro vazio (ex.: criado mas nunca escrito) é tratado como
        // configuração vazia, em vez de falhar a desserialização.
        if (stream.Length == 0)
        {
            return new JsonObject();
        }

        var no = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
        return no as JsonObject ?? new JsonObject();
    }

    private async Task GuardarNoDiscoAsync(JsonObject raiz, CancellationToken cancellationToken)
    {
        var diretorio = Path.GetDirectoryName(_caminhoFicheiro);
        if (!string.IsNullOrEmpty(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }

        var opcoes = new JsonSerializerOptions { WriteIndented = true };

        // Escreve para um ficheiro temporário e só depois substitui o
        // definitivo — evita corromper a configuração se a aplicação for
        // fechada a meio da escrita.
        var caminhoTemporario = _caminhoFicheiro + ".tmp";
        await using (var stream = File.Create(caminhoTemporario))
        {
            await JsonSerializer.SerializeAsync(stream, raiz, opcoes, cancellationToken);
        }

        File.Move(caminhoTemporario, _caminhoFicheiro, overwrite: true);
    }
}
