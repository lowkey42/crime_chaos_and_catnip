#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class PlayerHand : Node2D {

	[Export] private int HandRadius = 200;
	[Export] private float CardAngleLimit = 90.0f;
	[Export] private float MaxCardSpreadAngle = 20f;
	[Export] private Deck _deck = null!;
	[Export] private Board? _board;

	private readonly List<HeldCard> _heldCards = [];
	private readonly List<HeldCard> _touchedCards = [];

	private int _currentSelectedCardIndex = -1;
	private HeldCard? _highlightedCard = null;

	[Export] public CollisionShape2D _discardArea = null!;


	[Export] private int _maxCardCount = 5;
	[Export] private int _maxCardAtTurnEnd = 3;

	public override void _Ready() {
		RepositionCards();
	}

	public override void _Process(double delta) {
		// Entferne Highlight von allen Karten
		foreach (var card in _heldCards) {
			card.Unhighlight();
		}

		// Hebe nur die oberste Karte hervor
		if (_touchedCards.Count > 0) {
			// Finde die oberste Karte (die zuletzt berührte Karte)
			_currentSelectedCardIndex = _heldCards.FindLastIndex(card => _touchedCards.Contains(card));
			if (_currentSelectedCardIndex != -1)
				_heldCards[_currentSelectedCardIndex].Highlight();
		} else {
			_currentSelectedCardIndex = -1; // Keine Karte ausgewählt
		}
	}

	public CollisionShape2D GetCollisionShape2D() {
		return _discardArea;
	}


// Fügt eine Karte zur Hand hinzu
    public void AddCard(HeldCard card)
    {
        _heldCards.Add(card);
        AddChild(card);
        RepositionCards();
    }

    // Positioniert die Karten neu
    private void RepositionCards()
    {
	    if (_heldCards.Count == 0) return;

	    // Berechne den Winkelabstand zwischen den Karten
	    float cardSpread = Mathf.Min(CardAngleLimit / _heldCards.Count, MaxCardSpreadAngle);
	    float currentAngle = -((cardSpread * (_heldCards.Count - 1)) / 2) - 90; // Startwinkel
	    GD.Print("CardSpread" + cardSpread);

	    foreach (var card in _heldCards)
	    {
		    UpdateCardTransform(card, currentAngle);
		    currentAngle += cardSpread;
	    }
    }
    
    private Vector2 GetCardPosition(float angleInDegrees)
    {
		    float angleInRadians = Mathf.DegToRad(angleInDegrees);
		    float x =  HandRadius * Mathf.Cos(angleInRadians);
		    float y =  HandRadius * Mathf.Sin(angleInRadians);
		    

		    return new Vector2(x, y);
    }

    // Aktualisiert die Position und Rotation einer Karte
    private void UpdateCardTransform(HeldCard card, float angleInDegrees)
    {
        card.Position = GetCardPosition(angleInDegrees);
        card.Rotation = -card.Position.AngleTo(Vector2.Up);
        card.Rotation = card.Position.Angle() + Mathf.Pi / 2;
    }
    
    // Zieht eine Karte vom Deck und fügt sie zur Hand hinzu
    public async Task DrawCard()
    {
        var card = _deck.TryDrawCard();
        if (card != null)
        {
            AddCard(card);
            await Task.Delay(100); // Platzhalter für Animation
        }
        else
        {
            GD.Print("Keine Karten mehr im Deck!");
        }
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


    // Entfernt eine Karte aus der Hand
    public async Task DiscardCard(HeldCard card)
    {
        _heldCards.Remove(card);
        await Task.Delay(100); // Platzhalter für Animation
        card.Card?.OnDiscard(this);
        card.QueueFree();
        RepositionCards();
    }

    // Überprüft, ob der Spieler seinen Zug beenden kann
    public bool CanEndTurn()
    {
        return _heldCards.Count <= _maxCardAtTurnEnd;
    }

    public bool CanBePlayedAt(HeldCard heldCard, Vector2I boardPosition) {
	    var state = GetCardAccessibleState(boardPosition);
	    return state!=null && heldCard.Card.CanBePlayedAt(state);
    }

    private CardAccessibleState? GetCardAccessibleState(Vector2I boardPosition) {
	    var cell = _board?.TryGetCell(boardPosition);
	    return cell == null 
		    ? null 
		    : new CardAccessibleState() {Board = _board!, PlayerHand = this, TargetCell = cell};
    }

    public void PlayAt(HeldCard heldCard, Vector2I boardPosition) {
	    var state = GetCardAccessibleState(boardPosition);
	    if(state != null)
		    heldCard.Card.PlayAt(state);
	    else
		    GD.PrintErr("Couldn't play card, because the PlayerHand state isn't fully initialized!");
    }
    
    public void HandleCardHovered(HeldCard card)
    {
	    if (!_touchedCards.Contains(card))
	    {
		    _touchedCards.Add(card);
	    }
    }

    public void HandleCardUnhovered(HeldCard card)
    {
	    if (_touchedCards.Contains(card))
	    {
		    _touchedCards.Remove(card); 
	    }
    }

    
}
