namespace KiVenda.Infrastructure.Backup;

public sealed record ResultadoBackup(string CaminhoFicheiro, DateTime DataHora, long TamanhoBytes);

/// <summary>
/// Backup e restauração da base de dados local (Secção 4.9,
/// "Configurações → Backup / Restaurar Backup"). Trabalha diretamente
/// com o ficheiro SQLite, usando a API nativa de backup do SQLite (via
/// <c>Microsoft.Data.Sqlite</c>) em vez de uma simples cópia de
/// ficheiro, para produzir uma cópia consistente mesmo com a aplicação
/// em uso.
/// </summary>
public interface IServicoBackup
{
    Task<ResultadoBackup> CriarBackupAsync(string pastaDestino, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida a integridade do ficheiro de backup e restaura-o como base
    /// de dados ativa. Por segurança, a aplicação deve ser reiniciada
    /// depois de uma restauração bem-sucedida, para garantir que todas as
    /// ligações à base de dados são reabertas do zero.
    /// </summary>
    Task RestaurarBackupAsync(string caminhoFicheiroBackup, CancellationToken cancellationToken = default);

    /// <summary>Confirma que o ficheiro é uma base de dados SQLite válida antes de o restaurar.</summary>
    Task<bool> ValidarFicheiroBackupAsync(string caminhoFicheiroBackup, CancellationToken cancellationToken = default);
}
