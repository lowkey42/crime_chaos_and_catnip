using Godot;

namespace CrimeChaosAndCatnip;

public partial class UnitButton : MarginContainer
{
	[Signal]
	public delegate void ActivatedEventHandler();
	
	public void OnPressed() {
		EmitSignalActivated();
	}
	
}
