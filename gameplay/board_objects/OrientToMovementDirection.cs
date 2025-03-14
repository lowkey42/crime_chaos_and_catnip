using Godot;

namespace CrimeChaosAndCatnip;

public partial class OrientToMovementDirection : Node3D {

	[Export] private Unit _unit;
	
	public override void _Process(double delta) {
		base._Process(delta);

		if (_unit != null)
			GlobalRotationDegrees = GlobalRotationDegrees with {
				Y = _unit.MovementDirection switch {
					BoardOrientation.North => 180f,
					BoardOrientation.South => 0f,
					BoardOrientation.East => 90f,
					BoardOrientation.West => 270f,
					_ => 0f
				}
			};
	}

}
