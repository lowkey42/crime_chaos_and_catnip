#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Board : Node {

	public const int CellSize = 2;
	
	[Signal]
	public delegate void BoardChangedEventHandler(Vector2I boardPosition);
	
	[Signal]
	public delegate void UnitAddedEventHandler(Unit unit);
	
	[Signal]
	public delegate void UnitRemovedEventHandler(Unit unit);

	[Export] public GridLines? GridLines { get; private set; }
	
	[Export] private Vector2I _maxGridSize;

	[Export] private GridMap? _gridMap;

	public Vector2I Size => _maxGridSize;

	public class Cell(Vector3 position) {

		public readonly Vector3 Position = position;
		public readonly Vector2I BoardPosition = ToBoardPosition(position);
		public readonly List<BoardObject> Objects = [];

		public bool AlwaysBlocked = false;
		
		public bool IsBlocked => AlwaysBlocked || Objects.Any(obj => obj.BlocksField);

		public bool TryStack(BoardObject obj) {
			foreach(var existingObj in Objects)
				if (existingObj.TryStack(obj))
					return true;
			return false;
		}

		public BoardObject.InteractResult TryInteract(Unit unit) {
			var result = BoardObject.InteractResult.Ignored;
			
			for (var i = Objects.Count - 1; i >= 0; i--) {
				var interactResult = Objects[i].TryInteract(unit);
				result |= interactResult;
				
				if (interactResult.HasFlag(BoardObject.InteractResult.Interacted))
					Objects[i].OnInteracted();
				
				if (interactResult.HasFlag(BoardObject.InteractResult.RemoveSelf)) {
					Objects[i].QueueFree();
					Objects.RemoveAt(i); // remove immediately so the object can't be consumed by other units during the same tick
				}

				if (interactResult.HasFlag(BoardObject.InteractResult.BlockFurtherInteraction))
					break;
			}

			return result;
		}

	}

	private Cell[,]? _cells;

	private readonly List<Unit> _units = [];

	public override void _Ready() {
		/*
		if (_gridMap != null) {
			foreach (var cell in _gridMap.GetUsedCells()) {
				var p = _gridMap.ToGlobal(_gridMap.MapToLocal(cell) + _gridMap.CellSize);
				_maxGridSize = _maxGridSize.Max(ToBoardPosition(p));
			}
		}
		*/
		
		_cells = new Cell[_maxGridSize.X, _maxGridSize.Y];
		for (var x = 0; x < _maxGridSize.X; x++)
		for (var y = 0; y < _maxGridSize.Y; y++)
			_cells[x, y] = new Cell(new Vector3(x, 0, y)*CellSize); // TODO: determine height (3D Y) from map?
	}

	public T[,] ResizeToBoardDimensions<T>(T[,]? previous) {
		if(_cells==null)
			throw new NullReferenceException("_cells is null because _Ready() hasn't been called, yet");
		
		if(previous!=null && previous.GetLength(0)>=_cells.GetLength(0) && previous.GetLength(1)>=_cells.GetLength(1))
			return previous;
		
		return new T[_cells.GetLength(0), _cells.GetLength(1)];
	}

	public static Board? GetBoard(Node node) {
		foreach (var board in node.GetTree().GetNodesInGroup("Board")) {
			if (board is Board b) {
				return b;
			}
		}
		return null;
	}
	
	public static Vector2I ToBoardPosition(Vector3 position) {
		return new Vector2I((int) (position.X/CellSize + 0.5f), (int) (position.Z/CellSize + 0.5f));
	}

	public Rect2 BoardWorldDimensions() {
		if(_cells==null)
			throw new NullReferenceException("_cells is null because _Ready() hasn't been called, yet");
		
		var p = _cells[0, 0].Position;
		var s = _cells[_cells.GetLength(0) - 1, _cells.GetLength(1) - 1].Position - p;
		return new Rect2(new Vector2(p.X, p.Z), new Vector2(s.X, s.Z));
	}
	
	public Vector2I? ToNullableBoardPosition(Vector3 position) {
		var boardPosition = ToBoardPosition(position);
		return TryGetCell(boardPosition) != null ? boardPosition : null;
	}
	
	public void AddObject(Vector2I boardPosition, BoardObject obj) {
		if (boardPosition.X < 0 || boardPosition.Y < 0)
			return;
		if (boardPosition.X >= _maxGridSize.X || boardPosition.Y >= _maxGridSize.Y)
			return;
		
		ValidateBoardPosition(boardPosition);

		Debug.Assert(_cells != null, nameof(_cells) + " != null");
		var cell = _cells[boardPosition.X, boardPosition.Y];
		
		if(cell.TryStack(obj))
			return;
		AddSorted(obj, cell);

		if (obj is Unit unit) {
			_units.Add(unit);
			EmitSignalUnitAdded(unit);
		}

		EmitSignalBoardChanged(boardPosition);
	}

	private static void AddSorted(BoardObject obj, Cell cell) {
		var index = cell.Objects.BinarySearch(obj, Comparer<BoardObject>.Create((lhs, rhs) => lhs.Priority-rhs.Priority));
		if (index < 0) index = ~index;
		cell.Objects.Insert(index, obj);
	}

	public void RemoveObject(Vector2I boardPosition, BoardObject obj) {
		if (boardPosition.X < 0 || boardPosition.Y < 0)
			return;
		if (boardPosition.X >= _maxGridSize.X || boardPosition.Y >= _maxGridSize.Y)
			return;

		ValidateBoardPosition(boardPosition);
		Debug.Assert(_cells != null, nameof(_cells) + " != null");
		_cells[boardPosition.X, boardPosition.Y].Objects.Remove(obj);
		if (obj is Unit unit) {
			_units.Remove(unit);
			EmitSignalUnitRemoved(unit);
		}

		EmitSignalBoardChanged(boardPosition);
	}

	public void MoveObject(Vector2I oldBoardPosition, Vector2I newBoardPosition, BoardObject obj) {
		if (oldBoardPosition.X < 0 || oldBoardPosition.Y < 0)
			return;
		if (oldBoardPosition.X >= _maxGridSize.X || oldBoardPosition.Y >= _maxGridSize.Y)
			return;
		
		if (newBoardPosition.X < 0 || newBoardPosition.Y < 0)
			return;
		if (newBoardPosition.X >= _maxGridSize.X || newBoardPosition.Y >= _maxGridSize.Y)
			return;


		ValidateBoardPosition(oldBoardPosition);
		ValidateBoardPosition(newBoardPosition);
		if (oldBoardPosition.X == newBoardPosition.X && oldBoardPosition.Y == newBoardPosition.Y)
			return;

		Debug.Assert(_cells != null, nameof(_cells) + " != null");
		_cells[oldBoardPosition.X, oldBoardPosition.Y].Objects.Remove(obj);
		
		var cell = _cells[newBoardPosition.X, newBoardPosition.Y];
		if(cell.TryStack(obj))
			return;
		AddSorted(obj, cell);
		EmitSignalBoardChanged(oldBoardPosition);
		EmitSignalBoardChanged(newBoardPosition);
	}

	public Cell GetCell(Vector2I boardPosition) {
		ValidateBoardPosition(boardPosition);
		Debug.Assert(_cells != null, nameof(_cells) + " != null");
		return _cells[boardPosition.X, boardPosition.Y];
	}
	public Cell? TryGetCell(Vector2I boardPosition) {
		if (boardPosition.X < 0 || boardPosition.Y < 0)
			return null;
		if (boardPosition.X >= _maxGridSize.X || boardPosition.Y >= _maxGridSize.Y)
			return null;
		Debug.Assert(_cells != null, nameof(_cells) + " != null");
		return _cells[boardPosition.X, boardPosition.Y];
	}


	private void ValidateBoardPosition(Vector2I boardPosition) {
		if (boardPosition.X < 0 || boardPosition.Y < 0)
			throw new ArgumentException("Invalid board position");
		if (boardPosition.X >= _maxGridSize.X || boardPosition.Y >= _maxGridSize.Y)
			throw new ArgumentException("Invalid board position");
		if(_cells==null)
			throw new NullReferenceException("_cells is null because _Ready() hasn't been called, yet");
	}

	public IEnumerable<Unit> GetUnits() {
		return _units;
	}

	public bool IsBlocked(Vector2I boardPosition, IEnumerable<BoardObject> exclude) {
		var cell = TryGetCell(boardPosition);
		if (cell == null)
			return true;

		return cell.Objects.Any(obj => obj.BlocksField && !exclude.Contains(obj));
	}
	
	public bool IsBlockedOrOccupied(Vector2I boardPosition, IEnumerable<BoardObject> exclude) {
		var cell = TryGetCell(boardPosition);
		if (cell == null || cell.AlwaysBlocked)
			return true;

		return cell.Objects.Any(obj => (obj.BlocksField || obj is Unit) && !exclude.Contains(obj));
	}

	public void BoardMarkAsAlwaysBlocked(Vector2I boardPosition) {
		var cell = TryGetCell(boardPosition);
		if (cell is {AlwaysBlocked: false}) {
			cell.AlwaysBlocked = true;
			EmitSignalBoardChanged(boardPosition);
		}
	}

	public void OnTurnEnd() {
		// remove stunned status from all units after the turn
		foreach (var unit in GetUnits()) {
			unit.OnTurnEnd();
		}
	}

	public Vector3 GetWorldPosition(Vector2I boardPosition) {
		var cell = TryGetCell(boardPosition);
		return cell?.Position ?? new Vector3(boardPosition.X * CellSize, 0, boardPosition.Y * CellSize);
	}

}
