#nullable enable
using Godot;

namespace CrimeChaosAndCatnip;

public abstract partial class BoardObject : Node3D {

	public abstract bool BlocksField { get; }

	public Vector2I BoardPosition => Board.ToBoardPosition(GlobalPosition);

	public Board? Board { get; private set; }
	
	private Vector2I _lastBoardPosition;

	public override void _EnterTree() {
		base._EnterTree();

		// find the board we are on
		foreach (var board in GetTree().GetNodesInGroup("Board")) {
			if (board is Board b && IsAncestorOf(board)) {
				Board = b;
				break;
			}
		}
		
		Board?.AddObject(BoardPosition, this);
		_lastBoardPosition = BoardPosition;
	}

	public override void _ExitTree() {
		base._ExitTree();
		Board?.RemoveObject(BoardPosition, this);
		Board = null;
	}

	public override void _Process(double delta) {
		if (_lastBoardPosition != BoardPosition) {
			Board?.MoveObject(_lastBoardPosition, BoardPosition, this);
			_lastBoardPosition = BoardPosition;
		}
	}

	/// <summary>Called when a unit inters this objects cell. Called from top to bottom, if there are multiple objects on the cell</summary>
	/// <param name="unit">The unit that entered the cell</param>
	/// <returns>True if this object has been consumed by the unit</returns>
	public virtual bool TryInteract(Unit unit) {
		return false;
	}

	/// <summary>Called every time another BoardObject is placed on this cell.</summary>
	/// <param name="otherObject">The new object</param>
	/// <returns>True if the object has been combined with this one and the new object can be discarded</returns>
	public virtual bool TryStack(BoardObject otherObject) {
		return false;
	}
}
