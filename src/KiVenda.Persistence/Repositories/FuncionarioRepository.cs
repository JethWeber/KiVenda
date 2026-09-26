using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Funcionarios;
using Microsoft.EntityFrameworkCore;
namespace KiVenda.Persistence.Repositories;
internal sealed class FuncionarioRepository(KiVendaDbContext context):IFuncionarioRepository
{
 public async Task<Funcionario?> ObterPorIdAsync(Guid id,CancellationToken ct=default)=>await context.Funcionarios.FirstOrDefaultAsync(x=>x.Id==id,ct);
 public async Task<IReadOnlyList<Funcionario>> ListarAsync(string? termoPesquisa=null,CancellationToken ct=default){var q=context.Funcionarios.AsQueryable();if(!string.IsNullOrWhiteSpace(termoPesquisa)){var t=termoPesquisa.Trim();q=q.Where(x=>EF.Functions.Like(x.Nome,$"%{t}%")||(x.Codigo!=null&&EF.Functions.Like(x.Codigo,$"%{t}%"))||(x.BI!=null&&EF.Functions.Like(x.BI,$"%{t}%"))||(x.Cargo!=null&&EF.Functions.Like(x.Cargo,$"%{t}%")));}return await q.OrderBy(x=>x.Nome).ToListAsync(ct);}
 public async Task AdicionarAsync(Funcionario funcionario,CancellationToken ct=default)=>await context.Funcionarios.AddAsync(funcionario,ct);
 public void Remover(Funcionario funcionario)=>context.Funcionarios.Remove(funcionario);
 public async Task<string> ObterProximoCodigoAsync(CancellationToken ct=default){var ultimo=await context.Funcionarios.OrderByDescending(x=>x.Codigo).Select(x=>x.Codigo).FirstOrDefaultAsync(ct);var n=0;if(!string.IsNullOrWhiteSpace(ultimo)&&int.TryParse(ultimo.Replace("FUNC-",""),out var parsed))n=parsed;return $"FUNC-{n+1:000}";}
}