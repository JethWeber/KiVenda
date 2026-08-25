using KiVenda.Application.Caixa;
using KiVenda.Application.Clientes;
using KiVenda.Application.Compras;
using KiVenda.Application.Fornecedores;
using KiVenda.Application.Produtos;
using KiVenda.Application.Relatorios;
using KiVenda.Application.Stock;
using KiVenda.Application.Utilizadores;
using KiVenda.Application.Vendas;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Application.DependencyInjection;

/// <summary>
/// Ponto único de registo desta camada no composition root do Desktop
/// (Fase 6). Todos os casos de uso são <c>Scoped</c> — cada operação da
/// UI (ex.: um clique num botão) resolve o seu próprio grafo, incluindo
/// um <see cref="Abstractions.Persistence.IUnitOfWork"/> novo, evitando
/// que alterações rastreadas por um ecrã "vazem" para outro.
///
/// NÃO regista aqui: <see cref="Abstractions.Persistence.IUnitOfWork"/>
/// (vem da Persistence, Fase 2), <see cref="Abstractions.Auth.IContextoAutenticacao"/>
/// e <see cref="Abstractions.Auth.ISenhaHasher"/> (vêm do Desktop/Infrastructure,
/// Fase 4/5) — esta camada só regista o que é seu.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        // Produtos
        services.AddScoped<CriarProdutoUseCase>();
        services.AddScoped<EditarProdutoUseCase>();
        services.AddScoped<InativarProdutoUseCase>();
        services.AddScoped<CriarApresentacaoProdutoUseCase>();
        services.AddScoped<EditarApresentacaoProdutoUseCase>();
        services.AddScoped<ListarProdutosUseCase>();
        services.AddScoped<ListarCategoriasUseCase>();
        services.AddScoped<ListarUnidadesMedidaUseCase>();

        // Stock
        services.AddScoped<RegistarEntradaStockUseCase>();
        services.AddScoped<RegistarSaidaStockUseCase>();
        services.AddScoped<RegistarAjusteStockUseCase>();
        services.AddScoped<ConsultarStockUseCase>();
        services.AddScoped<ConsultarMovimentosStockUseCase>();
        services.AddScoped<RecalcularEstoqueMaterializadoUseCase>();

        // Compras
        services.AddScoped<RegistarCompraUseCase>();
        services.AddScoped<ListarComprasUseCase>();

        // Vendas
        services.AddScoped<IniciarVendaUseCase>();
        services.AddScoped<AdicionarItemVendaUseCase>();
        services.AddScoped<RemoverItemVendaUseCase>();
        services.AddScoped<AplicarDescontoVendaUseCase>();
        services.AddScoped<FinalizarVendaUseCase>();

        // Caixa
        services.AddScoped<AbrirCaixaUseCase>();
        services.AddScoped<FecharCaixaUseCase>();
        services.AddScoped<RegistarSuprimentoUseCase>();
        services.AddScoped<RegistarSangriaUseCase>();
        services.AddScoped<ConsultarMovimentacoesCaixaUseCase>();

        // Clientes
        services.AddScoped<CriarClienteUseCase>();
        services.AddScoped<EditarClienteUseCase>();
        services.AddScoped<ListarClientesUseCase>();
        services.AddScoped<ConsultarHistoricoComprasUseCase>();

        // Fornecedores
        services.AddScoped<CriarFornecedorUseCase>();
        services.AddScoped<EditarFornecedorUseCase>();
        services.AddScoped<ListarFornecedoresUseCase>();

        // Relatórios
        services.AddScoped<GerarRelatorioDiarioUseCase>();
        services.AddScoped<GerarRelatorioMensalUseCase>();
        services.AddScoped<GerarRelatorioStockUseCase>();
        services.AddScoped<ObterResumoDashboardUseCase>();

        // Utilizadores
        services.AddScoped<CriarUtilizadorUseCase>();
        services.AddScoped<DefinirPerfilUseCase>();
        services.AddScoped<AutenticarUtilizadorUseCase>();
        services.AddScoped<AlterarPasswordUseCase>();
        services.AddScoped<ListarUtilizadoresUseCase>();

        return services;
    }
}
