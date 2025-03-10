using System;
using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class Unit : BoardObject {

	[Export] public BoardOrientation MovementDirection = BoardOrientation.North;

	[Export] public int MovementLeft = 0;

	public override bool BlocksField => false;

}
