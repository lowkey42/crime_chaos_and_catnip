using Godot;
using System;
using CrimeChaosAndCatnip;

public partial class CardDrawButton : Node
{
	[Export] private Gameplay _gameplay;
	[Export] private PlayerHand _playerHand;

	void Pressed() {
		_= _playerHand.DrawCard();
	}
}
