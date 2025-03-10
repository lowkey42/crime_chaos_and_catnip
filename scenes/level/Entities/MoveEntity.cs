using Godot;
using System;
using System.Diagnostics;
using CrimeChaosAndCatnip.Level.Entities;

public partial class MoveEntity : Control
{
	[Export] public Player Player; 
	[Export] public Vector3 MoveDirection; 
	[Export] public int MoveSteps; 

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
