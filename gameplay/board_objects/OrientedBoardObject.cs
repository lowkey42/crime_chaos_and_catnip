using System;

namespace CrimeChaosAndCatnip;

public abstract partial class OrientedBoardObject : BoardObject {

	private BoardOrientation _boardOrientation;
	
	public BoardOrientation BoardOrientation {
		get => _boardOrientation;
		set {
			_boardOrientation = value;
			RotationDegrees = RotationDegrees with {Y = OrientationAngle(value)};
		}
	}

	private static float OrientationAngle(BoardOrientation orientation) {
		return orientation switch {
				BoardOrientation.North => 0,
				BoardOrientation.South => 180,
				BoardOrientation.East => 270,
				BoardOrientation.West => 90,
				_ => throw new ArgumentOutOfRangeException()
			};
	}

}
