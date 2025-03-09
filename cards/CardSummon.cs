using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardSummon : CardBase {

	public override bool CanBePlayedAt(BoardField field) { // TODO
		return true;
	}

	public override bool CanBePlayed() { // TODO
		return true;
	}

	public override void PlayAt(BoardField field) { // TODO
	}

}
