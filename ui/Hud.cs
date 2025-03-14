using Godot;
using CrimeChaosAndCatnip;

[GlobalClass]
public partial class Hud : CanvasLayer {

	[Export] private Label _scoreLabel;

	[Export] private Label _remainingCardsLabel;

	[Export] private Label _turnCounterLabel;

	[Export] private Button _endTurnButton;

	[Export] private Button _endGameButton;

	[Export] private Gameplay _gameplay;

	[Export] private Deck _deck;

	[Export] private Control _rotationUi;

	private OrientedBoardObject _cardSpawn;

	public override void _Ready() {
		RemoveChild(_rotationUi);
	}

	public override void _Process(double delta) {
		_scoreLabel.Text = _gameplay.Score.ToString();
		_remainingCardsLabel.Text = _deck.RemainingCards.ToString();
		_endTurnButton.Disabled = !_gameplay.CanEndTurn();
		
		_endTurnButton.Visible = !_gameplay.CanEndGame();
		_endGameButton.Visible = _gameplay.CanEndGame();

		_endTurnButton.TooltipText = _gameplay.CanEndTurn() ? "" : $"Play or discard {_gameplay.CardsOverLimit()} more cards to end your turn";
		
		_turnCounterLabel.Text = _gameplay.Turns.ToString();
	}
	
	public void TryEndTurn() {
		_gameplay.EndTurn();
	}
	
	public void TryEndGame() {
		_gameplay.EndGame();
	}

	private void OnCardPlayed(CardBase card, BoardObject spawn) {
		if (!card.RequiresOrientation || spawn is not OrientedBoardObject obj)
			return;

		_cardSpawn = obj;
		
		var camera = GetViewport().GetCamera3D();
		AddChild(_rotationUi);
		_rotationUi.GlobalPosition = camera.UnprojectPosition(spawn.GlobalPosition);
		_rotationUi.RotationDegrees = camera.RotationDegrees.Y;
	}

	public void RotateCardEast() {
		SetSpawnOrientation(BoardOrientation.East);
	}

	public void RotateCardSouth() {
		SetSpawnOrientation(BoardOrientation.South);
	}

	public void RotateCardWest() {
		SetSpawnOrientation(BoardOrientation.West);
	}

	public void RotateCardNorth() {
		SetSpawnOrientation(BoardOrientation.North);
	}

	private void SetSpawnOrientation(BoardOrientation orientation) {
		_cardSpawn.BoardOrientation = orientation;
		_cardSpawn = null;
		RemoveChild(_rotationUi);
	}

}
