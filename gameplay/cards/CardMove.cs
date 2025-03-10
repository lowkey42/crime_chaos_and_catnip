using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardMove : CardBase {

	[Export] public int Distance = 10;

	public override bool CanBePlayedAt(CardAccessibleState state) { // TODO
		return true;
	}

	public override void PlayAt(CardAccessibleState state) { // TODO
	}

}
