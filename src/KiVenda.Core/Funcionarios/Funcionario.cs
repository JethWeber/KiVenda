using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
namespace KiVenda.Core.Funcionarios;
public sealed class Funcionario : Entity
{
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? BI { get; private set; }
    public string? Cargo { get; private set; }
    public string? Departamento { get; private set; }
    public string? Turno { get; private set; }
    public DateTime DataAdmissao { get; private set; }
    public decimal SalarioBase { get; private set; }
    public bool Ativo { get; private set; }
    private Funcionario(){}
    public Funcionario(string codigo,string nome,string? telefone,string? email,string? bi,string? cargo,string? departamento,string? turno,DateTime dataAdmissao,decimal salarioBase,bool ativo=true)
    { if(string.IsNullOrWhiteSpace(codigo))throw new DomainException("O código do funcionário é obrigatório."); if(string.IsNullOrWhiteSpace(nome))throw new DomainException("O nome do funcionário é obrigatório."); if(salarioBase<0)throw new DomainException("O salário base não pode ser negativo."); Codigo=codigo.Trim();Nome=nome.Trim();Telefone=N(telefone);Email=N(email);BI=N(bi);Cargo=N(cargo);Departamento=N(departamento);Turno=N(turno);DataAdmissao=dataAdmissao.Date;SalarioBase=salarioBase;Ativo=ativo; }
    public void EditarDados(string nome,string? telefone,string? email,string? bi,string? cargo,string? departamento,string? turno,DateTime dataAdmissao,decimal salarioBase,bool ativo)
    { if(string.IsNullOrWhiteSpace(nome))throw new DomainException("O nome do funcionário é obrigatório."); if(salarioBase<0)throw new DomainException("O salário base não pode ser negativo."); Nome=nome.Trim();Telefone=N(telefone);Email=N(email);BI=N(bi);Cargo=N(cargo);Departamento=N(departamento);Turno=N(turno);DataAdmissao=dataAdmissao.Date;SalarioBase=salarioBase;Ativo=ativo;MarcarComoAtualizado(); }
    private static string? N(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}