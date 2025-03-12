using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardMove : CardBase {

	[Export] public int Distance = 10;

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		unit.MovementLeft = Distance;
		unit.MovementDirection = card.BoardOrientation;
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
