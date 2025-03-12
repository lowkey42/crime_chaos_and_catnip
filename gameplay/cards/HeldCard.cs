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
	
	private bool _canDrop = false;
	private Timer _dropTimer;
	

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
		
		
		_dropTimer = new Timer();
		AddChild(_dropTimer);
		_dropTimer.WaitTime = 0.5f;
		_dropTimer.OneShot = true;
		_dropTimer.Connect("timeout", new Callable(this, nameof(OnDropTimerTimeout)));
		
	}
	
	private void OnDropTimerTimeout()
	{
		_canDrop = true;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		var targetScale = 1f;
		
		if (_grabbed) {
			Vector2 globalMousePosition = GetGlobalMousePosition();
			Vector2 localMousePosition = _snapBackArea.ToLocal(globalMousePosition);

			if (!_snapBackArea.GetShape().GetRect().HasPoint(localMousePosition)) {
				targetScale = 0.2f;
			} else {
				targetScale = 1f;
			}
			var boardPosition = GetMouseBoardPosition();
			GetParentOrNull<PlayerHand>()
				?.MarkHoveredForbidden(boardPosition.HasValue && !CanBePlayedAt(boardPosition.Value));
		}
		
		Scale = Scale.Lerp(Vector2.One * targetScale, (float) delta / 0.2f);
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
		if (!_canDrop)
		{
			GD.Print("Drop blocked: Timer not finished.");
			return;
		}
		_shrinkTween?.Kill();
		Scale = Vector2.One;
		_grabbed = false;
		GetParentOrNull<PlayerHand>().MarkHoveredForbidden(false);
		var globalMousePosition = GetGlobalMousePosition();
		var localMousePosition = _discardArea.ToLocal(globalMousePosition);
		if (_discardArea != null && _discardArea.GetShape().GetRect().HasPoint(localMousePosition)) {
			GetParentOrNull<PlayerHand>()?.DiscardCard(this); // Karte verwerfen
			
		} else if (!_snapBackArea.GetShape().GetRect().HasPoint(localMousePosition)){
			GD.Print("AREA: " + _snapBackArea);
			var boardPosition = GetMouseBoardPosition();
			if (CanBePlayedAt(boardPosition))
				PlayAt(boardPosition);
		} else {
			GD.Print("Is In SnapBackArea" + nameof(_snapBackArea));
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
		_canDrop = false;
		_dropTimer.Start();
	}
	
	private void OnArea2DMouseEntered()
	{
		GetParentOrNull<PlayerHand>()?.HandleCardHovered(this);
	}

	private void OnArea2DMouseExited() {
		GetParentOrNull<PlayerHand>()?.HandleCardUnhovered(this);
	}

}
