namespace KiVenda.Desktop.ViewModels.Common;

/// <summary>
/// Um item do menu lateral (Secção 8 da documentação funcional: 10
/// módulos no menu principal). <see cref="FabricaConteudo"/> só é
/// invocada quando o item é selecionado — os módulos não são todos
/// construídos/carregados antecipadamente.
/// </summary>
public sealed class ItemMenuLateral
{
    public required string Nome { get; init; }

    public required string Icone { get; init; }

    public required Func<ViewModelBase> FabricaConteudo { get; init; }
}
