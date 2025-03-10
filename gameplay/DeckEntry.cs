using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class DeckEntry : Resource {

	[Export] public CardBase Card;

	[Export] public int Count = 1;

}
