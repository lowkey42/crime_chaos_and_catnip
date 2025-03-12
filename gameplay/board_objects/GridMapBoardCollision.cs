using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class GridMapBoardCollision : GridMap {

	[Export] private int[] _nonBlockingItems = [];

	private Board _board;
	
	public override void _Ready() {
		base._Ready();
		
		_board = Board.GetBoard(this);
		if (_board == null) {
			GD.PrintErr("No Board found for GridMapBoardCollision");
			return;
		}

		if (_board.IsNodeReady())
			OnBoardReady();
		else
			_board.Ready += OnBoardReady;
	}

	private void OnBoardReady() {
		foreach(var gridMapPosition in GetUsedCells()) {
			if(_nonBlockingItems.Contains(GetCellItem(gridMapPosition)))
				continue;

			var localPosition = ToGlobal(MapToLocal(gridMapPosition));
			var start = Board.ToBoardPosition(localPosition - CellSize/2);
			var end = Board.ToBoardPosition(localPosition + CellSize/2);

			for (var x = start.X; x < end.X; x++) {
				for (var y = start.Y; y < end.Y; y++) {
					_board.BoardMarkAsAlwaysBlocked(new Vector2I(x, y));
				}
			}
		}
	}

}
