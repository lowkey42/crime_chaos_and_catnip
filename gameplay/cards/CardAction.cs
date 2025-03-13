using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardAction : CardBase {

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		if (!(unit is PlayerUnit p))
			return BoardObject.InteractResult.Ignored;
		
		// TODO: if the unit is on a LootSource => farm loot
		
		// TODO: look at the 3x3 fields around the unit and destroy all enemies and destructure objects
		
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
