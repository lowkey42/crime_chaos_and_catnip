using Godot;
using System.Collections.Generic;
using CrimeChaosAndCatnip;

public partial class DeckSelect : CanvasLayer {

	[Export] private SpinBox _countRaccoon;
	
	[Export] private CardBase _raccoon;

	[Export] private SpinBox _countCat;
	
	[Export] private CardBase _cat;

	[Export] private SpinBox _countChipmunk;
	
	[Export] private CardBase _chipmunk;

	[Export] private SpinBox _countMove5;
	
	[Export] private CardBase _move5;

	[Export] private SpinBox _countMove10;
	
	[Export] private CardBase _move10;

	[Export] private SpinBox _countMove15;
	
	[Export] private CardBase _move15;

	[Export] private SpinBox _countAction;
	
	[Export] private CardBase _action;

	[Export] private SpinBox _countBox;
	
	[Export] private CardBase _box;
	
	[Export] private Deck _deck;
	
	[Export] private Hud _hud;
	
	[Export] private Gameplay _gameplay;

	public override void _Ready() {
		_hud.Visible = false;
	}

	public void StartGame() {
		var cards = new List<DeckEntry>();
		AddEntry(cards, _raccoon, (int) _countRaccoon.Value);
		AddEntry(cards, _cat, (int) _countCat.Value);
		AddEntry(cards, _chipmunk, (int) _countChipmunk.Value);
		
		AddEntry(cards, _move5, (int) _countMove5.Value);
		AddEntry(cards, _move10, (int) _countMove10.Value);
		AddEntry(cards, _move15, (int) _countMove15.Value);
		
		AddEntry(cards, _action, (int) _countAction.Value);
		AddEntry(cards, _box, (int) _countBox.Value);
		
		_deck.SetCards(cards);

		_hud.Visible = true;
		_gameplay.StartGame();
		
		QueueFree();
	}

	private static void AddEntry(List<DeckEntry> list, CardBase card, int count) {
		if (count > 0)
			list.Add(new DeckEntry(){Card = card, Count = count});
	}
	
}
