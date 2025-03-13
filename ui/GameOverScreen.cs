using Godot;

namespace CrimeChaosAndCatnip;

public partial class GameOverScreen : CanvasLayer
{
	[Export] private CanvasLayer _hud;
	
	[Export] private Label _lootLabel;
	
	[Export] private Label _turnsLabel;
	
	[Export] private Label _cardsLabel;
	
	[Export] private Label _messageLabel;
	
	[Export] private Button _continueButton;
	
	[Export(PropertyHint.File, "*.tscn,")] private string _nextScenePath;
	
	[Export] private Gameplay _gameplay;
	
	[Export] private PlayerHand _hand;

	public void ShowOverlay() {
		_lootLabel.Text = _gameplay.Score.ToString();
		_turnsLabel.Text = _gameplay.Turns.ToString();
		_cardsLabel.Text = _hand.TotalPlayedCards.ToString();

		_continueButton.GrabFocus();

		Visible = true;
		_hud.Visible = false;
	}

	public void Continue() {
		GetTree().ChangeSceneToFile(_nextScenePath);
	}
	
}
