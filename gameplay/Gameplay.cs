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
		LastTurn,
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
	public delegate void LastTurnEventHandler();

	[Signal]
	public delegate void GameOverEventHandler();

	[Signal]
	public delegate void UnitExitedEventHandler(int loot);


	[Export] private Deck _deck;

	[Export] private PlayerHand _hand;

	private Board _board;

	[Export] private float _stepTime = 0.5f;

	[Export] public int Score { get; private set; }

	[Export] public int Turns { get; private set; }

	private State _currentState = State.Shuffle;

	private readonly List<Unit> _unitsMovingInNextStep = [];
	private readonly List<Unit> _unitsStunnedInNextStep = [];
	private int[,] _unitsMoveTargets;

	public override void _Ready() {
		_board = Board.GetBoard(this);
		_ = Warmup();
	}

	public async Task Warmup() {
		_currentState = State.Shuffle;
		await _deck.ShuffleAnimation();
		await Draw();
	}

	public bool CanEndTurn() {
		return _currentState == State.PlayingCards && _hand.CanEndTurn();
	}

	public int CardsOverLimit() {
		return _hand.CardsOverLimit();
	}

	public bool CanPlayCards() {
		return _currentState is State.PlayingCards or State.LastTurn;
	}

	public void EndTurn() {
		if (!CanEndTurn())
			throw new InvalidOperationException("Can't end turn at this point");

		// execute automatic action asynchronously
		_ = Act();
	}

	public bool CanEndGame() {
		return _currentState == State.LastTurn;
	}

	public void EndGame() {
		if (!CanEndGame())
			return;
		
		_currentState = State.GameOver;
		EmitSignalGameOver();
	}

	private async Task Act() {
		if (_currentState != State.PlayingCards)
			throw new InvalidOperationException($"Invalid state transition from {_currentState} to Acting");

		Turns++;
		
		_currentState = State.Acting;
		EmitSignalActing();

		_board.GridLines?.Fade(0.1f);
		
		_unitsMoveTargets = _board.ResizeToBoardDimensions(_unitsMoveTargets);
		
		var didAnyUnitAct = false;
		do {
			await Task.Delay(TimeSpan.FromSeconds(_stepTime));
			
			didAnyUnitAct = false;
			_unitsMovingInNextStep.Clear();
			_unitsStunnedInNextStep.Clear();
			foreach (var unit in _board.GetUnits()) {
				if (unit.IsQueuedForDeletion() || unit.Stunned) // skip stunned units
					continue;
				
				unit.OnStep();
				
				var cell = _board.TryGetCell(unit.BoardPosition);
				if (cell == null)
					continue; // ignore units outside board area

				// check if the unit is controlled by the player and the cell is an exit
				if (unit is PlayerUnit playerUnit && cell.Objects.Any(obj => obj is Exit)) {
					Score += playerUnit.CollectedLoot;
					EmitSignalUnitExited(playerUnit.CollectedLoot);
					unit.QueueFree();
					continue;
				}

				// check if there is an actionable card/object on the units field
				var interactResult = cell.TryInteract(unit);
				if (interactResult.HasFlag(BoardObject.InteractResult.Interacted)) {
					didAnyUnitAct = true;
				} 
				
				if(!interactResult.HasFlag(BoardObject.InteractResult.BlockMovement) && unit.WantsToMove) {
					_unitsMovingInNextStep.Add(unit);
					if(unit is PlayerUnit)
						didAnyUnitAct = true;
				}
			}

			MoveUnits();
		} while (didAnyUnitAct);

		EmitSignalTurnDone();
		
		_board.OnTurnEnd();

		_board.GridLines?.Fade(1f);

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
				if (_board.IsBlockedOrOccupied(unit.MoveTarget, _unitsMovingInNextStep)) {
					if(unit.Stun())
						_unitsStunnedInNextStep.Add(unit);
					_unitsMovingInNextStep.RemoveAt(i);
					removed++;
				}
			}
		} while (removed > 0);

		// remove movers that would cross path (origin and destination swapped between them)
		for (var i = 0; i<_unitsMovingInNextStep.Count; i++) {
			var unitI = _unitsMovingInNextStep[i];

			for (var j = i + 1; j < _unitsMovingInNextStep.Count; j++) {
				var unitJ = _unitsMovingInNextStep[j];

				if (unitI.BoardPosition == unitJ.MoveTarget && unitI.MoveTarget == unitJ.BoardPosition) {
					if(unitI.Stun())
						_unitsStunnedInNextStep.Add(unitI);
					if(unitJ.Stun())
						_unitsStunnedInNextStep.Add(unitJ);
				}
			}
		}
		_unitsMovingInNextStep.RemoveAll(unit => unit.Stunned);
		
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
					if(unit.Stun())
						_unitsStunnedInNextStep.Add(unit);
					_unitsMoveTargets[unit.BoardPosition.X, unit.BoardPosition.Y]++;
					_unitsMovingInNextStep.RemoveAt(i);
					removed++;
				}
			}
		} while (removed > 0);

		// solve enemy attacks
		foreach (var unitA in _board.GetUnits()) {
			if (unitA is not EnemyUnit enemy)
				continue;

			if (!enemy.Stunned)
				continue; // enemies that could step freely, can't have caught a player

			var anyHit = false;
			foreach (var unitB in _board.GetUnits()) {
				if (unitB is not PlayerUnit player)
					continue;

				if (player.BoardPosition == enemy.BoardPosition || player.BoardPosition == enemy.MoveTarget
				                                                || player.MoveTarget == enemy.BoardPosition ||
				                                                player.MoveTarget == enemy.MoveTarget) {
					player.Kill(enemy);
					anyHit = true;
				}
			}
			
			if(anyHit)
				enemy.OnAttackHit();
		}
		
		// execute movements
		var moveTween = CreateTween();
		moveTween.SetParallel();
		foreach (var unit in _unitsMovingInNextStep) {
			var target = _board.GetCell(unit.MoveTarget).Position;
			moveTween.TweenProperty(unit, "global_position", target, _stepTime);
			unit.MovementLeft--;
		}
		// execute half movement + bounce for stunned units
		foreach (var unit in _unitsStunnedInNextStep) {
			unit.MovementLeft = 0;
			
			var currentPosition = _board.GetCell(unit.BoardPosition).Position;
			var targetPosition = _board.GetCell(unit.MoveTarget).Position;
			var halfPoint = currentPosition.Lerp(targetPosition, 0.5f);

			var stunTween = unit.CreateTween();
			stunTween.TweenProperty(unit, "global_position", halfPoint, _stepTime/2f);
			stunTween.TweenProperty(unit, "global_position", currentPosition, _stepTime/2f).SetTrans(Tween.TransitionType.Elastic);
			moveTween.TweenSubtween(stunTween);
			
		}
	}

	private async Task Draw() {
		if (_currentState != State.Shuffle && _currentState != State.Acting)
			throw new InvalidOperationException($"Invalid state transition from {_currentState} to Drawing");

		_currentState = State.Drawing;
		EmitSignalDrawing();

		if (!await _hand.TryDrawCards(_deck)) { // game over, not enough cards
			_currentState = State.LastTurn;
			EmitSignalLastTurn();
			return;
		}

		_currentState = State.PlayingCards;
		EmitSignalPlayingCards();
		// after this step, return is controlled to the player and EndTurn() is called
	}

}
