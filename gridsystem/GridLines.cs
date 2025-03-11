using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

public partial class GridLines : Node3D {

	[Export] private Color _colorBlocked = Color.Color8(255, 255, 255);

	[Export] private Color _colorPlayer = Color.Color8(255, 255, 255);

	[Export] private Color _colorEnemy = Color.Color8(255, 255, 255);

	[Export] private Color _colorLoot = Color.Color8(255, 255, 255);

	[Export] private Color _colorOther = Color.Color8(255, 255, 255);

	[Export] private Color _colorEmpty = Color.Color8(255, 255, 255);
	
	[Export] private PackedScene _gridCellScene;
	
	private Sprite3D[,] _cells;

	private Board _board;
	
	public override void _EnterTree() {
		// find the board we are on
		foreach (var node in GetTree().GetNodesInGroup("Board")) {
			if (node is Board b && IsAncestorOf(this)) {
				_board = b;
				break;
			}
		}

		if (_board == null) {
			GD.PrintErr("GridLines without a Board parent => self destructing");
			QueueFree();
			return;
		}

		_cells = _board.ResizeToBoardDimensions(_cells);
		for (var x = 0; x < _cells.GetLength(0); x++) {
			for (var y = 0; y < _cells.GetLength(1); y++) {
				_cells[x, y] = _gridCellScene.Instantiate<Sprite3D>();
				_cells[x, y].Position = _board.GetCell(new Vector2I(x, y)).Position;
				OnBoardChanged(new Vector2I(x, y));
			}
		}
		
		_board.BoardChanged += OnBoardChanged;
	}

	private void OnBoardChanged(Vector2I boardPosition) {
		var cell = _board.GetCell(boardPosition);
		_cells[boardPosition.X, boardPosition.Y].Modulate = GetCellColor(cell);
	}

	private Color GetCellColor(Board.Cell cell) {
		if (cell.IsBlocked) return _colorBlocked;
		if (cell.Objects.Any(obj => obj is PlayerUnit)) return _colorPlayer;
		if (cell.Objects.Any(obj => obj is EnemyUnit)) return _colorEnemy;
		if (cell.Objects.Any(obj => obj is Loot)) return _colorLoot;
		if (cell.Objects.Count > 0) return _colorOther;
		return _colorEmpty;
	}
}
