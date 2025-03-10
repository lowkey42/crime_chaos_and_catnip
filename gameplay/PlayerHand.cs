using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class PlayerHand : Node {

	[Export] private Deck _deck;

	[Export] private int _maxCardCount = 5;
	[Export] private int _maxCardAtTurnEnd = 3;

	private readonly List<HeldCard> _heldCards = [];

	public bool CanEndTurn() {
		return _heldCards.Count <= _maxCardAtTurnEnd;
	}

	public async Task DiscardCard(HeldCard card) {
		_heldCards.Remove(card);

		await Task.Delay(100); // TODO[ui]: placeholder for animation and stuff

		RemoveChild(card);
	}

	public async Task<bool> TryDrawCards(Deck deck) {
		for (var i = _heldCards.Count; i < _maxCardCount; i++) {
			var card = _deck.TryDrawCard();
			if (card == null)
				return false; // couldn't draw enough cards => game over

			AddChild(card);
			_heldCards.Add(card);

			await Task.Delay(100); // TODO[ui]: placeholder for animation and stuff
		}

		return true;
	}

}
