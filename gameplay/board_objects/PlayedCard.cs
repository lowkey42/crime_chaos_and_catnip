using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class PlayedCard : BoardObject {

	public BoardOrientation BoardOrientation { get; private set; }

	[Export] public CardBase Card { get; set; }

	[Export] private Sprite3D _sprite;

	public override bool BlocksField => false;

	public override void _Ready() {
		base._Ready();
		_sprite?.SetTexture(Card?.CardSprite);
	}

	public override InteractResult TryInteract(Unit unit) {
		return Card.PlayedCardInteraction(this, unit);
	}

}
