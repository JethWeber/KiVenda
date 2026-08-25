using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record CategoriaDto(Guid Id, string Nome);

public sealed record UnidadeMedidaDto(Guid Id, string Codigo, string Nome);

/// <summary>
/// Listagens simples, usadas para preencher os seletores de categoria e
/// unidade de medida no cadastro de produtos (Fase 6). A criação de
/// novas categorias/unidades fica para a Fase 11 (Configurações) — por
/// agora o utilizador escolhe entre as já semeadas (Fase 2) ou criadas
/// manualmente na base de dados.
/// </summary>
public sealed class ListarCategoriasUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<CategoriaDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var categorias = await uow.Categorias.ListarAsync(cancellationToken);

        return categorias.Select(c => new CategoriaDto(c.Id, c.Nome)).ToList();
    }
}

public sealed class ListarUnidadesMedidaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<UnidadeMedidaDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var unidades = await uow.UnidadesMedida.ListarAsync(cancellationToken);

        return unidades.Select(u => new UnidadeMedidaDto(u.Id, u.Codigo, u.Nome)).ToList();
    }
}
