using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Gameplay : Node {

	[Signal]
	public delegate void TurnStartingEventHandler();

	[Signal]
	public delegate void TurnStartedEventHandler();

	[Export] private Deck _deck;

	[Export] private PlayerHand _hand;

	[Export] private BoardState _boardState;

	public async Task StartTurn() {
		EmitSignalTurnStarting();

		if (!await _hand.TryDrawCards(_deck)) {
			// TODO: game over, not enough cards
		}

		_boardState.OnTurnStart();

		EmitSignalTurnStarted();
	}

	// TODO: core game loop: state-machine / async-await or something else

}
