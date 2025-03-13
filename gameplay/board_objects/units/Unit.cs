using System;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Unit : BoardObject {

	[Signal]
	public delegate void LootCollectedEventHandler(int value);
	
	[Signal]
	public delegate void StunAddedEventHandler();
	
	[Signal]
	public delegate void StunClearedEventHandler();
	
	[Signal]
	public delegate void KilledEventHandler();
	
	
	[Export] public BoardOrientation MovementDirection = BoardOrientation.North;

	[Export] public int MovementLeft = 0;

	[Export] public int SelfDestruct = -1;

	[Export] private Label _lootLabel;
	
	[Export] private PackedScene _lootScene;

	public override bool BlocksField => false;

	public bool WantsToMove => MovementLeft > 0;

	[Export] public int CollectedLoot { get; private set; } = 0;

	public bool Stunned { get; private set; } = false;

	public Vector2I MoveTarget => !WantsToMove
		? Vector2I.Zero
		: MovementDirection switch {
			BoardOrientation.North => BoardPosition + new Vector2I(0, -1),
			BoardOrientation.South => BoardPosition + new Vector2I(0, 1),
			BoardOrientation.East => BoardPosition + new Vector2I(1, 0),
			BoardOrientation.West => BoardPosition + new Vector2I(-1, 0),
			_ => throw new ArgumentOutOfRangeException()
		};

	public override void _Ready() {
		base._Ready();
		if(_lootLabel != null)
			_lootLabel.Text = CollectedLoot.ToString();
	}

	public void IncreaseLoot(int value) {
		CollectedLoot += value;
		if(_lootLabel != null)
			_lootLabel.Text = CollectedLoot.ToString();
		EmitSignalLootCollected(value);
	}

	public void Kill() {
		// drop loot at current field
		if (CollectedLoot > 0 && _lootScene!=null) {
			var droppedLoot = _lootScene.Instantiate<Loot>();
			droppedLoot.Value = CollectedLoot;
			GetParent().AddChild(droppedLoot);
			droppedLoot.Position = Position;
			CollectedLoot = 0;
		}
		
		EmitSignalKilled();
		Stunned = true;
		QueueFree();
	}

	public void ClearStunned() {
		if(Stunned)
			EmitSignalStunCleared();
		
		Stunned = false;
	}

	public bool Stun() {
		if (Stunned)
			return false;
		
		EmitSignalStunAdded();
		Stunned = true;
		return true;
	}

	public void OnTurnEnd() {
		ClearStunned();
		
		if (SelfDestruct > 0) {
			SelfDestruct--;
			if(SelfDestruct==0)
				Kill();
		}
	}

}
