using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

/// <summary>
/// Correção manual de stock (quebra, contagem física). Sempre exige
/// motivo (validado já pelo Core — ver Fase 1) e é sempre registada em
/// auditoria, por ser uma das operações sensíveis explícitas na
/// Secção 7 da documentação funcional.
/// </summary>
public sealed record RegistarAjusteStockCommand(Guid ProdutoId, decimal DeltaUnidadeBase, string Motivo);

public sealed class RegistarAjusteStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarAjusteStockCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AjustarStock);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var estoqueAntes = produto.EstoqueAtual;
        var movimento = produto.RegistarAjusteStock(comando.DeltaUnidadeBase, comando.Motivo, contexto.UtilizadorId);

        await uow.MovimentosStock.AdicionarAsync(movimento, cancellationToken);

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(
                contexto.UtilizadorId,
                "Ajuste manual de stock",
                "Produto",
                produto.Id,
                dadosAntes: $"{estoqueAntes} (motivo: {comando.Motivo})",
                dadosDepois: produto.EstoqueAtual.ToString("0.####")),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return movimento.Id;
    }
}
