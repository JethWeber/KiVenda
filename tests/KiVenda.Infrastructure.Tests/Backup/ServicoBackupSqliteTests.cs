using FluentAssertions;
using KiVenda.Core.Exceptions;
using KiVenda.Infrastructure.Backup;
using Microsoft.Data.Sqlite;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Backup;

/// <summary>
/// Usa Microsoft.Data.Sqlite diretamente (sem EF Core) para criar uma
/// base de dados de teste com uma tabela simples, exercitando
/// <see cref="ServicoBackupSqlite"/> exatamente como ele é: um serviço
/// que trabalha ao nível do ficheiro SQLite, independente do mapeamento
/// EF Core usado pela Persistence.
/// </summary>
public class ServicoBackupSqliteTests : IDisposable
{
    private readonly string _pastaTeste;
    private readonly string _caminhoBaseDeDados;

    public ServicoBackupSqliteTests()
    {
        _pastaTeste = Path.Combine(Path.GetTempPath(), $"kivenda-backup-teste-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_pastaTeste);
        _caminhoBaseDeDados = Path.Combine(_pastaTeste, "ativa.db");

        CriarBaseDeDadosComDados(_caminhoBaseDeDados, "Açúcar 1kg");
    }

    private static void CriarBaseDeDadosComDados(string caminho, string valorProduto)
    {
        using var conexao = new SqliteConnection($"Data Source={caminho}");
        conexao.Open();

        using var comandoCriar = conexao.CreateCommand();
        comandoCriar.CommandText = "CREATE TABLE Produto (Id INTEGER PRIMARY KEY, Nome TEXT NOT NULL);";
        comandoCriar.ExecuteNonQuery();

        using var comandoInserir = conexao.CreateCommand();
        comandoInserir.CommandText = "INSERT INTO Produto (Nome) VALUES ($nome);";
        comandoInserir.Parameters.AddWithValue("$nome", valorProduto);
        comandoInserir.ExecuteNonQuery();
    }

    private static string LerPrimeiroProduto(string caminho)
    {
        using var conexao = new SqliteConnection($"Data Source={caminho};Mode=ReadOnly");
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = "SELECT Nome FROM Produto LIMIT 1;";

        return (string)comando.ExecuteScalar()!;
    }

    [Fact]
    public async Task CriarBackupAsync_Deve_Gerar_Ficheiro_Com_Os_Mesmos_Dados()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);

        var resultado = await servico.CriarBackupAsync(Path.Combine(_pastaTeste, "backups"));

        File.Exists(resultado.CaminhoFicheiro).Should().BeTrue();
        resultado.TamanhoBytes.Should().BeGreaterThan(0);
        LerPrimeiroProduto(resultado.CaminhoFicheiro).Should().Be("Açúcar 1kg");
    }

    [Fact]
    public async Task ValidarFicheiroBackupAsync_Deve_Aceitar_Um_Backup_Real()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);
        var resultado = await servico.CriarBackupAsync(Path.Combine(_pastaTeste, "backups"));

        (await servico.ValidarFicheiroBackupAsync(resultado.CaminhoFicheiro)).Should().BeTrue();
    }

    [Fact]
    public async Task ValidarFicheiroBackupAsync_Deve_Rejeitar_Ficheiro_Que_Nao_E_Sqlite()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);
        var caminhoFalso = Path.Combine(_pastaTeste, "nao-e-um-backup.txt");
        await File.WriteAllTextAsync(caminhoFalso, "isto não é uma base de dados SQLite");

        (await servico.ValidarFicheiroBackupAsync(caminhoFalso)).Should().BeFalse();
    }

    [Fact]
    public async Task ValidarFicheiroBackupAsync_Deve_Rejeitar_Ficheiro_Inexistente()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);

        (await servico.ValidarFicheiroBackupAsync(Path.Combine(_pastaTeste, "nao-existe.db"))).Should().BeFalse();
    }

    [Fact]
    public async Task RestaurarBackupAsync_Deve_Repor_Os_Dados_Do_Backup_Na_Base_De_Dados_Ativa()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);
        var backup = await servico.CriarBackupAsync(Path.Combine(_pastaTeste, "backups"));

        // Simula alteração de dados depois do backup.
        using (var conexao = new SqliteConnection($"Data Source={_caminhoBaseDeDados}"))
        {
            conexao.Open();
            using var comando = conexao.CreateCommand();
            comando.CommandText = "UPDATE Produto SET Nome = 'Nome alterado depois do backup';";
            comando.ExecuteNonQuery();
        }

        await servico.RestaurarBackupAsync(backup.CaminhoFicheiro);

        LerPrimeiroProduto(_caminhoBaseDeDados).Should().Be("Açúcar 1kg");
    }

    [Fact]
    public async Task RestaurarBackupAsync_Deve_Rejeitar_Ficheiro_Invalido()
    {
        var servico = new ServicoBackupSqlite(_caminhoBaseDeDados);
        var caminhoFalso = Path.Combine(_pastaTeste, "nao-e-um-backup.txt");
        await File.WriteAllTextAsync(caminhoFalso, "conteúdo qualquer");

        var acao = async () => await servico.RestaurarBackupAsync(caminhoFalso);

        await acao.Should().ThrowAsync<DomainException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaTeste))
        {
            Directory.Delete(_pastaTeste, recursive: true);
        }
    }
}
