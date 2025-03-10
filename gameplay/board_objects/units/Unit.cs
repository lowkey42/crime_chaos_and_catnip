using System;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Unit : BoardObject {

	[Signal]
	public delegate void LootCollectedEventHandler(int value);
	
	
	[Export] public BoardOrientation MovementDirection = BoardOrientation.North;

	[Export] public int MovementLeft = 0;

	public override bool BlocksField => false;

	public bool WantsToMove => MovementLeft > 0;

	public int CollectedLoot { get; private set; } = 0;

	public bool Stunned = false;

	public Vector2I MoveTarget => !WantsToMove
		? Vector2I.Zero
		: MovementDirection switch {
			BoardOrientation.North => BoardPosition + new Vector2I(0, 1),
			BoardOrientation.South => BoardPosition + new Vector2I(0, 1),
			BoardOrientation.East => BoardPosition + new Vector2I(1, 0),
			BoardOrientation.West => BoardPosition + new Vector2I(-1, 0),
			_ => throw new ArgumentOutOfRangeException()
		};

	public void IncreaseLoot(int value) {
		CollectedLoot += value;
		EmitSignalLootCollected(value);
	}

}
