using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

/// <summary>
/// Só permite renomear e ativar/inativar uma apresentação — o fator de
/// conversão e o código de barras são intencionalmente imutáveis depois
/// de criados (ver <see cref="Core.Produtos.ApresentacaoProduto"/>,
/// Fase 1), para nunca alterar retroativamente a interpretação de
/// movimentos de stock já registados nessa apresentação. Se o fator
/// estiver errado, a apresentação deve ser inativada e uma nova criada.
/// </summary>
public sealed record EditarApresentacaoProdutoCommand(Guid ProdutoId, Guid ApresentacaoId, string NovoNome);

public sealed record InativarApresentacaoProdutoCommand(Guid ProdutoId, Guid ApresentacaoId);

public sealed class EditarApresentacaoProdutoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(EditarApresentacaoProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var apresentacao = produto.ObterApresentacao(comando.ApresentacaoId);
        apresentacao.Renomear(comando.NovoNome);

        await uow.SaveChangesAsync(cancellationToken);
    }

    public async Task InativarAsync(InativarApresentacaoProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var apresentacao = produto.ObterApresentacao(comando.ApresentacaoId);
        apresentacao.Inativar();

        await uow.SaveChangesAsync(cancellationToken);
    }
}
