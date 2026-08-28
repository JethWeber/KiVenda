using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Clientes;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Clientes;

public sealed record CriarClienteCommand(string Nome, string? Telefone = null);

public sealed record EditarClienteCommand(Guid ClienteId, string Nome, string? Telefone);

public sealed record ClienteDto(Guid Id, string Nome, string? Telefone);

public sealed record ConsultarHistoricoComprasQuery(Guid ClienteId);

public sealed record VendaResumoDto(Guid VendaId, DateTime Data, decimal Total);

public sealed record ListarClientesQuery(string? TermoPesquisa = null);

/// <summary>
/// A gestão de clientes não está listada como restrita a nenhum perfil
/// na tabela de permissões da documentação funcional (Secção 5) — só
/// "Consultar clientes" aparece, sob Atendente. Por isso, criar/editar
/// cliente exige apenas a permissão-base de acesso ao sistema
/// (<see cref="Acao.ConsultarProdutosStockClientes"/>, disponível a
/// ambos os perfis), refletindo o facto de o cadastro de cliente
/// tipicamente acontecer no meio de uma venda, feita por um Atendente.
/// </summary>
public sealed class CriarClienteUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(CriarClienteCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var cliente = new Cliente(comando.Nome, comando.Telefone);

        await uow.Clientes.AdicionarAsync(cliente, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }
}

public sealed class EditarClienteUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(EditarClienteCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var cliente = await uow.Clientes.ObterPorIdAsync(comando.ClienteId, cancellationToken)
            ?? throw new DomainException("Cliente não encontrado.");

        cliente.EditarDados(comando.Nome, comando.Telefone);

        await uow.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ListarClientesUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<ClienteDto>> ExecutarAsync(ListarClientesQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var clientes = await uow.Clientes.ListarAsync(query.TermoPesquisa, cancellationToken);

        return clientes.Select(c => new ClienteDto(c.Id, c.Nome, c.Telefone)).ToList();
    }
}

public sealed class ConsultarHistoricoComprasUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<VendaResumoDto>> ExecutarAsync(ConsultarHistoricoComprasQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var vendas = await uow.Vendas.ListarAsync(clienteId: query.ClienteId, cancellationToken: cancellationToken);

        return vendas.Select(v => new VendaResumoDto(v.Id, v.Data, v.Total)).ToList();
    }
}
