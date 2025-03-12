using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class HeldCard : Node2D {
	
	[Signal] public delegate void MouseEnteredEventHandler();
	[Signal] public delegate void MouseExitedEventHandler();

	[Export] public CardBase Card;

	private CollisionShape2D _discardArea;
	
	private CollisionShape2D _snapBackArea;
	

	[Export] private Sprite2D _sprite;
	
	private bool _grabbed = false;
	

	public override void _Ready() {
		base._Ready();
		
		var discardNodes = GetTree().GetNodesInGroup("discardArea");
		if (discardNodes.Count > 0)
		{
			_discardArea = discardNodes[0] as CollisionShape2D;
		}
		var snapBackNodes = GetTree().GetNodesInGroup("cantDropArea");
		if (discardNodes.Count > 0)
		{
			_snapBackArea = snapBackNodes[0] as CollisionShape2D;
		}
		
		_sprite?.SetTexture(Card?.CardSprite);
	}

	private void OnMouseEntered()
	{
		EmitSignal(nameof(MouseEntered));
	}

	private void OnMouseExited()
	{
		EmitSignal(nameof(MouseExited));
	}

	public bool CanBePlayedAt(Vector2I? boardPosition) {
		if (boardPosition == null) {
			return false;
		}
		return GetParentOrNull<PlayerHand>()?.CanBePlayedAt(this, boardPosition.Value) ?? false;
	}
	public void PlayAt(Vector2I? boardPosition) {
		if (boardPosition == null) {
			return;
		}
		GetParentOrNull<PlayerHand>()?.PlayAt(this, boardPosition.Value);
	}

	public void Highlight()
	{
		if(!_grabbed)
			_sprite?.SetModulate(new Color(1, 0.5f, 0.1f, 1)); // Gelb hervorheben
	}

	public void Unhighlight()
	{
		_sprite?.SetModulate(new Color(1, 1, 1, 1)); // Normale Farbe
	}

	public void OnDropped() {
		_grabbed = false;
		GD.Print("Dropped");
		GD.Print("DiscardArea is set: " + _discardArea.Name);
		var globalMousePosition = GetGlobalMousePosition();
		var localMousePosition = _discardArea.ToLocal(globalMousePosition);
		if (_discardArea != null && _discardArea.GetShape().GetRect().HasPoint(localMousePosition)) {
			GD.Print("Discard area");
			GetParentOrNull<PlayerHand>()?.DiscardCard(this); // Karte verwerfen
		} else {
			GD.Print("No discard area");
		}
		
		if (!_snapBackArea.GetShape().GetRect().HasPoint(localMousePosition)){
			var camera = GetViewport().GetCamera3D();
			var mousePosition = GetViewport().GetMousePosition();
			var rayHit = Plane.PlaneXZ.IntersectsRay(
				camera.ProjectRayOrigin(mousePosition), 
				camera.ProjectRayNormal(mousePosition));
			var _hoveredCell = rayHit != null ? Board.ToBoardPosition(rayHit.Value) : (Vector2I?) null;
			//Raycast auf die plain und schauen wo es auf dem bord liegt
			GD.Print("ray position: " + rayHit.ToString());
			GD.Print("Board position: " + _hoveredCell.ToString());
			if (CanBePlayedAt(_hoveredCell)) {
				PlayAt(_hoveredCell);
				GD.Print("Card played at position: " + _hoveredCell);
			}
			else
			{
				GD.Print("Card cannot be played at this position.");
			}
		}

}

	private void OnGrabbed() {
		Unhighlight();
		_grabbed = true;
	}
	
	private void OnArea2DMouseEntered()
	{
		GetParentOrNull<PlayerHand>()?.HandleCardHovered(this);
	}

	private void OnArea2DMouseExited() {
		GetParentOrNull<PlayerHand>()?.HandleCardUnhovered(this);
	}

}
