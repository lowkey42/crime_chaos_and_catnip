using System.Collections.Generic;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class BoardState : Node {

	private BoardField[,] _lastBoard;

	private BoardField[,] _nextBoard;

	private readonly List<CardBase> _playedCards = [];

	public override void _Ready() {
		// TODO: init _lastBoard based on ??? and call ResetNextBoard()
	}

	public bool CanCardBePlayedAt(CardBase card, Vector2I position) {
		if (position.X < 0 || position.Y < 0)
			return false;
		if (position.X >= _nextBoard.GetLength(0) || position.Y >= _nextBoard.GetLength(1))
			return false;

		return card.CanBePlayed() && card.CanBePlayedAt(_nextBoard[position.X, position.Y]);
	}

	public bool TryPlayCard(CardBase card, Vector2I position) {
		if (position.X < 0 || position.Y < 0)
			return false;
		if (position.X >= _nextBoard.GetLength(0) || position.Y >= _nextBoard.GetLength(1))
			return false;

		var field = _nextBoard[position.X, position.Y];
		if (field == null)
			return false;

		if (!card.CanBePlayed() || !card.CanBePlayedAt(field))
			return false;

		card.PlayAt(field);
		_playedCards.Add(card);
		return true;
	}

	public void DiscardTurn() {
		// reset played cards and intermediary board state
		_playedCards.Clear();
		ResetNextBoard();
	}

	public void OnTurnStart() {
		// reset played cards replace live board state with the intermediary one
		_playedCards.Clear();
		_lastBoard = _nextBoard;
		ResetNextBoard();
	}

	private void ResetNextBoard() {
		_nextBoard = new BoardField[_nextBoard.GetLength(0), _nextBoard.GetLength(1)];
		for (var x = 0; x < _nextBoard.GetLength(0); x++)
		for (var y = 0; y < _nextBoard.GetLength(1); y++)
			_nextBoard[x, y] = new BoardField(_lastBoard[x, y]);
	}

}
