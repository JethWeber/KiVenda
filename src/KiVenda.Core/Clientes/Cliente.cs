using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
namespace KiVenda.Core.Clientes;
public sealed class Cliente : Entity
{
    public string Nome { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Nif { get; private set; }
    private Cliente() { }
    public Cliente(string nome, string? telefone = null, string? email = null, string? nif = null) { ValidarNome(nome); Nome=nome.Trim(); Telefone=N(telefone); Email=N(email); Nif=N(nif); }
    public void EditarDados(string nome, string? telefone, string? email=null, string? nif=null) { ValidarNome(nome); Nome=nome.Trim(); Telefone=N(telefone); Email=N(email); Nif=N(nif); MarcarComoAtualizado(); }
    private static void ValidarNome(string nome) { if(string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do cliente é obrigatório."); }
    private static string? N(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}