using System;
using System.Collections.Generic;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Board : Node {

	[Export] private Vector2I _maxGridSize;

	public class Cell(Vector3 position) {

		public readonly Vector3 Position = position;
		public readonly Vector2I BoardPosition = ToBoardPosition(position);
		public readonly List<BoardObject> Objects = [];

	}

	private Cell[,] _cells;

	public override void _Ready() {
		base._Ready();
		_cells = new Cell[_maxGridSize.X, _maxGridSize.Y];
		for (var x = 0; x < _maxGridSize.X; x++)
		for (var y = 0; y < _maxGridSize.Y; y++)
			_cells[x, y] = new Cell(new Vector3(x, 0, y)); // TODO: determine height (3D Y) from map?
	}

	public static Vector2I ToBoardPosition(Vector3 position) {
		return new Vector2I((int) (position.X + 0.5f), (int) (position.Z + 0.5f));
	}

	public void AddObject(Vector2I boardPosition, BoardObject obj) {
		ValidateBoardPosition(boardPosition);
		_cells[boardPosition.X, boardPosition.Y].Objects.Add(obj);
	}

	public void RemoveObject(Vector2I boardPosition, BoardObject obj) {
		ValidateBoardPosition(boardPosition);
		_cells[boardPosition.X, boardPosition.Y].Objects.Remove(obj);
	}

	public void MoveObject(Vector2I oldBoardPosition, Vector2I newBoardPosition, BoardObject obj) {
		ValidateBoardPosition(oldBoardPosition);
		ValidateBoardPosition(newBoardPosition);
		if (oldBoardPosition.X == newBoardPosition.X && oldBoardPosition.Y == newBoardPosition.Y)
			return;

		_cells[oldBoardPosition.X, oldBoardPosition.Y].Objects.Remove(obj);
		_cells[newBoardPosition.X, newBoardPosition.Y].Objects.Add(obj);
	}

	public Cell GetCell(Vector2I boardPosition) {
		ValidateBoardPosition(boardPosition);
		return _cells[boardPosition.X, boardPosition.Y];
	}


	private void ValidateBoardPosition(Vector2I boardPosition) {
		if (boardPosition.X < 0 || boardPosition.Y < 0)
			throw new ArgumentException("Invalid board position");
		if (boardPosition.X > _maxGridSize.X || boardPosition.Y > _maxGridSize.Y)
			throw new ArgumentException("Invalid board position");
	}

}
