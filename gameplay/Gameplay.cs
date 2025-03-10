using System;
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


	[Export] private Deck _deck;

	[Export] private PlayerHand _hand;

	[Export] private Board _board;

	private State _currentState = State.Shuffle;

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

		await Task.Delay(100); // TODO[gameplay]: placeholder for unit movement

		// Turn is over => next turn
		EmitSignalTurnDone();
		EmitSignalTurnStarted();

		await Draw();
	}

	private async Task Draw() {
		if (_currentState != State.Shuffle && _currentState != State.Acting)
			throw new InvalidOperationException($"Invalid state transition from {_currentState} to Drawing");

		_currentState = State.Drawing;
		EmitSignalDrawing();

		if (!await _hand.TryDrawCards(_deck)) {
			// TODO: game over, not enough cards
			_currentState = State.GameOver;
			EmitSignalGameOver();
			return;
		}

		_currentState = State.PlayingCards;
		EmitSignalPlayingCards();
		// after this step, return is controlled to the player and EndTurn() is called
	}

}
