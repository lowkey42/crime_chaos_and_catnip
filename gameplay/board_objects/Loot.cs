using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Loot : BoardObject {

	[Export] public int Value = 100;

	public override bool BlocksField => false;

}
