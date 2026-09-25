using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Empresas;

/// <summary>
/// Dados da empresa/loja proprietária desta instalação do KiVenda.
/// Existe no máximo um cadastro ativo por base de dados.
/// </summary>
public sealed class Empresa : Entity
{
    public string NomeComercial { get; private set; } = null!;
    public string? RazaoSocial { get; private set; }
    public string? Nif { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Endereco { get; private set; }
    public string? Municipio { get; private set; }
    public string? Provincia { get; private set; }
    public string? Website { get; private set; }
    public byte[]? Logo { get; private set; }
    public string? LogoMimeType { get; private set; }

    private Empresa() { }

    public Empresa(
        string nomeComercial,
        string? razaoSocial = null,
        string? nif = null,
        string? telefone = null,
        string? email = null,
        string? endereco = null,
        string? municipio = null,
        string? provincia = null,
        string? website = null,
        byte[]? logo = null,
        string? logoMimeType = null)
    {
        ValidarNome(nomeComercial);
        ValidarLogo(logo, logoMimeType);

        NomeComercial = nomeComercial.Trim();
        RazaoSocial = Normalizar(razaoSocial);
        Nif = Normalizar(nif);
        Telefone = Normalizar(telefone);
        Email = Normalizar(email);
        Endereco = Normalizar(endereco);
        Municipio = Normalizar(municipio);
        Provincia = Normalizar(provincia);
        Website = Normalizar(website);
        Logo = logo;
        LogoMimeType = logoMimeType;
    }

    public void EditarDados(
        string nomeComercial,
        string? razaoSocial,
        string? nif,
        string? telefone,
        string? email,
        string? endereco,
        string? municipio,
        string? provincia,
        string? website,
        byte[]? logo,
        string? logoMimeType)
    {
        ValidarNome(nomeComercial);
        ValidarLogo(logo, logoMimeType);

        NomeComercial = nomeComercial.Trim();
        RazaoSocial = Normalizar(razaoSocial);
        Nif = Normalizar(nif);
        Telefone = Normalizar(telefone);
        Email = Normalizar(email);
        Endereco = Normalizar(endereco);
        Municipio = Normalizar(municipio);
        Provincia = Normalizar(provincia);
        Website = Normalizar(website);
        Logo = logo;
        LogoMimeType = logoMimeType;
        MarcarComoAtualizado();
    }

    public void RemoverLogo()
    {
        Logo = null;
        LogoMimeType = null;
        MarcarComoAtualizado();
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome comercial da empresa é obrigatório.");
    }

    private static void ValidarLogo(byte[]? logo, string? logoMimeType)
    {
        if (logo is null)
        {
            if (!string.IsNullOrWhiteSpace(logoMimeType))
                throw new DomainException("O tipo do logótipo não pode existir sem o ficheiro.");
            return;
        }

        if (logo.Length == 0)
            throw new DomainException("O logótipo selecionado está vazio.");

        if (logo.Length > 2 * 1024 * 1024)
            throw new DomainException("O logótipo não pode ultrapassar 2 MB.");

        if (logoMimeType is not ("image/png" or "image/jpeg" or "image/webp"))
            throw new DomainException("O logótipo deve ser PNG, JPEG ou WebP.");
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
