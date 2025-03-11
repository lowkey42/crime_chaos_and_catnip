using Godot;
using System;
using System.Diagnostics;
using CrimeChaosAndCatnip.Level.Entities;

public partial class MoveEntity : Control
{
	[Export] private Player Player; 
	[Export] private Vector3 MoveDirection; 
	[Export] private int MoveSteps; 

	private void Pressed()
	{
		if (Player != null)
		{
			GD.PrintErr("Player is currently pressed");
			Player.MovePlayer(MoveDirection, MoveSteps);
		}
		else
		{
			GD.PrintErr("Kein Spieler zugewiesen!");
		}
	}
}
