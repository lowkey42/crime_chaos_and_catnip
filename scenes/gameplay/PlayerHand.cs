using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class PlayerHand : Node {

	[Export] private int _cardCount;

	// TODO: how do we want to manage cards? child nodes under this one or primarily in code?

	public async Task<bool> TryDrawCards(Deck deck) {
		await Task.Delay(100); // TODO: draw cards from deck, return false if the deck doesn't contain enough cards

		return true;
	}

}
