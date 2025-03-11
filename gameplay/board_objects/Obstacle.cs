using Godot;

namespace CrimeChaosAndCatnip.board_objects;

[GlobalClass]
public partial class Obstacle : BoardObject {

	public override bool BlocksField => true;

}
