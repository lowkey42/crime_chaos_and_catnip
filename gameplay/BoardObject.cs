#nullable enable
using System;
using System.Diagnostics;
using Godot;

namespace CrimeChaosAndCatnip;

public abstract partial class BoardObject : Node3D {
	
	[Flags] public enum InteractResult {

		Ignored = 0,
		Interacted = 1,
		RemoveSelf = 2,
		BlockFurtherInteraction = 4,
		BlockMovement = 8

	}
	
	[Signal]
	public delegate void InteractedEventHandler();
	
	public abstract bool BlocksField { get; }

	public Vector2I BoardPosition => Board.ToBoardPosition(GlobalPosition);

	public Board? Board { get; private set; }

	public virtual int Priority => 0;
	
	private Vector2I _lastBoardPosition;

	public override void _Ready() {
		base._Ready();

		Board = Board.GetBoard(this);
		if (Board != null) {
			if (Board.IsNodeReady())
				OnBoardReady();
			else
				Board.Ready += OnBoardReady;
		}
	}

	private void OnBoardReady() {
		Debug.Assert(Board != null, nameof(Board) + " != null");
		GlobalPosition = Board.GetWorldPosition(BoardPosition);
		Board.AddObject(BoardPosition, this);
		_lastBoardPosition = BoardPosition;
	}

	public void SnapToGrid() {
		if(Board != null)
			GlobalPosition = Board.GetWorldPosition(BoardPosition);
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
	public virtual InteractResult TryInteract(Unit unit) {
		return InteractResult.Ignored;
	}

	/// <summary>Called every time another BoardObject is placed on this cell.</summary>
	/// <param name="otherObject">The new object</param>
	/// <returns>True if the object has been combined with this one and the new object can be discarded</returns>
	public virtual bool TryStack(BoardObject otherObject) {
		return false;
	}

	public void OnInteracted() {
		EmitSignalInteracted();
	}
	
}
