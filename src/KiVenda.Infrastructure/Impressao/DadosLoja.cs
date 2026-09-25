namespace KiVenda.Infrastructure.Impressao;

public sealed record DadosLoja(
    string Nome,
    string? Nif = null,
    string? Endereco = null,
    string? Municipio = null,
    string? Provincia = null,
    string? Contacto = null,
    string? Website = null,
    byte[]? Logo = null,
    string? LogoMimeType = null);