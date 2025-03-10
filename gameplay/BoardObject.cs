#nullable enable
using Godot;

namespace CrimeChaosAndCatnip;

public abstract partial class BoardObject : Node3D {

	public abstract bool BlocksField { get; }

	public Vector2I GridPosition => Board.ToBoardPosition(GlobalPosition);

	public Board? Board { get; private set; }

	public override void _EnterTree() {
		base._EnterTree();

		// find the board we are on
		foreach (var board in GetTree().GetNodesInGroup("Board")) {
			if (board is Board b && IsAncestorOf(board)) {
				Board = b;
				break;
			}
		}
	}

	public override void _ExitTree() {
		base._ExitTree();
		Board = null;
	}

}
