using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Fornecedores;

public sealed record CriarFornecedorCommand(string Nome, string? Telefone = null, string? ProdutosFornecidos = null);

public sealed record EditarFornecedorCommand(Guid FornecedorId, string Nome, string? Telefone, string? ProdutosFornecidos);

/// <summary>
/// Fornecedores existem para agilizar o registo de Compras (Secção 4.7
/// da documentação funcional), módulo que a tabela de permissões
/// restringe a Gerente ("Registar compras"). Por coerência, a gestão de
/// fornecedores segue a mesma restrição.
/// </summary>
public sealed class CriarFornecedorUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(CriarFornecedorCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.RegistarCompras);

        var fornecedor = new Fornecedor(comando.Nome, comando.Telefone, comando.ProdutosFornecidos);

        await uow.Fornecedores.AdicionarAsync(fornecedor, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return fornecedor.Id;
    }
}

public sealed class EditarFornecedorUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(EditarFornecedorCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.RegistarCompras);

        var fornecedor = await uow.Fornecedores.ObterPorIdAsync(comando.FornecedorId, cancellationToken)
            ?? throw new DomainException("Fornecedor não encontrado.");

        fornecedor.EditarDados(comando.Nome, comando.Telefone, comando.ProdutosFornecidos);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
