using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class HeldCard : Node2D {

	public static bool AnyDragged { get; private set; }
	
	[Signal]
	public delegate void MouseEnteredEventHandler();

	[Signal]
	public delegate void MouseExitedEventHandler();

	[Export] public CardBase Card;

	[Export] private TextureRect _sprite;
	
	[Export]private AnimationPlayer _animationPlayer;

	
	public bool IsGrabbed => _grabbed;
	private bool _grabbed = false;
	private bool _canDrop = false;
	private Timer _dropTimer;


	public override void _Ready() {
		base._Ready();

		_sprite?.SetTexture(Card?.CardSprite);

		_dropTimer = new Timer();
		AddChild(_dropTimer);
		_dropTimer.WaitTime = 0.2f;
		_dropTimer.OneShot = true;
		_dropTimer.Connect("timeout", new Callable(this, nameof(OnDropTimerTimeout)));
	}

	private void OnDropTimerTimeout() {
		_canDrop = true;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		var targetScale = 1f;

		if(_grabbed && !Gameplay.CanPlayCards)
			OnDropped();
		
		if (_grabbed) {
			targetScale = GetParentOrNull<PlayerHand>()?.IsInPlayableArea(GetGlobalMousePosition()) ?? false ? 0.2f : 1f;
			
			var boardPosition = GetMouseBoardPosition();
			GetParentOrNull<PlayerHand>()
				?.MarkHoveredForbidden(boardPosition.HasValue && !CanBePlayedAt(boardPosition.Value));
		}

		Scale = Scale.Lerp(Vector2.One * targetScale, (float) delta / 0.2f);
	}

	private void OnMouseEntered() {
		EmitSignal(nameof(MouseEntered));
	}

	private void OnMouseExited() {
		EmitSignal(nameof(MouseExited));
	}

	public bool CanBePlayedAt(Vector2I? boardPosition) {
		if (boardPosition == null) return false;
		return GetParentOrNull<PlayerHand>()?.CanBePlayedAt(this, boardPosition.Value) ?? false;
	}

	public void PlayAt(Vector2I? boardPosition) {
		if (boardPosition == null) return;
		GetParentOrNull<PlayerHand>()?.PlayAt(this, boardPosition.Value);
	}

	public void Highlight() {
		if (!_grabbed) {
			if (_sprite != null) {
				var spriteMaterial = (ShaderMaterial) _sprite.Material;
				spriteMaterial.SetShaderParameter("highlight_active", true);
			}
		}

	}

	public void Unhighlight() {
		if (_sprite != null)
		{
			var spriteMaterial = (ShaderMaterial)_sprite.Material;
			spriteMaterial.SetShaderParameter("highlight_active", false); 
		}
	}

	public void OnDropped() {
		Scale = Vector2.One;
		_grabbed = false;
		AnyDragged = false;
		GetParentOrNull<PlayerHand>().MarkHoveredForbidden(false);
		
		if (!_canDrop)
			return;

		
		var mousePosition = GetViewport().GetMousePosition();
		
		if (GetParentOrNull<PlayerHand>()?.IsInDiscardArea(mousePosition) ?? false)
			GetParentOrNull<PlayerHand>()?.DiscardCard(this); // Karte verwerfen
		else if (GetParentOrNull<PlayerHand>()?.IsInPlayableArea(mousePosition) ?? false) {
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
		AnyDragged = true;
		_canDrop = false;
		_dropTimer.Start();
	}

	private void OnArea2DMouseEntered() {
		GetParentOrNull<PlayerHand>()?.HandleCardHovered(this);
	}

	private void OnArea2DMouseExited() {
		GetParentOrNull<PlayerHand>()?.HandleCardUnhovered(this);
	}

	public void FlipCard() {
		if (_animationPlayer != null)
		{
			_animationPlayer.Play("card_flip"); // "draw_card" ist der Name der Animation
		}
	}

}
