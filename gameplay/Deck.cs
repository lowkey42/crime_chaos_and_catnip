#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Deck : Node {

	[Export] private DeckEntry[] _cards = [];

	[Export] private PackedScene _heldCardScene = null!;

	private readonly List<HeldCard> _liveCards = [];

	public override void _EnterTree() {
		base._EnterTree();

		foreach (var card in _cards) {
			for (var i = 0; i < card.Count; i++) {
				var heldCard = (HeldCard) _heldCardScene.Instantiate();
				heldCard.Card = card.Card;
				AddChild(heldCard);
				// TODO[ui]: init positions and other other data and animate

				_liveCards.Add(heldCard);
			}
		}

		// TODO[gameplay]: smarter shuffle, to ensure that the first hand contains at least X summonable units
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(_liveCards));
	}

	public HeldCard? TryDrawCard() {
		if (_liveCards.Count == 0)
			return null;

		var card = _liveCards[^1];
		_liveCards.RemoveAt(_liveCards.Count - 1);
		RemoveChild(card);
		return card;
	}

}
