using Content.Shared._CP14.Cargo;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._CP14.TravelingStoreShip;

[GenerateTypedNameReferences]
public sealed partial class CP14StoreProductControl : Control
{
    [Dependency] private readonly IEntityManager _entity = default!;

    private readonly SpriteSystem _sprite;

    public CP14StoreProductControl(CP14StoreUiProductEntry entry)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _sprite = _entity.System<SpriteSystem>();

        PriceHolder.RemoveAllChildren();
        PriceHolder.AddChild(new CP14PriceControl(entry.Price));
        ProductName.Text = $"[bold]{entry.Name}[/bold]";

        SpecialLabel.Visible = entry.Special;

        if (entry.Icon is not null)
        {
            View.Visible = true;
            View.Texture = _sprite.Frame0(entry.Icon);
        }
        else if (entry.EntityView is not null)
        {
            EntityView.Visible = true;
            EntityView.SetPrototype(entry.EntityView);
        }
    }
}
