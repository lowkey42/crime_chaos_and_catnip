using Godot;

namespace CrimeChaosAndCatnip;

public partial class PlayerUnit : Unit {

	[Export] private int _lootType = 0;

	public int LootType => _lootType;
	
}
