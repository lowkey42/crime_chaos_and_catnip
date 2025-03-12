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

	private Tween _shrinkTween;
	

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

	public override void _Process(double delta) {
		base._Process(delta);

		if (_grabbed) {
			var boardPosition = GetMouseBoardPosition();
			GetParentOrNull<PlayerHand>().MarkHoveredForbidden(boardPosition.HasValue && !CanBePlayedAt(boardPosition.Value));
		}
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
		_shrinkTween?.Kill();
		Scale = Vector2.One;
		_grabbed = false;
		GetParentOrNull<PlayerHand>().MarkHoveredForbidden(false);
		var globalMousePosition = GetGlobalMousePosition();
		var localMousePosition = _discardArea.ToLocal(globalMousePosition);
		if (_discardArea != null && _discardArea.GetShape().GetRect().HasPoint(localMousePosition)) {
			GetParentOrNull<PlayerHand>()?.DiscardCard(this); // Karte verwerfen
			
		} else if (!_snapBackArea.GetShape().GetRect().HasPoint(localMousePosition)){
			var boardPosition = GetMouseBoardPosition();
			if (CanBePlayedAt(boardPosition))
				PlayAt(boardPosition);
		}
	}

	private Vector2I? GetMouseBoardPosition() {
		var camera = GetViewport().GetCamera3D();
		var mousePosition = GetViewport().GetMousePosition();
		var rayHit = Plane.PlaneXZ.IntersectsRay(
			camera.ProjectRayOrigin(mousePosition), 
			camera.ProjectRayNormal(mousePosition));
		return rayHit != null ? Board.ToBoardPosition(rayHit.Value) : null;
	}

	private void OnGrabbed() {
		Unhighlight();
		_grabbed = true;
		_shrinkTween?.Kill();
		_shrinkTween = CreateTween();
		_shrinkTween.TweenProperty(this, "scale", Vector2.One * 0.1f, 0.5f).SetTrans(Tween.TransitionType.Bounce);
		Scale = Vector2.One * 0.1f;
	}
	
	private void OnArea2DMouseEntered()
	{
		GetParentOrNull<PlayerHand>()?.HandleCardHovered(this);
	}

	private void OnArea2DMouseExited() {
		GetParentOrNull<PlayerHand>()?.HandleCardUnhovered(this);
	}

}
