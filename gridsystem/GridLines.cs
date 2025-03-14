using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

public partial class GridLines : Node3D {

	[Export] private Color _colorHoverForbidden = Color.Color8(255, 0, 0);

	[Export] private Color _colorBlocked = Color.Color8(255, 255, 255);

	[Export] private Color _colorPlayer = Color.Color8(255, 255, 255);

	[Export] private Color _colorEnemy = Color.Color8(255, 255, 255);

	[Export] private Color _colorLoot = Color.Color8(255, 255, 255);

	[Export] private Color _colorOther = Color.Color8(255, 255, 255);

	[Export] private Color _colorExit = Color.Color8(255, 255, 255);

	[Export] private Color _colorEmpty = Color.Color8(255, 255, 255);

	[Export] private float _lineThicknessHovered = 0.2f;

	[Export] private float _lineThicknessEmpty = 0.05f;

	[Export] private float _lineThicknessNoneEmpty = 0.15f;

	[Export] private float _animationAmountUnit = 0.1f;

	[Export] private float _animationAmountHover = 0.2f;

	[Export] private float _animationAmountDefault = 0.1f;

	[Export] private float _animationSpeedUnit = 4f;

	[Export] private float _animationSpeedHover = 1f;

	[Export] private float _animationSpeedDefault = 1f;

	[Export] private float _hoverOvershoot = 0.6f;

	[Export] private float _hoverColorIntensity = 1.5f;

	[Export] private PackedScene _gridCellScene;

	private Sprite3D[,] _cells;

	private Board _board;

	private Vector2I? _hoveredCell;

	public bool HoverForbidden { get; set; }

	public override void _Ready() {
		// find the board we are on
		foreach (var node in GetTree().GetNodesInGroup("Board")) {
			if (node is Board b) {
				_board = b;
				break;
			}
		}

		if (_board == null) {
			GD.PrintErr("GridLines without a Board parent => self destructing");
			QueueFree();
			return;
		}

		if (_board != null) {
			if (_board.IsNodeReady())
				OnBoardReady();
			else
				_board.Ready += OnBoardReady;
		}
	}

	private void OnBoardReady() {
		_cells = _board.ResizeToBoardDimensions(_cells);
		for (var x = 0; x < _cells.GetLength(0); x++) {
			for (var y = 0; y < _cells.GetLength(1); y++) {
				_cells[x, y] = _gridCellScene.Instantiate<Sprite3D>();
				AddChild(_cells[x, y]);
				_cells[x, y].GlobalPosition = _board.GetCell(new Vector2I(x, y)).Position;
				_cells[x, y].Scale = Vector3.One * Board.CellSize;
				OnBoardChanged(new Vector2I(x, y));
			}
		}

		_board.BoardChanged += OnBoardChanged;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		var newHoveredCell = _hoveredCell;
		var camera = GetViewport().GetCamera3D();
		if (camera != null) {
			var mousePosition = GetViewport().GetMousePosition();
			var rayHit = Plane.PlaneXZ.IntersectsRay(camera.ProjectRayOrigin(mousePosition),
				camera.ProjectRayNormal(mousePosition));
			newHoveredCell = rayHit != null ? _board.ToNullableBoardPosition(rayHit.Value) : null;
		}

		if (newHoveredCell != _hoveredCell) {
			var oldHoveredCell = _hoveredCell;
			_hoveredCell = newHoveredCell;
			if (oldHoveredCell != null) OnBoardChanged(oldHoveredCell.Value);
			if (newHoveredCell != null) OnBoardChanged(newHoveredCell.Value);
		}
	}

	private void OnBoardChanged(Vector2I boardPosition) {
		var cell = _board.GetCell(boardPosition);
		var material = (ShaderMaterial) _cells[boardPosition.X, boardPosition.Y].MaterialOverride;

		var cellColor = GetCellColor(cell);
		cellColor *= boardPosition == _hoveredCell ? _hoverColorIntensity : 1f;

		material.SetShaderParameter("grid_color", cellColor);
		material.SetShaderParameter("fade_distance", GetCellLineThickness(cell));
		material.SetShaderParameter("animation_amount", GetCellAnimationAmount(cell));
		material.SetShaderParameter("animation_speed", GetCellAnimationSpeed(cell));
		material.SetShaderParameter("animation_overshoot", boardPosition == _hoveredCell ? _hoverOvershoot : 0f);
	}

	private Color GetCellColor(Board.Cell cell) {
		if (HoverForbidden && cell.BoardPosition == _hoveredCell)
			return _colorHoverForbidden;

		if (cell.IsBlocked) return _colorBlocked;
		if (cell.Objects.Any(obj => obj is Exit)) return _colorExit;
		if (cell.Objects.Any(obj => obj is PlayerUnit)) return _colorPlayer;
		if (cell.Objects.Any(obj => obj is EnemyUnit)) return _colorEnemy;
		if (cell.Objects.Any(obj => obj is Loot or LootSource)) return _colorLoot;
		if (cell.Objects.Count > 0) return _colorOther;
		return _colorEmpty;
	}

	private float GetCellLineThickness(Board.Cell cell) {
		if (cell.BoardPosition == _hoveredCell)
			return _lineThicknessHovered;
		return cell.Objects.Count > 0 ? _lineThicknessNoneEmpty : _lineThicknessEmpty;
	}

	private float GetCellAnimationAmount(Board.Cell cell) {
		if (cell.BoardPosition == _hoveredCell)
			return _animationAmountHover;
		if (cell.Objects.Any(obj => obj is Unit))
			return _animationAmountUnit;
		return _animationAmountDefault;
	}

	private float GetCellAnimationSpeed(Board.Cell cell) {
		if (cell.BoardPosition == _hoveredCell)
			return _animationSpeedHover;
		if (cell.Objects.Any(obj => obj is Unit))
			return _animationSpeedUnit;
		return _animationSpeedDefault;
	}

	public void Fade(float alpha) {
		var targetColor = Color.Color8(255, 255, 255) with {A = alpha};
		var fadeTween = CreateTween();
		fadeTween.SetParallel();
		foreach (var cell in _cells) {
			fadeTween.TweenProperty(cell, "modulate", targetColor, 0.5f);
		}
	}

}
