using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class CardSummon : CardBase {

	public override bool CanBePlayedAt(CardAccessibleState state) {
		return base.CanBePlayedAt(state) && !state.TargetCell.Objects.Any(obj => obj is Unit);
	}

}
