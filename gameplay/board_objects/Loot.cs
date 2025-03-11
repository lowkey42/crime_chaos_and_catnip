using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Loot : BoardObject {

	[Export] public int Value = 100;

	public override bool BlocksField => false;

	public override bool TryStack(BoardObject otherObject) {
		if (otherObject is Loot loot) {
			Value += loot.Value;
			return true;
		}

		return false;
	}

	public override InteractResult TryInteract(Unit unit) {
		unit.IncreaseLoot(Value);
		return InteractResult.Interacted;
	}

}
