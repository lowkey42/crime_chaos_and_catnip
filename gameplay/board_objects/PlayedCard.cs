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
		if(_sprite==null) GD.PrintErr("Sprite of PlayedCard not set");
		if(Card==null) GD.PrintErr("Card of PlayedCard not set");
		if(Card?.CardSprite==null) GD.PrintErr("Card in PlayedCard has no texture");
		
		_sprite?.SetTexture(Card?.CardSprite);
	}

	public override InteractResult TryInteract(Unit unit) {
		return Card.PlayedCardInteraction(this, unit);
	}

}
