using System.Globalization;
using System.Text;
using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

public sealed class ServicoImpressaoTexto : IServicoImpressao
{
    private const int LarguraColunas = 40;
    private readonly string _pastaRecibos;

    public ServicoImpressaoTexto(string pastaRecibos)
    {
        _pastaRecibos = pastaRecibos;
    }

    public async Task ImprimirReciboVendaAsync(
        ReciboVendaDto recibo,
        DadosLoja dadosLoja,
        CancellationToken cancellationToken = default)
    {
        var conteudo = GerarPreview(recibo, dadosLoja);
        var nomeFicheiro = $"recibo-{recibo.VendaId:N}.txt";

        await EscreverParaDestinoAsync(nomeFicheiro, conteudo, cancellationToken);
    }

    public async Task ImprimirTextoAsync(
        string titulo,
        string conteudo,
        CancellationToken cancellationToken = default)
    {
        var nomeFicheiro = $"{Sanitizar(titulo)}-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        await EscreverParaDestinoAsync(nomeFicheiro, conteudo, cancellationToken);
    }

    public Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    public static string GerarPreview(ReciboVendaDto recibo, DadosLoja dadosLoja)
    {
        var cultura = ObterCulturaFormatacao();
        var sb = new StringBuilder();

        void Centralizar(string texto)
        {
            foreach (var linha in QuebrarTexto(texto))
            {
                var espacos = Math.Max(0, (LarguraColunas - linha.Length) / 2);
                sb.Append(' ', espacos).AppendLine(linha);
            }
        }

        void Separador(char caractere = '-') =>
            sb.AppendLine(new string(caractere, LarguraColunas));

        Centralizar(dadosLoja.Nome);

        if (!string.IsNullOrWhiteSpace(dadosLoja.Nif))
        {
            Centralizar($"NIF: {dadosLoja.Nif}");
        }

        Separador();

        sb.AppendLine($"FATURA {recibo.VendaId.ToString()[..8].ToUpperInvariant()}");
        sb.AppendLine($"Data:     {recibo.Data.ToLocalTime():dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Operador: {recibo.OperadorNome}");
        sb.AppendLine("Cliente:  Consumidor Final");

        Separador();

        sb.AppendLine("DESCRIÇÃO");
        sb.AppendLine("QTD                              TOTAL");

        foreach (var item in recibo.Itens)
        {
            foreach (var linha in QuebrarTexto(item.ProdutoNome))
            {
                sb.AppendLine(linha);
            }

            var quantidade = item.QuantidadeNaApresentacao.ToString("0.##", cultura);
            var total = item.ValorTotal.ToString("N2", cultura);
            var linhaQuantidadeValor =
                $"  {quantidade} {item.ApresentacaoNome}".PadRight(22)
                + total.PadLeft(LarguraColunas - 22);

            sb.AppendLine(linhaQuantidadeValor);
        }

        Separador();

        sb.AppendLine(LinhaValor("Subtotal:", recibo.Subtotal, cultura));
        sb.AppendLine(LinhaValor("TOTAL A PAGAR:", recibo.Total, cultura));

        Separador();

        sb.AppendLine($"Pagamento: {recibo.MetodoPagamento}");

        Separador('=');

        if (!string.IsNullOrWhiteSpace(dadosLoja.Endereco))
        {
            Centralizar(dadosLoja.Endereco);
        }

        var localizacao = string.Join(
            " - ",
            new[] { dadosLoja.Municipio, dadosLoja.Provincia }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(localizacao))
        {
            Centralizar(localizacao);
        }

        if (!string.IsNullOrWhiteSpace(dadosLoja.Contacto))
        {
            Centralizar($"Tel: {dadosLoja.Contacto}");
        }

        if (!string.IsNullOrWhiteSpace(dadosLoja.Website))
        {
            Centralizar(dadosLoja.Website);
        }

        sb.AppendLine();
        Centralizar("Obrigado pela preferência!");
        Centralizar("Volte Sempre!");

        Separador();

        Centralizar("PROCESSADO POR COMPUTADOR");
        Centralizar("KiVenda Desktop");

        return sb.ToString();
    }

    private static string LinhaValor(string rotulo, decimal valor, CultureInfo cultura)
    {
        var valorTexto = $"{valor.ToString("N2", cultura)} Kz";
        return rotulo.PadRight(Math.Max(1, LarguraColunas - valorTexto.Length)) + valorTexto;
    }

    private async Task EscreverParaDestinoAsync(
        string nomeFicheiro,
        string conteudo,
        CancellationToken cancellationToken)
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

    private static IEnumerable<string> QuebrarTexto(string texto)
    {
        const int largura = LarguraColunas;
        if (string.IsNullOrWhiteSpace(texto))
        {
            yield break;
        }

        var palavras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var linha = new StringBuilder();

        foreach (var palavra in palavras)
        {
            if (linha.Length > 0 && linha.Length + palavra.Length + 1 > largura)
            {
                yield return linha.ToString();
                linha.Clear();
            }

            if (linha.Length > 0)
            {
                linha.Append(' ');
            }

            linha.Append(palavra);
        }

        if (linha.Length > 0)
        {
            yield return linha.ToString();
        }
    }

    private static CultureInfo ObterCulturaFormatacao()
    {
        var cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return cultura;
    }
}