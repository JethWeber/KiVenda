using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

/// <summary>
/// Substitui "EliminarProduto": um produto com movimentos de stock
/// associados não pode ser apagado (ver Fase 1), por isso a UI (Fase 6)
/// mostra um botão "Eliminar", mas a operação real é uma inativação —
/// o produto deixa de aparecer nas listagens ativas e de poder ser
/// vendido/comprado, mas o seu histórico permanece intacto.
/// </summary>
public sealed record InativarProdutoCommand(Guid ProdutoId);

public sealed class InativarProdutoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(InativarProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        produto.Inativar();

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(contexto.UtilizadorId, "Inativou Produto", "Produto", produto.Id),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
