using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
namespace KiVenda.Core.Fornecedores;
public sealed class Fornecedor : Entity
{
    public string Nome { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Nif { get; private set; }
    public string? ProdutosFornecidos { get; private set; }
    private Fornecedor() { }
    public Fornecedor(string nome,string? telefone=null,string? produtosFornecidos=null,string? email=null,string? nif=null){ValidarNome(nome);Nome=nome.Trim();Telefone=N(telefone);ProdutosFornecidos=N(produtosFornecidos);Email=N(email);Nif=N(nif);}
    public void EditarDados(string nome,string? telefone,string? produtosFornecidos,string? email=null,string? nif=null){ValidarNome(nome);Nome=nome.Trim();Telefone=N(telefone);ProdutosFornecidos=N(produtosFornecidos);Email=N(email);Nif=N(nif);MarcarComoAtualizado();}
    private static void ValidarNome(string nome){if(string.IsNullOrWhiteSpace(nome))throw new DomainException("O nome do fornecedor é obrigatório.");}
    private static string? N(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}