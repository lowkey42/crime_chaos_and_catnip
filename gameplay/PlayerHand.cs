#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class PlayerHand : Control {
	[Signal]
	public delegate void CardPlayedEventHandler(CardBase card, BoardObject spawn);

	[Export] private int _handRadius = 200;
	[Export] private float _yFactor = 0.6f;
	[Export] private float _cardAngleLimit = 90.0f;
	[Export] private float _maxCardSpreadAngle = 20f;
	[Export] private Deck _deck = null!;
	
	[Export] private Control? _discardPile;
	[Export] private Control? _bottomArea;

	private readonly List<HeldCard> _heldCards = [];
	private readonly List<HeldCard> _touchedCards = [];

	private int _currentSelectedCardIndex = -1;
	private HeldCard? _highlightedCard = null;
	
	[Export] private int _maxCardCount = 5;
	[Export] private int _maxCardAtTurnEnd = 3;

	private Board? _board;

	[Export] public int TotalPlayedCards { get; private set; }
	
	public override void _Ready() {
		RepositionCards();
		_board = Board.GetBoard(this);
	}

	public override void _Process(double delta) {
		
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

	public bool IsInPlayableArea(Vector2 screenPosition) {
		return _bottomArea!=null && !_bottomArea.GetGlobalRect().HasPoint(screenPosition);
	}
	public bool IsInDiscardArea(Vector2 screenPosition) {
		return _discardPile!=null && _discardPile.GetGlobalRect().HasPoint(screenPosition);
	}
	
    public async void AddCard(HeldCard card)
    {
        _heldCards.Add(card);
        AddChild(card);
        await RepositionCardsWithTween();
    }

    
    private void RepositionCards()
    {
	    if (_heldCards.Count == 0) return;
	    
	    float cardSpread = Mathf.Min(_cardAngleLimit / _heldCards.Count, _maxCardSpreadAngle);
	    float currentAngle = -((cardSpread * (_heldCards.Count - 1)) / 2) - 90;

	    foreach (var card in _heldCards)
	    {
		    UpdateCardTransform(card, currentAngle);
		    currentAngle += cardSpread;
	    }
    }
    
    private async Task RepositionCardsWithTween()
    {
	    if (_heldCards.Count == 0) return;

	    float cardSpread = Mathf.Min(_cardAngleLimit / _heldCards.Count, _maxCardSpreadAngle);
	    float currentAngle = -((cardSpread * (_heldCards.Count - 1)) / 2 - 90);

	    // Erstelle einen Tween für jede Karte
	    foreach (var card in _heldCards)
	    {
		    Vector2 finalPosition = GetCardPosition(currentAngle);
		    float finalRotation = GetCardRotation(currentAngle);

		    // Erstelle einen Tween für die Position und Rotation der Karte
		    Tween tween = CreateTween();
		    tween.TweenProperty(card, "position", finalPosition, 0.5f)
			    .SetEase(Tween.EaseType.Out)
			    .SetTrans(Tween.TransitionType.Quad);

		    tween.Parallel().TweenProperty(card, "rotation", finalRotation, 0.5f);

		    currentAngle += cardSpread;
	    }

	    // Warte auf das Ende der Animation
	    await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
    }
    
    private Vector2 GetCardPositionForAnimation(HeldCard card)
    {
	    float cardSpread = Mathf.Min(_cardAngleLimit / _heldCards.Count, _maxCardSpreadAngle);
	    float currentAngle = -((cardSpread * (_heldCards.Count - 1)) / 2) - 90;
	    
	    int cardIndex = _heldCards.IndexOf(card);
	    if (cardIndex >= 0)
	    {
		    float angle = currentAngle + cardIndex * cardSpread;
		    return GetCardPosition(angle);
	    }
	    return GetCardPosition(currentAngle + (_heldCards.Count - 1) * cardSpread);
    }
    
    private float GetCardRotationForAnimation(HeldCard card)
    {
	    float cardSpread = Mathf.Min(_cardAngleLimit / _heldCards.Count, _maxCardSpreadAngle);
	    float currentAngle = -((cardSpread * (_heldCards.Count - 1)) / 2) - 90;
	    
	    int cardIndex = _heldCards.IndexOf(card);
	    if (cardIndex >= 0)
	    {
		    float angle = currentAngle + cardIndex * cardSpread;
		    return Mathf.DegToRad(angle + 90);
	    }
	    return Mathf.DegToRad(currentAngle + (_heldCards.Count - 1) * cardSpread + 90);
    }
    
    private Vector2 GetCardPosition(float angleInDegrees)
    {
		    float angleInRadians = Mathf.DegToRad(angleInDegrees);
		    float x =  _handRadius * Mathf.Cos(angleInRadians);
		    float y = _handRadius * Mathf.Sin(angleInRadians) * _yFactor;

		    return new Vector2(x, y);
    }
    
    private float GetCardRotation(float angleInDegrees)
    {
	    float angleInRadians = Mathf.DegToRad(angleInDegrees);
	    return Mathf.Atan2(_handRadius * Mathf.Sin(angleInRadians) * _yFactor, _handRadius * Mathf.Cos(angleInRadians));
    }

    // Aktualisiert die Position und Rotation einer Karte
    private void UpdateCardTransform(HeldCard card, float angleInDegrees)
    {
        // Berechne die Position der Karte
    card.Position = GetCardPosition(angleInDegrees);

    // Berechne die Rotation der Karte basierend auf der elliptischen Form
    float x = card.Position.X;
    float y = card.Position.Y;

    // Steigung der Ellipse an der Position (x, y)
    float slope = -((_handRadius * _yFactor) * (_handRadius * _yFactor) * x) / (_handRadius * _handRadius * y);

    // Berechne die Rotation aus der Steigung
    float rotation = Mathf.Atan(slope);

    // Korrigiere die Rotation, damit die Karte zur Mitte zeigt
    card.Rotation = rotation; // +90 Grad, damit die Karte zur Mitte zeigt
    }
    
    
    public async Task DrawCard()
    {
        var card = _deck.TryDrawCard();
        if (card != null)
        {
            AddCard(card);
            
            //zeigt die endposition und rotation der karte an 
            Vector2 finalPosition = GetCardPositionForAnimation(card);
            float finalRotation = GetCardRotationForAnimation(card);
            
            card.Position = finalPosition;
            card.Rotation = finalRotation;
            card.Scale = Vector2.One;
            card.Position = _deck.GlobalPosition - GlobalPosition; 
            card.Rotation = 0; 
            card.Scale = Vector2.One * 0.5f; 
            
            Tween tween = CreateTween();
            tween.TweenProperty(card, "position", finalPosition, 0.5f)
	            .SetEase(Tween.EaseType.Out) 
	            .SetTrans(Tween.TransitionType.Quad); 
            
            tween.Parallel().TweenProperty(card, "rotation", finalRotation, 0.5f);
            tween.Parallel().TweenProperty(card, "scale", Vector2.One, 0.5f);

            
            await ToSignal(tween, "finished");
            
            await RepositionCardsWithTween();
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
		    
		    //zeigt die endposition und rotation der karte an 
		    Vector2 finalPosition = GetCardPositionForAnimation(card);
		    float finalRotation = GetCardRotationForAnimation(card);
            
		    card.Position = finalPosition;
		    card.Rotation = finalRotation;
		    card.Scale = Vector2.One;
		    card.Position = _deck.GlobalPosition - GlobalPosition; 
		    card.Rotation = 0; 
		    card.Scale = Vector2.One * 0.5f; 
            
		    Tween tween = CreateTween();
		    tween.TweenProperty(card, "position", finalPosition, 0.5f)
			    .SetEase(Tween.EaseType.Out) 
			    .SetTrans(Tween.TransitionType.Quad); 
            
		    tween.Parallel().TweenProperty(card, "rotation", finalRotation, 0.5f);
		    tween.Parallel().TweenProperty(card, "scale", Vector2.One, 0.5f);
		    
		    await ToSignal(tween, "finished");

		    RepositionCards();

	    }

	    return true;
    }
    
    public async Task DiscardCard(HeldCard card)
    {
        _heldCards.Remove(card);
        await Task.Delay(100); // Platzhalter für Animation
        card.Card?.OnDiscard(this);
        card.QueueFree();
        RepositionCards();

    }
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
	    if (state != null) {
		    TotalPlayedCards++;
		    var spawned = heldCard.Card.PlayAt(state);
		    EmitSignalCardPlayed(heldCard.Card, spawned);
		    _heldCards.Remove(heldCard);
		    heldCard.QueueFree();
		    RepositionCards();
	    } else
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


    public void MarkHoveredForbidden(bool forbidden) {
	    if (_board?.GridLines != null) {
		    _board.GridLines.HoverForbidden = forbidden;
	    }
    }

}
