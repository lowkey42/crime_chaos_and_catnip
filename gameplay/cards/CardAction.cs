using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardAction : CardBase {

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		if (unit is not PlayerUnit p || card.Board==null)
			return BoardObject.InteractResult.Ignored;

		// if the unit is on a LootSource => farm loot
		var cell = card.Board.TryGetCell(card.BoardPosition);
		if (cell != null) {
			for (var i = cell.Objects.Count - 1; i >= 0; i--) {
				var obj = cell.Objects[i];
				if (obj is LootSource ls) {
					ls.TryLoot(p);
				}
			}
		}
		
		// look at the 3x3 fields around the unit and destroy all enemies and destructure objects
		for (var x = -1; x <= 1; x++) {
			for (var y = -1; y <= 1; y++) {
				var boardPosition = card.BoardPosition;
				boardPosition.X += x;
				boardPosition.Y += y;
				cell = card.Board.TryGetCell(boardPosition);
				if (cell != null) {
					foreach (var obj in cell.Objects) {
						if (obj is EnemyUnit enemy) {
							enemy.Kill();
						}
					}
				}
			}
		}
		
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
