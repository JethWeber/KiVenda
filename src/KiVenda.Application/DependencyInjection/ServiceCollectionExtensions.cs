using KiVenda.Application.Auditoria;
using KiVenda.Application.Caixa;
using KiVenda.Application.Cadastros;
using KiVenda.Application.Clientes;
using KiVenda.Application.Compras;
using KiVenda.Application.Fornecedores;
using KiVenda.Application.Empresas;
using KiVenda.Application.Produtos;
using KiVenda.Application.Relatorios;
using KiVenda.Application.Stock;
using KiVenda.Application.Utilizadores;
using KiVenda.Application.Vendas;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<ConsultarAuditoriaUseCase>();
        services.AddScoped<CriarProdutoUseCase>();
        services.AddScoped<EditarProdutoUseCase>();
        services.AddScoped<InativarProdutoUseCase>();
        services.AddScoped<CriarApresentacaoProdutoUseCase>();
        services.AddScoped<EditarApresentacaoProdutoUseCase>();
        services.AddScoped<ListarProdutosUseCase>();
        services.AddScoped<ListarCategoriasUseCase>();
        services.AddScoped<ListarUnidadesMedidaUseCase>();
        services.AddScoped<LocalizarProdutoPorCodigoUseCase>();
        services.AddScoped<RegistarEntradaStockUseCase>();
        services.AddScoped<RegistarSaidaStockUseCase>();
        services.AddScoped<RegistarAjusteStockUseCase>();
        services.AddScoped<ConsultarStockUseCase>();
        services.AddScoped<ConsultarMovimentosStockUseCase>();
        services.AddScoped<RecalcularEstoqueMaterializadoUseCase>();
        services.AddScoped<RegistarCompraUseCase>();
        services.AddScoped<ListarComprasUseCase>();
        services.AddScoped<IniciarVendaUseCase>();
        services.AddScoped<AdicionarItemVendaUseCase>();
        services.AddScoped<RemoverItemVendaUseCase>();
        services.AddScoped<AplicarDescontoVendaUseCase>();
        services.AddScoped<FinalizarVendaUseCase>();
        services.AddScoped<ConsultarVendaUseCase>();
        services.AddScoped<CancelarVendaUseCase>();
        services.AddScoped<AbrirCaixaUseCase>();
        services.AddScoped<FecharCaixaUseCase>();
        services.AddScoped<RegistarSuprimentoUseCase>();
        services.AddScoped<RegistarSangriaUseCase>();
        services.AddScoped<ConsultarMovimentacoesCaixaUseCase>();
        services.AddScoped<CriarClienteUseCase>();
        services.AddScoped<EditarClienteUseCase>();
        services.AddScoped<ListarClientesUseCase>();
        services.AddScoped<ConsultarHistoricoComprasUseCase>();
        services.AddScoped<CriarEmpresaUseCase>();
        services.AddScoped<EditarEmpresaUseCase>();
        services.AddScoped<ObterEmpresaUseCase>();
        services.AddScoped<RemoverEmpresaUseCase>();
        services.AddScoped<CriarFornecedorUseCase>();
        services.AddScoped<EditarFornecedorUseCase>();
        services.AddScoped<ListarFornecedoresUseCase>();
        services.AddScoped<ListarClientesCadastroUseCase>();
        services.AddScoped<GuardarClienteCadastroUseCase>();
        services.AddScoped<EliminarClienteUseCase>();
        services.AddScoped<ListarFornecedoresCadastroUseCase>();
        services.AddScoped<GuardarFornecedorCadastroUseCase>();
        services.AddScoped<EliminarFornecedorUseCase>();
        services.AddScoped<ListarFuncionariosUseCase>();
        services.AddScoped<GuardarFuncionarioUseCase>();
        services.AddScoped<EliminarFuncionarioUseCase>();
        services.AddScoped<CriarUtilizadorUseCase>();
        services.AddScoped<DefinirPerfilUseCase>();
        services.AddScoped<AutenticarUtilizadorUseCase>();
        services.AddScoped<AlterarPasswordUseCase>();
        services.AddScoped<ListarUtilizadoresUseCase>();
        services.AddScoped<EditarMeusDadosUseCase>();
        services.AddScoped<GerarRelatorioDiarioUseCase>();
        services.AddScoped<GerarRelatorioMensalUseCase>();
        services.AddScoped<GerarRelatorioStockUseCase>();
        services.AddScoped<ObterResumoDashboardUseCase>();
        return services;
    }
}