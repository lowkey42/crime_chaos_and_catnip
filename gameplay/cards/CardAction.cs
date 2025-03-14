using System.Collections.Generic;
using System.Linq;
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
						if (obj is LootSource ls) {
							ls.TryLoot(p);
						}
						if (obj is EnemyUnit enemy) {
							enemy.Kill();
							var particleEffect = GD.Load<PackedScene>("res://feather_effect.tscn").Instantiate<Node3D>();
							var particlesList = particleEffect.GetChildren().OfType<GpuParticles3D>().ToList();
							enemy.GetTree().Root.AddChild(particleEffect); 
							foreach (var particle in particlesList) {
								GD.Print("Particle: " + particle.Name);
								particle.GlobalPosition = enemy.GlobalPosition + new Vector3(0, 0.5f,1);
								particle.Emitting = true; // Starte die Partikel-Emission
								
							}
						}
					}
				}
			}
		}
		
		
		return BoardObject.InteractResult.Interacted | BoardObject.InteractResult.BlockFurtherInteraction |
		       BoardObject.InteractResult.RemoveSelf;
	}

}
