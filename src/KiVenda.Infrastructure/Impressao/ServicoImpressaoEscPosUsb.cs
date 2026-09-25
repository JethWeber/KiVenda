using KiVenda.Application.Vendas;
using KiVenda.Infrastructure.Configuracao;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Serviço de impressão térmica baseado em ESC/POS.
/// A configuração é carregada do JSON local para que o utilizador possa
/// alterar o dispositivo sem reiniciar nem editar variáveis de ambiente.
/// </summary>
public sealed class ServicoImpressaoEscPosUsb : IServicoImpressao
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;
    private readonly TransporteImpressoraUsb _transporte;
    private readonly ServicoImpressaoTexto _servicoRelatorios;

    public ServicoImpressaoEscPosUsb(
        IArmazenamentoConfiguracaoLocal armazenamento,
        TransporteImpressoraUsb transporte,
        ServicoImpressaoTexto servicoRelatorios)
    {
        _armazenamento = armazenamento;
        _transporte = transporte;
        _servicoRelatorios = servicoRelatorios;
    }

    public async Task ImprimirReciboVendaAsync(
        ReciboVendaDto recibo,
        DadosLoja dadosLoja,
        CancellationToken cancellationToken = default)
    {
        var configuracao = await ObterConfiguracaoAsync(cancellationToken);

        if (!configuracao.Ativo)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configuracao.Dispositivo))
        {
            throw new InvalidOperationException(
                "A impressora térmica está ativada, mas nenhum dispositivo foi configurado.");
        }

        var gerador = new GeradorEscPos(configuracao);
        var dados = gerador.GerarRecibo(recibo, dadosLoja);

        await _transporte.EnviarAsync(configuracao.Dispositivo, dados, cancellationToken);
    }

    /// <summary>
    /// Relatórios continuam a usar a impressão normal/textual.
    /// </summary>
    public Task ImprimirTextoAsync(
        string titulo,
        string conteudo,
        CancellationToken cancellationToken = default) =>
        _servicoRelatorios.ImprimirTextoAsync(titulo, conteudo, cancellationToken);

    public async Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracao = await ObterConfiguracaoAsync(cancellationToken);

        if (!configuracao.Ativo || string.IsNullOrWhiteSpace(configuracao.Dispositivo))
        {
            return Array.Empty<string>();
        }

        if (File.Exists(configuracao.Dispositivo))
        {
            return
            [
                $"Térmica ESC/POS ({configuracao.Dispositivo})"
            ];
        }

        return Array.Empty<string>();
    }

    private async Task<ConfiguracaoImpressoraTermica> ObterConfiguracaoAsync(
        CancellationToken cancellationToken)
    {
        return await _armazenamento.ObterAsync<ConfiguracaoImpressoraTermica>(
            ConfiguracaoImpressoraTermica.Chave,
            cancellationToken)
            ?? ConfiguracaoImpressoraTermica.Padrao;
    }
}
