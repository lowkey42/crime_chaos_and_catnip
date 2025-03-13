using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardMove : CardBase {

	[Export] public int Distance = 10;

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		if (unit is not PlayerUnit)
			return BoardObject.InteractResult.Ignored;
		
		unit.MovementLeft = Distance;
		unit.MovementDirection = card.BoardOrientation;
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
