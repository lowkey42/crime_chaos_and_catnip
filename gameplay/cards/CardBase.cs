using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardBase : Resource {

	[Export] public string Name { get; set; }

	[Export] public string Description { get; set; }

	[Export] public Texture2D CardSprite { get; set; }

	public virtual bool CanBePlayedAt(CardAccessibleState state) {
		return true;
	}

	public virtual void PlayAt(CardAccessibleState state) { }

	public virtual BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		return BoardObject.InteractResult.Ignored;
	}

}
