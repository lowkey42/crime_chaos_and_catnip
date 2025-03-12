using Godot;
using CrimeChaosAndCatnip;

[GlobalClass]
public partial class Hud : CanvasLayer {

	[Export] private Label _scoreLabel;

	[Export] private Label _remainingCardsLabel;

	[Export] private Button _endTurnButton;

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
	}
	
	public void TryEndTurn() {
		_gameplay.EndTurn();
	}

	private void OnCardPlayed(CardBase card, BoardObject spawn) {
		if (spawn is not OrientedBoardObject obj)
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
