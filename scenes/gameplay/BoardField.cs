using System.Collections.Generic;
using Godot;

namespace CrimeChaosAndCatnip;

public class BoardField {

	public Vector2I PositionGridSpace { get; private set; }

	public Vector3 PositionWorldSpace { get; private set; }

	public readonly List<IInteractable> Content;

	public BoardField(BoardField other) {
		PositionGridSpace = other.PositionGridSpace;
		PositionWorldSpace = other.PositionWorldSpace;
		Content = [..other.Content];
	}

	public BoardField(Vector2I positionGridSpace, Vector3 positionWorldSpace) {
		PositionGridSpace = positionGridSpace;
		PositionWorldSpace = positionWorldSpace;
		Content = [];
	}

}
