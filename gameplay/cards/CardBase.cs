using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardBase : Resource {

	[Export] public string Name { get; private set; }

	[Export] public string Description { get; private set; }

	[Export] public Texture2D CardSprite { get; private set; }

	[Export] public PackedScene SpawnOnPlay { get; private set; }

	[Export] public bool RequiresOrientation { get; private set; } = false;
	
	private PackedScene _particleEffect;

	public virtual bool CanBePlayedAt(CardAccessibleState state) {
		return !state.TargetCell.IsBlocked;
	}

	public BoardObject PlayAt(CardAccessibleState state) {
		BoardObject spawned = null;
		
		if (SpawnOnPlay != null) {
			spawned = (BoardObject) SpawnOnPlay.Instantiate();
			if (spawned is PlayedCard pc)
				pc.Card = this;
			state.Board.AddChild(spawned);
			spawned.GlobalPosition = state.TargetCell.Position;
			
			var particleEffect = GD.Load<PackedScene>("res://scenes/level/effekt_tests.tscn").Instantiate<Node3D>();
			state.Board.AddChild(particleEffect);
			var particlesList = particleEffect.GetChildren().OfType<GpuParticles3D>().ToList();
			foreach (var particle in particlesList) {
				particle.GlobalPosition = state.TargetCell.Position + new Vector3(0, 1f,0);
				particle.Emitting = true; // Starte die Partikel-Emission
			}
		}
		
		OnPlayed(state);
		return spawned;
	}

	protected virtual void OnPlayed(CardAccessibleState state) { }
	
	public virtual void OnDiscard(PlayerHand hand) { }

	public virtual BoardObject.InteractResult PlayedCardInteraction(PlayedCard card, Unit unit) {
		return BoardObject.InteractResult.Ignored;
	}

}
