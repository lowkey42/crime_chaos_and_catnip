using Godot;
using CrimeChaosAndCatnip;

public partial class Hud : CanvasLayer {

	[Export] private Label _scoreLabel;

	[Export] private Label _remainingCardsLabel;

	[Export] private Button _endTurnButton;

	[Export] private Gameplay _gameplay;

	[Export] private Deck _deck;

	public override void _Process(double delta) {
		_scoreLabel.Text = _gameplay.Score.ToString();
		_remainingCardsLabel.Text = _deck.RemainingCards.ToString();
		_endTurnButton.Disabled = !_gameplay.CanEndTurn();
	}

	public void TryEndTurn() {
		_gameplay.EndTurn();
	}

}
