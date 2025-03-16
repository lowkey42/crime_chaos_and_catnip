using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class BoardCollision : Node3D {

	[Export] private float _margin = 0.3f;
	
	private Board _board;
	
	public override void _Ready() {
		base._Ready();
		
		_board = Board.GetBoard(this);
		if (_board == null) {
			GD.PrintErr("No Board found for GridMapBoardCollision");
			return;
		}

		if (_board.IsNodeReady())
			OnBoardReady();
		else
			_board.Ready += OnBoardReady;
	}

	private void OnBoardReady() {
		MarkRecursive(this);
	}

	private void MarkRecursive(Node3D node) {
		if (node is MeshInstance3D v) {
			var aabb = v.GetAabb();
			var size = aabb.Size;
			for (var x = _margin; x <= size.X-_margin; x += 0.5f) {
				for (var z = _margin; z <= size.Z-_margin; z += 0.5f) {
					var p = node.ToGlobal(aabb.Position + new Vector3(x, 0, z));
					_board.BoardMarkAsAlwaysBlocked(Board.ToBoardPosition(p));
				}
			}
			return;
		}
		
		foreach(var child in node.GetChildren())
			if(child is Node3D child3d)
				MarkRecursive(child3d);
	}

}
