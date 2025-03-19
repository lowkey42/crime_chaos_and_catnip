#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Deck : Node2D {

	[Export] private int _firstHandMinUnits = 2;

	[Export] private int _firstHandMinMoves = 2;

	[Export] private int _firstHandMinActions = 1;

	[Export] private int _handSize = 8;
	
	[Export] private DeckEntry[] _cards = [];

	[Export] private PackedScene _heldCardScene = null!;
	
	private AnimationPlayer _animationPlayer = null!;

	private readonly List<HeldCard?> _liveCards = [];
	
	public int RemainingCards => _liveCards.Count;

	public void SetCards(List<DeckEntry> cards) {
		_cards = cards.ToArray();
		ShuffleDeck();
	}
	
	public override void _EnterTree() {
		base._EnterTree();
		ShuffleDeck();
	}

	public void ShuffleDeck() {
		foreach (var child in GetChildren()) {
			child.QueueFree();
		}
		_liveCards.Clear();

		var unitCards = new List<HeldCard>();
		var moveCards = new List<HeldCard>();
		var actionCards = new List<HeldCard>();
		var otherCards = new List<HeldCard>();
		
		// instantiate cards by type
		foreach (var card in _cards) {
			for (var i = 0; i < card.Count; i++) {
				var heldCard = (HeldCard) _heldCardScene.Instantiate();
				heldCard.Card = card.Card;
				
				if(card.Card is CardSummon) unitCards.Add(heldCard);
				else if(card.Card is CardMove) moveCards.Add(heldCard);
				else if(card.Card is CardAction) actionCards.Add(heldCard);
				else otherCards.Add(heldCard);
			}
		}
		
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(unitCards));
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(moveCards));
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(actionCards));
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(otherCards));
		
		// construct the first hand
		var cardCount = _cards.Sum(c => c.Count);
		for (var i = 0; i < _firstHandMinUnits; i++) {
			_liveCards.Add(unitCards[unitCards.Count - 1]);
			unitCards.RemoveAt(unitCards.Count - 1);
		}
		for (var i = 0; i < _firstHandMinMoves; i++) {
			_liveCards.Add(moveCards[moveCards.Count - 1]);
			moveCards.RemoveAt(moveCards.Count - 1);
		}
		for (var i = 0; i < _firstHandMinActions; i++) {
			_liveCards.Add(actionCards[actionCards.Count - 1]);
			actionCards.RemoveAt(actionCards.Count - 1);
		}
		
		// distribute some of the cards evenly
		var start = _liveCards.Count;
		for (var i = start; i < cardCount; i++) {
			_liveCards.Add(null);
		}
		DistributeCards(cardCount, start, unitCards);
		DistributeCards(cardCount, start, moveCards);
		DistributeCards(cardCount, start, actionCards);

		// fill the remaining deck with random cards
		var remainingCards = new List<HeldCard>();
		remainingCards.AddRange(unitCards);
		remainingCards.AddRange(moveCards);
		remainingCards.AddRange(actionCards);
		remainingCards.AddRange(otherCards);
		Random.Shared.Shuffle(CollectionsMarshal.AsSpan(remainingCards));
		for (var i = 0; i < cardCount; i++) {
			if (_liveCards[i] == null) {
				_liveCards[i] = remainingCards[remainingCards.Count - 1];
				remainingCards.RemoveAt(remainingCards.Count - 1);
			}
		}

		
		// shuffle each card-chunk of size _handSize
		for (var i = 0; i < _liveCards.Count; i+=_handSize) {
			Random.Shared.Shuffle(CollectionsMarshal.AsSpan(_liveCards).Slice(i, Math.Min(_handSize, _liveCards.Count-i)));
		}

		_liveCards.Reverse();

		foreach (var heldCard in _liveCards) {
			AddChild(heldCard);
			// TODO[ui]: init positions and other other data and animate
		}
	}

	private void DistributeCards(int cardCount, int start, List<HeldCard> cards) {
		var stepAction = Math.Max(_handSize, (cardCount - start) / cards.Count);
		for (var i = start; i < cardCount && cards.Count>0; i+=stepAction) {
			for (; i < cardCount; i++) {
				if (_liveCards[i] == null) {
					_liveCards[i] = cards[cards.Count - 1];
					cards.RemoveAt(cards.Count - 1);
					break;
				}
			}
		}
	}

	public async Task ShuffleAnimation() {
		await Task.Delay(1);
	}

	public HeldCard? TryDrawCard() {
		if (_liveCards.Count == 0)
			return null;

		var card = _liveCards[^1];
		_liveCards.RemoveAt(_liveCards.Count - 1);
		RemoveChild(card);
		card!.FlipCard();
		return card;
	}

}
