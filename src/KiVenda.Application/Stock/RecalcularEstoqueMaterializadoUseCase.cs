using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

/// <summary>
/// Ferramenta de diagnóstico/correção de divergências: reprocessa todo o
/// histórico de <see cref="Core.Produtos.MovimentoStock"/> de um produto
/// e corrige o valor materializado, sem depender de confiar no valor
/// atual em <c>Produto.EstoqueAtual</c>. Sempre auditado, por poder
/// alterar um valor visível em relatórios.
/// </summary>
public sealed record RecalcularEstoqueMaterializadoCommand(Guid ProdutoId);

public sealed class RecalcularEstoqueMaterializadoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<decimal> ExecutarAsync(RecalcularEstoqueMaterializadoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AjustarStock);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var estoqueAntes = produto.EstoqueAtual;
        var movimentos = await uow.MovimentosStock.ListarTodosPorProdutoAsync(produto.Id, cancellationToken);

        produto.RecalcularEstoqueMaterializado(movimentos);

        if (estoqueAntes != produto.EstoqueAtual)
        {
            await uow.LogsAuditoria.AdicionarAsync(
                new LogAuditoria(
                    contexto.UtilizadorId,
                    "Recalculou estoque materializado",
                    "Produto",
                    produto.Id,
                    dadosAntes: estoqueAntes.ToString("0.####"),
                    dadosDepois: produto.EstoqueAtual.ToString("0.####")),
                cancellationToken);
        }

        await uow.SaveChangesAsync(cancellationToken);

        return produto.EstoqueAtual;
    }
}
