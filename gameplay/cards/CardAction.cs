using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardAction : CardBase {

	public override BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		if (unit is not PlayerUnit p || !p.CanAttack || card.Board==null)
			return BoardObject.InteractResult.Ignored;
		
		// look at the 3x3 fields around the unit and destroy all enemies and destructure objects
		for (var x = -1; x <= 1; x++) {
			for (var y = -1; y <= 1; y++) {
				var boardPosition = card.BoardPosition;
				boardPosition.X += x;
				boardPosition.Y += y;
				
				var cell = card.Board.TryGetCell(boardPosition);
				if (cell != null) {
					foreach (var obj in cell.Objects) {
						switch (obj) {
							case Loot l:
								l.TryInteract(unit);
								break;
							case LootSource ls:
								ls.TryLoot(p);
								break;
							case EnemyUnit enemy:
								enemy.Kill();
								break;
						}
					}
				}
			}
		}
		
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
