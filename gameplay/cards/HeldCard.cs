using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class HeldCard : Node2D {

	[Export] public CardBase Card;

	[Export] private Sprite2D _sprite;

	public override void _Ready() {
		base._Ready();
		_sprite?.SetTexture(Card?.CardSprite);
	}

}
