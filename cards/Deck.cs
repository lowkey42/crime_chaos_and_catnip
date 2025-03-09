#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Deck : Node {

	[Export] private DeckEntry[] Cards = [];

	private readonly List<CardBase> _liveCards = [];

	public override void _Ready() {
		foreach (var card in Cards)
			for (var i = 0; i < card.Count; i++)
				_liveCards.Add(card.Card);

		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(_liveCards));
	}

	public CardBase? TryDrawCard() {
		if (_liveCards.Count == 0)
			return null;

		var card = _liveCards[^1];
		_liveCards.RemoveAt(_liveCards.Count - 1);
		return card;
	}

}
