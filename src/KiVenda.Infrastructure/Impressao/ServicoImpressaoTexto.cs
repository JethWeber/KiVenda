using System.Globalization;
using System.Text;
using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Implementação de referência do serviço de impressão: formata o
/// recibo no estilo típico de uma impressora térmica de talão (largura
/// fixa, texto monoespaçado) e grava o resultado em ficheiro de texto
/// em <see cref="Caminhos.CaminhosAplicacao.PastaRecibos"/>.
///
/// <para>
/// <b>Nota importante:</b> a integração real com uma impressora física
/// (ESC/POS via USB/série no Linux, ou a API de impressão do Windows)
/// depende do hardware exato usado pelo comerciante e não pode ser
/// implementada de forma significativa sem testar num dispositivo real
/// (ver Fase 12 — Testes de Infraestrutura). Esta classe entrega toda a
/// lógica de negócio (formatação do recibo) e faz a fronteira de I/O
/// através de um ficheiro, para já — trocar essa fronteira por um
/// verdadeiro envio para impressora é uma alteração isolada a
/// <see cref="EscreverParaDestinoAsync"/>, sem tocar na formatação.
/// </para>
/// </summary>
public sealed class ServicoImpressaoTexto : IServicoImpressao
{
    private const int LarguraColunas = 40;

    private readonly string _pastaRecibos;

    public ServicoImpressaoTexto(string pastaRecibos)
    {
        _pastaRecibos = pastaRecibos;
    }

    public async Task ImprimirReciboVendaAsync(ReciboVendaDto recibo, DadosLoja dadosLoja, CancellationToken cancellationToken = default)
    {
        var conteudo = FormatarRecibo(recibo, dadosLoja);
        var nomeFicheiro = $"recibo-{recibo.VendaId:N}.txt";

        await EscreverParaDestinoAsync(nomeFicheiro, conteudo, cancellationToken);
    }

    public async Task ImprimirTextoAsync(string titulo, string conteudo, CancellationToken cancellationToken = default)
    {
        var nomeFicheiro = $"{Sanitizar(titulo)}-{DateTime.Now:yyyyMMdd-HHmmss}.txt";

        await EscreverParaDestinoAsync(nomeFicheiro, conteudo, cancellationToken);
    }

    public Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        // Sem integração real com impressora (ver nota na classe), não há
        // impressoras a listar nesta implementação de referência.
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    private static string FormatarRecibo(ReciboVendaDto recibo, DadosLoja dadosLoja)
    {
        var cultura = ObterCulturaFormatacao();
        var sb = new StringBuilder();

        void Centralizar(string texto)
        {
            var espacos = Math.Max(0, (LarguraColunas - texto.Length) / 2);
            sb.Append(' ', espacos).AppendLine(texto);
        }

        void Separador() => sb.AppendLine(new string('-', LarguraColunas));

        Centralizar(dadosLoja.Nome);
        if (dadosLoja.Endereco is not null)
        {
            Centralizar(dadosLoja.Endereco);
        }

        if (dadosLoja.Contacto is not null)
        {
            Centralizar(dadosLoja.Contacto);
        }

        Separador();
        sb.AppendLine($"Recibo: {recibo.VendaId.ToString()[..8].ToUpperInvariant()}");
        sb.AppendLine($"Data:   {recibo.Data:dd/MM/yyyy HH:mm}");
        Separador();

        foreach (var item in recibo.Itens)
        {
            sb.AppendLine($"{item.ProdutoNome} ({item.ApresentacaoNome})");
            var linhaQuantidadeValor = $"  {item.QuantidadeNaApresentacao.ToString("0.##", cultura)} x".PadRight(20)
                + item.ValorTotal.ToString("N2", cultura).PadLeft(LarguraColunas - 20);
            sb.AppendLine(linhaQuantidadeValor);
        }

        Separador();
        sb.AppendLine(LinhaValor("Subtotal", recibo.Subtotal, cultura));
        if (recibo.Desconto > 0)
        {
            sb.AppendLine(LinhaValor("Desconto", -recibo.Desconto, cultura));
        }

        sb.AppendLine(LinhaValor("TOTAL", recibo.Total, cultura));
        Separador();

        foreach (var pagamento in recibo.Pagamentos)
        {
            sb.AppendLine(LinhaValor(pagamento.Metodo.ToString(), pagamento.Valor, cultura));
        }

        Separador();
        Centralizar("Obrigado pela preferência!");

        return sb.ToString();
    }

    private static string LinhaValor(string rotulo, decimal valor, CultureInfo cultura)
    {
        var valorTexto = $"{valor.ToString("N2", cultura)} Kz";
        return rotulo.PadRight(LarguraColunas - valorTexto.Length) + valorTexto;
    }

    private async Task EscreverParaDestinoAsync(string nomeFicheiro, string conteudo, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_pastaRecibos);
        var caminho = Path.Combine(_pastaRecibos, nomeFicheiro);

        await File.WriteAllTextAsync(caminho, conteudo, Encoding.UTF8, cancellationToken);
    }

    private static string Sanitizar(string texto)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var limpo = new string(texto.Select(c => invalidos.Contains(c) ? '-' : c).ToArray());
        return limpo.Length == 0 ? "relatorio" : limpo;
    }

    /// <summary>
    /// Nunca usa a cultura "pt-AO" do sistema operativo diretamente: em
    /// teste real (Fedora), essa cultura formatava com espaço como
    /// separador de milhares ("5 000,00"), não o ponto usado nos
    /// mockups do KiVenda ("5.000,00") — os dados ICU de "pt-AO" variam
    /// entre sistemas/distribuições Linux e não são algo a que valha a
    /// pena confiar aqui. Construímos sempre a formatação manualmente,
    /// para o recibo ficar igual independentemente da máquina.
    /// </summary>
    private static CultureInfo ObterCulturaFormatacao()
    {
        var cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return cultura;
    }
}
