namespace CrimeChaosAndCatnip;

public partial class Exit : BoardObject {

	public override bool BlocksField => false;

	public override InteractResult TryInteract(Unit unit) {
		return unit is PlayerUnit ? InteractResult.Interacted : InteractResult.Ignored;
	}

}
