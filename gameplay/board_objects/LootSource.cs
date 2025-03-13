using Godot;

namespace CrimeChaosAndCatnip;

public partial class LootSource : BoardObject {
	
	[Signal]
	public delegate void LootedEventHandler();

	[Export] private int _lootType = 0;

	[Export] private int _lootAmount = 100;
	
	[Export] private PackedScene _lootedScene;

	public override bool BlocksField => false;

	public bool TryLoot(PlayerUnit unit) {
		if (unit.LootType != _lootType)
			return false;
		
		unit.IncreaseLoot(_lootAmount);

		if (_lootedScene != null) {
			var c = _lootedScene.Instantiate<Node3D>();
			GetParent().AddChild(c);
			c.Position = Position;
		}
		
		EmitSignalLooted();
		
		QueueFree();
		
		return true;
	}

}
