using KiVenda.Core.Funcionarios;
namespace KiVenda.Application.Abstractions.Persistence;
public interface IFuncionarioRepository
{
 Task<Funcionario?> ObterPorIdAsync(Guid id,CancellationToken cancellationToken=default);
 Task<IReadOnlyList<Funcionario>> ListarAsync(string? termoPesquisa=null,CancellationToken cancellationToken=default);
 Task AdicionarAsync(Funcionario funcionario,CancellationToken cancellationToken=default);
 void Remover(Funcionario funcionario);
 Task<string> ObterProximoCodigoAsync(CancellationToken cancellationToken=default);
}