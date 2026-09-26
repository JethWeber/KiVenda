using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Cadastros;
using KiVenda.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class CadastrosViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    public ObservableCollection<ClienteCadastroDto> Clientes { get; } = new();
    public ObservableCollection<FornecedorCadastroDto> Fornecedores { get; } = new();
    public ObservableCollection<FuncionarioDto> Funcionarios { get; } = new();

    [ObservableProperty] private int _abaSelecionada;
    [ObservableProperty] private string _pesquisaClientes = "";
    [ObservableProperty] private string _pesquisaFornecedores = "";
    [ObservableProperty] private string _pesquisaFuncionarios = "";
    [ObservableProperty] private bool _formularioAberto;
    [ObservableProperty] private string? _entidadeEmEdicao;
    [ObservableProperty] private Guid? _idEmEdicao;
    [ObservableProperty] private string? _mensagemErro;
    [ObservableProperty] private bool _aGuardar;
    [ObservableProperty] private string _nome = "";
    [ObservableProperty] private string _telefone = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _nif = "";
    [ObservableProperty] private string _produtosFornecidos = "";
    [ObservableProperty] private string _bi = "";
    [ObservableProperty] private string _cargo = "";
    [ObservableProperty] private string _departamento = "";
    [ObservableProperty] private string _turno = "";
    [ObservableProperty] private DateTime _dataAdmissao = DateTime.Today;
    [ObservableProperty] private decimal _salarioBase;
    [ObservableProperty] private bool _ativo = true;
    [ObservableProperty] private bool _ehFornecedor;
    [ObservableProperty] private bool _ehFuncionario;

    public CadastrosViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _ = InicializarAsync();
    }

    partial void OnPesquisaClientesChanged(string value) => _ = CarregarClientesAsync();
    partial void OnPesquisaFornecedoresChanged(string value) => _ = CarregarFornecedoresAsync();
    partial void OnPesquisaFuncionariosChanged(string value) => _ = CarregarFuncionariosAsync();
    partial void OnEntidadeEmEdicaoChanged(string? value) { EhFornecedor = value == "Fornecedor"; EhFuncionario = value == "Funcionario"; }

    private async Task InicializarAsync(){await CarregarClientesAsync();await CarregarFornecedoresAsync();await CarregarFuncionariosAsync();}
    private async Task CarregarClientesAsync(){try{await using var s=_scopeFactory.CreateAsyncScope();var x=await s.ServiceProvider.GetRequiredService<ListarClientesCadastroUseCase>().ExecutarAsync(PesquisaClientes);Clientes.Clear();foreach(var i in x)Clientes.Add(i);}catch(Exception ex){MensagemErro=ex.Message;}}
    private async Task CarregarFornecedoresAsync(){try{await using var s=_scopeFactory.CreateAsyncScope();var x=await s.ServiceProvider.GetRequiredService<ListarFornecedoresCadastroUseCase>().ExecutarAsync(PesquisaFornecedores);Fornecedores.Clear();foreach(var i in x)Fornecedores.Add(i);}catch(Exception ex){MensagemErro=ex.Message;}}
    private async Task CarregarFuncionariosAsync(){try{await using var s=_scopeFactory.CreateAsyncScope();var x=await s.ServiceProvider.GetRequiredService<ListarFuncionariosUseCase>().ExecutarAsync(PesquisaFuncionarios);Funcionarios.Clear();foreach(var i in x)Funcionarios.Add(i);}catch(Exception ex){MensagemErro=ex.Message;}}

    [RelayCommand] private void NovoCliente()=>Abrir("Cliente");
    [RelayCommand] private void NovoFornecedor()=>Abrir("Fornecedor");
    [RelayCommand] private void NovoFuncionario()=>Abrir("Funcionario");
    private void Abrir(string tipo){EntidadeEmEdicao=tipo;IdEmEdicao=null;FormularioAberto=true;MensagemErro=null;Nome=Telefone=Email=Nif=ProdutosFornecidos=Bi=Cargo=Departamento=Turno="";DataAdmissao=DateTime.Today;SalarioBase=0;Ativo=true;}
    [RelayCommand] private void EditarCliente(ClienteCadastroDto x){Abrir("Cliente");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Nif=x.Nif??"";}
    [RelayCommand] private void EditarFornecedor(FornecedorCadastroDto x){Abrir("Fornecedor");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Nif=x.Nif??"";ProdutosFornecidos=x.ProdutosFornecidos??"";}
    [RelayCommand] private void EditarFuncionario(FuncionarioDto x){Abrir("Funcionario");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Bi=x.BI??"";Cargo=x.Cargo??"";Departamento=x.Departamento??"";Turno=x.Turno??"";DataAdmissao=x.DataAdmissao;SalarioBase=x.SalarioBase;Ativo=x.Ativo;}
    [RelayCommand] private void FecharFormulario(){FormularioAberto=false;EntidadeEmEdicao=null;IdEmEdicao=null;MensagemErro=null;}

    [RelayCommand] private async Task GuardarAsync()
    {
        MensagemErro=null;if(string.IsNullOrWhiteSpace(Nome)){MensagemErro="O nome é obrigatório.";return;}AGuardar=true;
        try{await using var s=_scopeFactory.CreateAsyncScope();
            if(EntidadeEmEdicao=="Cliente"){await s.ServiceProvider.GetRequiredService<GuardarClienteCadastroUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Nif);await CarregarClientesAsync();}
            else if(EntidadeEmEdicao=="Fornecedor"){await s.ServiceProvider.GetRequiredService<GuardarFornecedorCadastroUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Nif,ProdutosFornecidos);await CarregarFornecedoresAsync();}
            else{await s.ServiceProvider.GetRequiredService<GuardarFuncionarioUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Bi,Cargo,Departamento,Turno,DataAdmissao,SalarioBase,Ativo);await CarregarFuncionariosAsync();}
            FecharFormulario();
        }catch(DomainException ex){MensagemErro=ex.Message;}catch(Exception ex){MensagemErro=$"Não foi possível guardar: {ex.Message}";}finally{AGuardar=false;}
    }

    [RelayCommand] private Task EliminarClienteAsync(ClienteCadastroDto x)=>EliminarAsync("Cliente",x.Id);
    [RelayCommand] private Task EliminarFornecedorAsync(FornecedorCadastroDto x)=>EliminarAsync("Fornecedor",x.Id);
    [RelayCommand] private Task EliminarFuncionarioAsync(FuncionarioDto x)=>EliminarAsync("Funcionario",x.Id);
    private async Task EliminarAsync(string tipo,Guid id){try{await using var s=_scopeFactory.CreateAsyncScope();if(tipo=="Cliente"){await s.ServiceProvider.GetRequiredService<EliminarClienteUseCase>().ExecutarAsync(id);await CarregarClientesAsync();}else if(tipo=="Fornecedor"){await s.ServiceProvider.GetRequiredService<EliminarFornecedorUseCase>().ExecutarAsync(id);await CarregarFornecedoresAsync();}else{await s.ServiceProvider.GetRequiredService<EliminarFuncionarioUseCase>().ExecutarAsync(id);await CarregarFuncionariosAsync();}}catch(Exception ex){MensagemErro=$"Não foi possível eliminar: {ex.Message}";}}
}