using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class GridMapBoardCollision : GridMap {

	[Export] private int[] _nonBlockingItems = [];
	
	public override void _Ready() {
		base._Ready();
		
		var board = Board.GetBoard(this);
		if (board == null)
			return;

		foreach(var gridMapPosition in GetUsedCells()) {
			if(_nonBlockingItems.Contains(GetCellItem(gridMapPosition)))
				continue;

			var localPosition = MapToLocal(gridMapPosition);
			var start = localPosition.Floor(); // TODO[test]: position might be offset, if the cell position isn't the bottom left but the center
			var end = start + CellSize;

			for (var x = start.X; x < end.X; x++) {
				for (var z = start.Z; z < end.Z; z++) {
					var boardPosition = board.ToNullableBoardPosition(new Vector3(x, 0, z));
					if (boardPosition != null)
						board.BoardMarkAsAlwaysBlocked(boardPosition.Value);
				}
			}
		}
	}

}
