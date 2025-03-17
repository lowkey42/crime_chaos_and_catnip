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
		
		_endGameButton.Visible = _gameplay.CanEndGame();
		_endGameButton.Disabled = !_gameplay.CanEndTurn();
		
		_turnCounterLabel.Text = _gameplay.Turns.ToString();

		if (_cardSpawn != null) {
			PositionRotationUi();
		}
	}
	
	public void TryEndTurn() {
		_gameplay.EndTurn();
	}

	private void OnCardPlayed(CardBase card, BoardObject spawn) {
		if (!card.RequiresOrientation || spawn is not OrientedBoardObject obj)
			return;

		var hidden = _cardSpawn == null;
		_cardSpawn = obj;
		if(hidden)
			AddChild(_rotationUi);
		PositionRotationUi();
	}

	private void PositionRotationUi() {
		var camera = GetViewport().GetCamera3D();
		_rotationUi.GlobalPosition = camera.UnprojectPosition(_cardSpawn.GlobalPosition);
		_rotationUi.RotationDegrees = camera.GlobalRotationDegrees.Y;
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
