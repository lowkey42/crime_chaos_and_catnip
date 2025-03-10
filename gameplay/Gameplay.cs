using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Gameplay : Node {

	private enum State {

		Shuffle,
		Drawing,
		PlayingCards,
		Acting,
		GameOver

	}

	[Signal]
	public delegate void TurnStartedEventHandler();

	[Signal]
	public delegate void DrawingEventHandler();

	[Signal]
	public delegate void PlayingCardsEventHandler();

	[Signal]
	public delegate void ActingEventHandler();

	[Signal]
	public delegate void TurnDoneEventHandler();

	[Signal]
	public delegate void GameOverEventHandler();

	[Signal]
	public delegate void UnitExitedEventHandler(int loot);


	[Export] private Deck _deck;

	[Export] private PlayerHand _hand;

	[Export] private Board _board;

	[Export] private float _stepTime = 0.5f;

	[Export] public int Score { get; private set; }

	private State _currentState = State.Shuffle;

	private readonly List<Unit> _unitsMovingInNextStep = [];
	private int[,] _unitsMoveTargets;

	public bool CanEndTurn() {
		return _currentState == State.PlayingCards && _hand.CanEndTurn();
	}

	public bool CanPlayCards() {
		return _currentState == State.PlayingCards;
	}

	public void EndTurn() {
		if (!CanEndTurn())
			throw new InvalidOperationException("Can't end turn at this point");

		// execute automatic action asynchronously
		_ = Act();
	}

	private async Task Act() {
		if (_currentState != State.PlayingCards)
			throw new InvalidOperationException($"Invalid state transition from {_currentState} to Acting");

		_currentState = State.Acting;
		EmitSignalActing();

		_unitsMoveTargets = _board.ResizeToBoardDimensions(_unitsMoveTargets);
		
		var didAnyUnitAct = false;
		do {
			await Task.Delay(TimeSpan.FromSeconds(_stepTime));
			
			didAnyUnitAct = false;
			_unitsMovingInNextStep.Clear();
			foreach (var unit in _board.GetUnits()) {
				if (unit.Stunned) // skip stunned units
					continue;
				
				var cell = _board.GetCell(unit.BoardPosition);

				// check if the unit is controlled by the player and the cell is an exit
				if (unit is PlayerUnit playerUnit && cell.Objects.Any(obj => obj is Exit)) {
					Score += playerUnit.CollectedLoot;
					EmitSignalUnitExited(playerUnit.CollectedLoot);
					unit.Stunned = true;
					unit.QueueFree();
				}

				// check if there is an actionable card/object on the units field
				var index = cell.Objects.FindLastIndex(obj => obj.TryInteract(unit));
				if (index != -1) {
					didAnyUnitAct = true;
				} else if(unit.WantsToMove) {
					didAnyUnitAct = true;
					_unitsMovingInNextStep.Add(unit);
				}
			}

			MoveUnits();
		} while (didAnyUnitAct);

		EmitSignalTurnDone();
		
		// remove stunned status from all units after the turn
		foreach (var unit in _board.GetUnits()) {
			unit.Stunned = false;
		}

		EmitSignalTurnStarted();

		await Draw();
	}

	private void MoveUnits() {
		if (_unitsMovingInNextStep.Count == 0)
			return;
			
		// remove all movers, whose target field is occupied by a non-mover
		// repeat, until there are no more such movers left
		var removed = 0;
		do {
			removed = 0;
			for (var i = _unitsMovingInNextStep.Count - 1; i >= 0; i--) {
				var unit = _unitsMovingInNextStep[i];
				if (_board.IsBlocked(unit.BoardPosition, _unitsMovingInNextStep)) {
					unit.Stunned = true;
					_unitsMovingInNextStep.RemoveAt(i);
					removed++;
				}
			}
		} while (removed > 0);

		if (_unitsMovingInNextStep.Count == 0)
			return;
			
		// clear previous mover targets
		for (var x = 0; x < _unitsMoveTargets.GetLength(0); x++) {
			for (var y = 0; y < _unitsMoveTargets.GetLength(1); y++) {
				_unitsMoveTargets[x, y] = 0;
			}
		}

		// mark all target fields of movers
		foreach (var unit in _unitsMovingInNextStep) {
			var target = unit.MoveTarget;
			_unitsMoveTargets[target.X, target.Y]++;
		}
			
		// stun all movers whose target field has multiple members, mark their origin as occupied and remove them
		do {
			removed = 0;
			for (var i = _unitsMovingInNextStep.Count - 1; i >= 0; i--) {
				var unit = _unitsMovingInNextStep[i];
				var target = unit.MoveTarget;
				if (_unitsMoveTargets[target.X, target.Y] > 1) {
					unit.Stunned = true;
					_unitsMoveTargets[unit.BoardPosition.X, unit.BoardPosition.Y]++;
					_unitsMovingInNextStep.RemoveAt(i);
					removed++;
				}
			}
		} while (removed > 0);

		// execute movements
		var moveTween = CreateTween();
		foreach (var unit in _unitsMovingInNextStep) {
			var target = _board.GetCell(unit.MoveTarget).Position;
			moveTween.TweenProperty(unit, "position", target, _stepTime);
		}
	}

	private async Task Draw() {
		if (_currentState != State.Shuffle && _currentState != State.Acting)
			throw new InvalidOperationException($"Invalid state transition from {_currentState} to Drawing");

		_currentState = State.Drawing;
		EmitSignalDrawing();

		if (!await _hand.TryDrawCards(_deck)) { // game over, not enough cards
			_currentState = State.GameOver;
			EmitSignalGameOver();
			return;
		}

		_currentState = State.PlayingCards;
		EmitSignalPlayingCards();
		// after this step, return is controlled to the player and EndTurn() is called
	}

}
