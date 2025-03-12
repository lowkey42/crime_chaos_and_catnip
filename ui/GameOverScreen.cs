using Godot;

namespace CrimeChaosAndCatnip;

public partial class GameOverScreen : CanvasLayer
{
	[Export] private CanvasLayer _hud;
	
	[Export] private Label _messageLabel;
	
	[Export] private Button _continueButton;
	
	[Export(PropertyHint.File, "*.tscn,")] private string _nextScenePath;
	
	[Export] private Gameplay _gameplay;
	
	[Export] private PlayerHand _hand;

	public void ShowOverlay() {
		_messageLabel.Text = _messageLabel.Text
			.Replace("{LOOT}", _gameplay.Score.ToString())
			.Replace("{CARDS}", _hand.TotalPlayedCards.ToString());

		_continueButton.GrabFocus();

		Visible = true;
		_hud.Visible = false;
	}

	public void Continue() {
		GetTree().ChangeSceneToFile(_nextScenePath);
	}
	
}
