using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Empresas;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Empresas;

public sealed record CriarEmpresaCommand(
    string NomeComercial,
    string? RazaoSocial,
    string? Nif,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Municipio,
    string? Provincia,
    string? Website,
    byte[]? Logo,
    string? LogoMimeType);

public sealed record EditarEmpresaCommand(
    Guid EmpresaId,
    string NomeComercial,
    string? RazaoSocial,
    string? Nif,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Municipio,
    string? Provincia,
    string? Website,
    byte[]? Logo,
    string? LogoMimeType);

public sealed record EmpresaDto(
    Guid Id,
    string NomeComercial,
    string? RazaoSocial,
    string? Nif,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Municipio,
    string? Provincia,
    string? Website,
    byte[]? Logo,
    string? LogoMimeType);

public sealed class ObterEmpresaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<EmpresaDto?> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConfigurarSistema);
        var empresa = await uow.Empresas.ObterAsync(cancellationToken);
        return empresa is null ? null : Mapear(empresa);
    }

    internal static EmpresaDto Mapear(Empresa empresa) =>
        new(empresa.Id, empresa.NomeComercial, empresa.RazaoSocial, empresa.Nif,
            empresa.Telefone, empresa.Email, empresa.Endereco, empresa.Municipio,
            empresa.Provincia, empresa.Website, empresa.Logo, empresa.LogoMimeType);
}

public sealed class CriarEmpresaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(CriarEmpresaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConfigurarSistema);

        if (await uow.Empresas.ObterAsync(cancellationToken) is not null)
            throw new DomainException("Já existe um cadastro de empresa nesta instalação.");

        var empresa = new Empresa(comando.NomeComercial, comando.RazaoSocial, comando.Nif,
            comando.Telefone, comando.Email, comando.Endereco, comando.Municipio,
            comando.Provincia, comando.Website, comando.Logo, comando.LogoMimeType);

        await uow.Empresas.AdicionarAsync(empresa, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return empresa.Id;
    }
}

public sealed class EditarEmpresaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(EditarEmpresaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConfigurarSistema);

        var empresa = await uow.Empresas.ObterAsync(cancellationToken);
        if (empresa is null || empresa.Id != comando.EmpresaId)
            throw new DomainException("Empresa não encontrada.");

        empresa.EditarDados(comando.NomeComercial, comando.RazaoSocial, comando.Nif,
            comando.Telefone, comando.Email, comando.Endereco, comando.Municipio,
            comando.Provincia, comando.Website, comando.Logo, comando.LogoMimeType);

        await uow.SaveChangesAsync(cancellationToken);
    }
}

public sealed class RemoverEmpresaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConfigurarSistema);

        var empresa = await uow.Empresas.ObterAsync(cancellationToken);
        if (empresa is null)
            return;

        uow.Empresas.Remover(empresa);
        await uow.SaveChangesAsync(cancellationToken);
    }
}
