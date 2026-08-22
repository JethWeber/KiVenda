using KiVenda.Core.Exceptions;
using Microsoft.Data.Sqlite;

namespace KiVenda.Infrastructure.Backup;

/// <summary>
/// Usa <see cref="SqliteConnection.BackupDatabase"/> — a API nativa de
/// backup do SQLite — em vez de copiar o ficheiro `.db` diretamente.
/// Isto garante uma cópia consistente mesmo que existam transações em
/// curso no momento do backup (uma cópia de ficheiro "crua" arrisca-se
/// a apanhar o ficheiro a meio de uma escrita).
/// </summary>
public sealed class ServicoBackupSqlite : IServicoBackup
{
    private const int CabecalhoSqliteBytes = 16;
    private static readonly byte[] AssinaturaSqlite = "SQLite format 3\0"u8.ToArray();

    private readonly string _caminhoBaseDeDadosAtiva;

    public ServicoBackupSqlite(string caminhoBaseDeDadosAtiva)
    {
        _caminhoBaseDeDadosAtiva = caminhoBaseDeDadosAtiva;
    }

    public async Task<ResultadoBackup> CriarBackupAsync(string pastaDestino, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(pastaDestino);

        var dataHora = DateTime.Now;
        var nomeFicheiro = $"kivenda-backup-{dataHora:yyyyMMdd-HHmmss}.wtbak";
        var caminhoDestino = Path.Combine(pastaDestino, nomeFicheiro);

        await using (var origem = new SqliteConnection($"Data Source={_caminhoBaseDeDadosAtiva};Mode=ReadOnly"))
        await using (var destino = new SqliteConnection($"Data Source={caminhoDestino}"))
        {
            await origem.OpenAsync(cancellationToken);
            await destino.OpenAsync(cancellationToken);

            origem.BackupDatabase(destino);
        }

        var tamanho = new FileInfo(caminhoDestino).Length;

        return new ResultadoBackup(caminhoDestino, dataHora, tamanho);
    }

    public async Task RestaurarBackupAsync(string caminhoFicheiroBackup, CancellationToken cancellationToken = default)
    {
        if (!await ValidarFicheiroBackupAsync(caminhoFicheiroBackup, cancellationToken))
        {
            throw new DomainException("O ficheiro selecionado não é um backup válido do KiVenda.");
        }

        // A restauração usa a mesma API de backup, mas invertida: o
        // ficheiro de backup passa a ser a origem, e a base de dados
        // ativa o destino. O chamador (Desktop) é responsável por
        // garantir que não há nenhum DbContext aberto sobre o ficheiro
        // ativo neste momento — ver Fase 11 do plano de implementação.
        await using var origem = new SqliteConnection($"Data Source={caminhoFicheiroBackup};Mode=ReadOnly");
        await using var destino = new SqliteConnection($"Data Source={_caminhoBaseDeDadosAtiva}");

        await origem.OpenAsync(cancellationToken);
        await destino.OpenAsync(cancellationToken);

        origem.BackupDatabase(destino);
    }

    public async Task<bool> ValidarFicheiroBackupAsync(string caminhoFicheiroBackup, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoFicheiroBackup))
        {
            return false;
        }

        var buffer = new byte[CabecalhoSqliteBytes];

        await using var stream = File.OpenRead(caminhoFicheiroBackup);
        if (stream.Length < CabecalhoSqliteBytes)
        {
            return false;
        }

        var lidos = await stream.ReadAsync(buffer, cancellationToken);

        return lidos == CabecalhoSqliteBytes && buffer.AsSpan().SequenceEqual(AssinaturaSqlite);
    }
}
