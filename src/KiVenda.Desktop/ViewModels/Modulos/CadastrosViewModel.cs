using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Cadastros;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class CadastrosViewModel : ViewModelBase
{
    // Cultura apenas para separador de milhar nas contagens (não usar a
    // cultura do sistema — mesma regra já seguida pelo FormatadorKz).
    private static readonly CultureInfo CulturaNumerica = new("pt-PT");

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
    [ObservableProperty] private DateTimeOffset? _dataAdmissao = new(DateTime.Today);
    [ObservableProperty] private decimal _salarioBase;
    [ObservableProperty] private bool _ativo = true;
    [ObservableProperty] private bool _ehFornecedor;
    [ObservableProperty] private bool _ehFuncionario;

    // Etapas do cadastro de funcionário (formulário longo).
    [ObservableProperty] private int _etapaFuncionario;
    [ObservableProperty] private bool _funcionarioEtapa1 = true;
    [ObservableProperty] private bool _funcionarioEtapa2;
    [ObservableProperty] private bool _funcionarioEtapa3;

    // Indicam qual aba está ativa, para a UI (ex.: qual botão "+ Novo"
    // mostrar no cabeçalho). Derivadas de AbaSelecionada, nunca
    // definidas diretamente.
    [ObservableProperty] private bool _ehAbaClientes = true;
    [ObservableProperty] private bool _ehAbaFornecedores;
    [ObservableProperty] private bool _ehAbaFuncionarios;

    // ===================== Indicadores =====================
    // Apenas os calculáveis a partir dos dados já carregados nas
    // coleções acima — nenhuma métrica fictícia ou de compliance.

    [ObservableProperty] private string _totalClientesTexto = "—";
    [ObservableProperty] private string _clientesComNifTexto = "—";
    [ObservableProperty] private string _clientesComContactoTexto = "—";

    [ObservableProperty] private string _totalFornecedoresTexto = "—";
    [ObservableProperty] private string _fornecedoresComNifTexto = "—";
    [ObservableProperty] private string _fornecedoresComProdutosTexto = "—";

    [ObservableProperty] private string _totalFuncionariosTexto = "—";
    [ObservableProperty] private string _funcionariosAtivosTexto = "—";
    [ObservableProperty] private string _funcionariosInativosTexto = "—";
    [ObservableProperty] private string _massaSalarialTexto = "—";
    [ObservableProperty] private string _admissoesEsteAnoTexto = "—";

    // ===================== Formulário =====================

    /// <summary>"Novo Cliente", "Editar Funcionário"... conforme a entidade e se é criação ou edição.</summary>
    public string TituloFormulario =>
        (IdEmEdicao is null ? "Novo " : "Editar ") + RotuloEntidade(EntidadeEmEdicao);

    private static string RotuloEntidade(string? tipo) => tipo switch
    {
        "Cliente" => "Cliente",
        "Fornecedor" => "Fornecedor",
        "Funcionario" => "Funcionário",
        _ => "Registo"
    };

    // ===================== Aviso (toast) de sucesso/erro: aparece 3 s e some sozinho =====================
    private CancellationTokenSource? _toastCts;

    [ObservableProperty] private bool _toastVisivel;
    [ObservableProperty] private bool _toastErro;
    [ObservableProperty] private string _toastTitulo = string.Empty;
    [ObservableProperty] private string _toastMensagem = string.Empty;

    private async void MostrarToast(bool erro, string mensagem)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();

        ToastErro = erro;
        ToastTitulo = erro ? "Erro" : "Sucesso";
        ToastMensagem = mensagem;
        ToastVisivel = true;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            ToastVisivel = false;
        }
        catch (OperationCanceledException)
        {
            // Chegou outro aviso: ele reinicia a contagem.
        }
    }

    // Qualquer mensagem de erro (lista ou formulário) vira também aviso visual.
    partial void OnMensagemErroChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            MostrarToast(erro: true, value);
    }

    public CadastrosViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _ = InicializarAsync();
    }

    partial void OnPesquisaClientesChanged(string value) => _ = CarregarClientesAsync();
    partial void OnPesquisaFornecedoresChanged(string value) => _ = CarregarFornecedoresAsync();
    partial void OnPesquisaFuncionariosChanged(string value) => _ = CarregarFuncionariosAsync();
    partial void OnEntidadeEmEdicaoChanged(string? value)
    {
        EhFornecedor = value == "Fornecedor";
        EhFuncionario = value == "Funcionario";
        OnPropertyChanged(nameof(TituloFormulario));
        EtapaFuncionario = 0;
        AtualizarEtapaFuncionario();
    }

    partial void OnEtapaFuncionarioChanged(int value) => AtualizarEtapaFuncionario();
    partial void OnIdEmEdicaoChanged(Guid? value) => OnPropertyChanged(nameof(TituloFormulario));

    private void AtualizarEtapaFuncionario()
    {
        FuncionarioEtapa1 = EtapaFuncionario == 0;
        FuncionarioEtapa2 = EtapaFuncionario == 1;
        FuncionarioEtapa3 = EtapaFuncionario == 2;
    }

    partial void OnAbaSelecionadaChanged(int value)
    {
        EhAbaClientes = value == 0;
        EhAbaFornecedores = value == 1;
        EhAbaFuncionarios = value == 2;
    }

    [RelayCommand] private void SelecionarAbaClientes() => AbaSelecionada = 0;
    [RelayCommand] private void SelecionarAbaFornecedores() => AbaSelecionada = 1;
    [RelayCommand] private void SelecionarAbaFuncionarios() => AbaSelecionada = 2;

    private async Task InicializarAsync(){await CarregarClientesAsync();await CarregarFornecedoresAsync();await CarregarFuncionariosAsync();}

    private async Task CarregarClientesAsync()
    {
        try
        {
            await using var s=_scopeFactory.CreateAsyncScope();
            var x=await s.ServiceProvider.GetRequiredService<ListarClientesCadastroUseCase>().ExecutarAsync(PesquisaClientes);
            Clientes.Clear();
            foreach(var i in x)Clientes.Add(i);
            AtualizarIndicadoresClientes();
        }
        catch(Exception ex){MensagemErro=ex.Message;}
    }

    private async Task CarregarFornecedoresAsync()
    {
        try
        {
            await using var s=_scopeFactory.CreateAsyncScope();
            var x=await s.ServiceProvider.GetRequiredService<ListarFornecedoresCadastroUseCase>().ExecutarAsync(PesquisaFornecedores);
            Fornecedores.Clear();
            foreach(var i in x)Fornecedores.Add(i);
            AtualizarIndicadoresFornecedores();
        }
        catch(Exception ex){MensagemErro=ex.Message;}
    }

    private async Task CarregarFuncionariosAsync()
    {
        try
        {
            await using var s=_scopeFactory.CreateAsyncScope();
            var x=await s.ServiceProvider.GetRequiredService<ListarFuncionariosUseCase>().ExecutarAsync(PesquisaFuncionarios);
            Funcionarios.Clear();
            foreach(var i in x)Funcionarios.Add(i);
            AtualizarIndicadoresFuncionarios();
        }
        catch(Exception ex){MensagemErro=ex.Message;}
    }

    private void AtualizarIndicadoresClientes()
    {
        TotalClientesTexto = FormatarInteiro(Clientes.Count);
        ClientesComNifTexto = FormatarInteiro(Clientes.Count(c => !string.IsNullOrWhiteSpace(c.Nif)));
        ClientesComContactoTexto = FormatarInteiro(Clientes.Count(c => !string.IsNullOrWhiteSpace(c.Telefone) || !string.IsNullOrWhiteSpace(c.Email)));
    }

    private void AtualizarIndicadoresFornecedores()
    {
        TotalFornecedoresTexto = FormatarInteiro(Fornecedores.Count);
        FornecedoresComNifTexto = FormatarInteiro(Fornecedores.Count(f => !string.IsNullOrWhiteSpace(f.Nif)));
        FornecedoresComProdutosTexto = FormatarInteiro(Fornecedores.Count(f => !string.IsNullOrWhiteSpace(f.ProdutosFornecidos)));
    }

    private void AtualizarIndicadoresFuncionarios()
    {
        TotalFuncionariosTexto = FormatarInteiro(Funcionarios.Count);
        FuncionariosAtivosTexto = FormatarInteiro(Funcionarios.Count(f => f.Ativo));
        FuncionariosInativosTexto = FormatarInteiro(Funcionarios.Count(f => !f.Ativo));
        MassaSalarialTexto = FormatadorKz.Formatar(Funcionarios.Where(f => f.Ativo).Sum(f => f.SalarioBase));
        AdmissoesEsteAnoTexto = FormatarInteiro(Funcionarios.Count(f => f.DataAdmissao.Year == DateTime.Today.Year));
    }

    private static string FormatarInteiro(int valor) => valor.ToString("N0", CulturaNumerica);

    [RelayCommand] private void NovoCliente()=>Abrir("Cliente");
    [RelayCommand] private void NovoFornecedor()=>Abrir("Fornecedor");
    [RelayCommand] private void NovoFuncionario()=>Abrir("Funcionario");

    [RelayCommand]
    private void AvancarEtapaFuncionario()
    {
        if (EtapaFuncionario < 2)
            EtapaFuncionario++;
    }

    [RelayCommand]
    private void VoltarEtapaFuncionario()
    {
        if (EtapaFuncionario > 0)
            EtapaFuncionario--;
    }
    private void Abrir(string tipo){EntidadeEmEdicao=tipo;IdEmEdicao=null;FormularioAberto=true;MensagemErro=null;Nome=Telefone=Email=Nif=ProdutosFornecidos=Bi=Cargo=Departamento=Turno="";DataAdmissao=new DateTimeOffset(DateTime.Today);SalarioBase=0;Ativo=true;}
    [RelayCommand] private void EditarCliente(ClienteCadastroDto x){Abrir("Cliente");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Nif=x.Nif??"";}
    [RelayCommand] private void EditarFornecedor(FornecedorCadastroDto x){Abrir("Fornecedor");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Nif=x.Nif??"";ProdutosFornecidos=x.ProdutosFornecidos??"";}
    [RelayCommand] private void EditarFuncionario(FuncionarioDto x){Abrir("Funcionario");IdEmEdicao=x.Id;Nome=x.Nome;Telefone=x.Telefone??"";Email=x.Email??"";Bi=x.BI??"";Cargo=x.Cargo??"";Departamento=x.Departamento??"";Turno=x.Turno??"";DataAdmissao=new DateTimeOffset(x.DataAdmissao);SalarioBase=x.SalarioBase;Ativo=x.Ativo;}
    [RelayCommand] private void FecharFormulario(){FormularioAberto=false;EntidadeEmEdicao=null;IdEmEdicao=null;MensagemErro=null;}

    [RelayCommand] private async Task GuardarAsync()
    {
        MensagemErro=null;if(string.IsNullOrWhiteSpace(Nome)){MensagemErro="O nome é obrigatório.";return;}AGuardar=true;
        try{await using var s=_scopeFactory.CreateAsyncScope();
            if(EntidadeEmEdicao=="Cliente"){await s.ServiceProvider.GetRequiredService<GuardarClienteCadastroUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Nif);await CarregarClientesAsync();}
            else if(EntidadeEmEdicao=="Fornecedor"){await s.ServiceProvider.GetRequiredService<GuardarFornecedorCadastroUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Nif,ProdutosFornecidos);await CarregarFornecedoresAsync();}
            else{await s.ServiceProvider.GetRequiredService<GuardarFuncionarioUseCase>().ExecutarAsync(IdEmEdicao,Nome,Telefone,Email,Bi,Cargo,Departamento,Turno,DataAdmissao?.DateTime ?? DateTime.Today,SalarioBase,Ativo);await CarregarFuncionariosAsync();}
            var tipoGuardado = RotuloEntidade(EntidadeEmEdicao);
            var acaoGuardada = IdEmEdicao is null ? "adicionado" : "atualizado";
            var nomeGuardado = Nome;
            FecharFormulario();
            MostrarToast(false, $"{tipoGuardado} \"{nomeGuardado}\" {acaoGuardada} com sucesso.");
        }catch(DomainException ex){MensagemErro=ex.Message;}catch(Exception ex){MensagemErro=$"Não foi possível guardar: {ex.Message}";}finally{AGuardar=false;}
    }

    [RelayCommand] private Task EliminarClienteAsync(ClienteCadastroDto x)=>EliminarAsync("Cliente",x.Id);
    [RelayCommand] private Task EliminarFornecedorAsync(FornecedorCadastroDto x)=>EliminarAsync("Fornecedor",x.Id);
    [RelayCommand] private Task EliminarFuncionarioAsync(FuncionarioDto x)=>EliminarAsync("Funcionario",x.Id);
    private async Task EliminarAsync(string tipo,Guid id){try{await using var s=_scopeFactory.CreateAsyncScope();if(tipo=="Cliente"){await s.ServiceProvider.GetRequiredService<EliminarClienteUseCase>().ExecutarAsync(id);await CarregarClientesAsync();}else if(tipo=="Fornecedor"){await s.ServiceProvider.GetRequiredService<EliminarFornecedorUseCase>().ExecutarAsync(id);await CarregarFornecedoresAsync();}else{await s.ServiceProvider.GetRequiredService<EliminarFuncionarioUseCase>().ExecutarAsync(id);await CarregarFuncionariosAsync();}MostrarToast(false,$"{RotuloEntidade(tipo)} eliminado com sucesso.");}catch(Exception ex){MensagemErro=$"Não foi possível eliminar: {ex.Message}";}}
}
