using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardMove : CardBase {

	[Export] public int Distance = 10;

	public override bool CanBePlayedAt(CardAccessibleState state) { // TODO
		return true;
	}

	public override void PlayAt(CardAccessibleState state) { // TODO
	}

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		unit.MovementLeft = Distance;
		unit.MovementDirection = card.BoardOrientation;
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.BlockMovement;
	}

}
