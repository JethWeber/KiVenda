using KiVenda.Application.Vendas;
using KiVenda.Infrastructure.Configuracao;

namespace KiVenda.Infrastructure.Impressao;

public sealed class ServicoImpressaoEscPosUsb : IServicoImpressao, IServicoImpressaoTermica
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;
    private readonly IDetectorImpressoras _detector;
    private readonly ITransporteImpressora _transporte;
    private readonly ServicoImpressaoTexto _servicoRelatorios;

    public ServicoImpressaoEscPosUsb(
        IArmazenamentoConfiguracaoLocal armazenamento,
        IDetectorImpressoras detector,
        ITransporteImpressora transporte,
        ServicoImpressaoTexto servicoRelatorios)
    {
        _armazenamento = armazenamento;
        _detector = detector;
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
            return;

        configuracao.Validar();

        var gerador = new GeradorEscPos(configuracao);
        var dados = gerador.GerarRecibo(recibo, dadosLoja);

        await _transporte.EnviarAsync(configuracao, dados, cancellationToken);
    }

    public Task ImprimirTextoAsync(
        string titulo,
        string conteudo,
        CancellationToken cancellationToken = default) =>
        _servicoRelatorios.ImprimirTextoAsync(titulo, conteudo, cancellationToken);

    public async Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        var dispositivos = await _detector.DetetarAsync(cancellationToken);

        return dispositivos
            .Where(d => d.Disponivel)
            .Select(d => d.Nome)
            .ToList();
    }

    public async Task TestarImpressoraAsync(
        ConfiguracaoImpressoraTermica configuracao,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        configuracao.Validar();

        var gerador = new GeradorEscPos(configuracao);
        var dados = gerador.GerarTeste("Teste de comunicação");

        await _transporte.EnviarAsync(configuracao, dados, cancellationToken);
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
