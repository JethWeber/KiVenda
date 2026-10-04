# Impressão térmica ESC/POS — KiVenda

## Objetivo

O KiVenda usa o padrão Epson ESC/POS para imprimir comprovativos de venda em impressoras térmicas compatíveis.

O motor de comandos é o pacote open-source **ESCPOS_NET 3.0.0**, usado para gerar os bytes ESC/POS. A biblioteca é MIT e declara compatibilidade com .NET Standard 2.0 e, por compatibilidade, .NET 10.

## Fluxo

```text
FinalizarVendaUseCase
        |
        | ReciboVendaDto
        v
VendasView
        |
        | confirmação/preview
        v
IServicoImpressao
        |
        v
ServicoImpressaoEscPosUsb
        |
        +--> GeradorEscPos (ESCPOS_NET / EPSON)
        |
        v
ITransporteImpressora
        |
        +--> WindowsSpooler
        +--> Rede TCP/IP
        +--> Serial / USB-Serial
        +--> Dispositivo Linux
```

A Application não conhece impressoras nem ESC/POS.

## Ligações suportadas

### Windows

- **Impressora instalada:** usa o Windows Print Spooler em RAW.
- **Rede:** IP/hostname + porta TCP, normalmente 9100.
- **Serial / USB-Serial:** porta COM + baud rate.

A deteção automática consulta impressoras instaladas e portas seriais.

### Linux

- **USB/dispositivo local:** `/dev/usb/lp*`.
- **Serial / USB-Serial:** `/dev/ttyUSB*`, `/dev/ttyACM*` ou `/dev/serial/by-id/*`.
- **Rede:** IP/hostname + porta TCP.

A deteção automática consulta os dispositivos locais conhecidos. Uma ligação de rede é configurada manualmente.

## Configuração persistida

- tipo de conexão;
- dispositivo local/porta, quando aplicável;
- IP/hostname e porta, quando for rede;
- baud rate, quando for serial;
- largura em colunas;
- linhas de alimentação final;
- corte automático;
- codificação;
- ativo/inativo.

O sistema adapta as opções apresentadas ao sistema operativo.

## Codificação

O padrão do KiVenda é `cp850`. A impressora precisa suportar a code page correspondente; caso contrário, caracteres como `ã`, `ç` ou outros podem sair incorretos.

## Impressão

O recibo inclui nome comercial, NIF quando configurado, identificador da venda, data/hora, operador, itens, apresentações, quantidades, subtotal, total, método de pagamento, valor pago, troco, contactos da loja, alimentação final e corte quando ativado.

O documento é identificado como **RECIBO DE VENDA**, não como fatura fiscal.

## Falhas

Uma falha de impressão não desfaz a venda. A venda já foi persistida pela Application antes da impressão.

O PDV apresenta a situação como **Venda concluída, mas a impressão falhou.**

## Teste

1. Testar impressão.
2. Confirmar caracteres acentuados.
3. Confirmar alinhamento e largura.
4. Confirmar alimentação e corte.
5. Fazer uma venda real de teste.
6. Desligar a impressora e confirmar que o PDV não perde a venda.
7. Voltar a ligar a impressora e testar novamente.

## Nota de implementação

A versão atual do ESCPOS_NET usa uma fila interna em `BasePrinter.Write` para alguns transportes. Para evitar perder bytes quando um objeto de impressora é criado e descartado no mesmo ciclo da operação, o KiVenda usa ESCPOS_NET para gerar comandos ESC/POS, `ImmediateNetworkPrinter` para TCP/IP e I/O direto do sistema operativo para spooler RAW Windows e dispositivos/portas locais.
