using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardBase : Resource {

	[Export] public string Name { get; set; }

	[Export] public string Description { get; set; }

	[Export] public Texture CardSprite { get; set; }

	// TODO[API]: Probably needs more state, at least the current hand, for cards that discard/draw cards
	public virtual bool CanBePlayedAt(BoardField field) {
		return true;
	}

	public virtual bool CanBePlayed() {
		return true;
	}

	// TODO[API]: Probably needs more state, at least the current hand, for cards that discard/draw cards
	public virtual void PlayAt(BoardField field) { }

}
