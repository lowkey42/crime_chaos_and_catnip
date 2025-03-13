using Godot;

namespace CrimeChaosAndCatnip;

public partial class PlayerUnit : Unit {

	[Export] private int _lootType = 0;

	[Export] public bool CanAttack { get; private set; } = true;

	public int LootType => _lootType;
	
}
