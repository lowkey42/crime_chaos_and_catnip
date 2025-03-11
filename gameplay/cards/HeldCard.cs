using Godot;

namespace CrimeChaosAndCatnip;

[GlobalClass]
public partial class HeldCard : Node2D {
	
	[Signal] public delegate void MouseEnteredEventHandler();
	[Signal] public delegate void MouseExitedEventHandler();

	[Export] public CardBase Card;

	[Export]private CollisionShape2D _discardArea;

	[Export] private Sprite2D _sprite;
	
	private bool _grabbed = false;
	

	public override void _Ready() {
		base._Ready();
		_discardArea = GetParentOrNull<PlayerHand>()?.GetCollisionShape2D();
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

	public bool CanBePlayedAt(Vector2I boardPosition) {
		return GetParentOrNull<PlayerHand>()?.CanBePlayedAt(this, boardPosition) ?? false;
	}
	public void PlayAt(Vector2I boardPosition) {
		GetParentOrNull<PlayerHand>()?.PlayAt(this, boardPosition);
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

	public void mouseEntered() {
		GD.Print("Mouse entered");
		Highlight();
		
	}

	public void mouseExited() {
		GD.Print("Mouse exited");
		Unhighlight();
	}

	public void OnDropped()
	{
		_grabbed = false;
		GD.Print("Dropped");

		// Überprüfen, ob die Maus über dem Discard-Bereich liegt
		if (_discardArea != null && _discardArea.GetShape().GetRect().HasPoint(GetGlobalMousePosition()))
		{
			GD.Print("Discard area");
			GetParentOrNull<PlayerHand>()?.DiscardCard(this); // Karte verwerfen
		}
		else
		{
			// Optional: Karte an ihre ursprüngliche Position zurücksetzen
			GD.Print("No discard area");
		}
	}

	public void onGrabbed() {
		Unhighlight();
		_grabbed = true;
	}
	
	private void OnArea2DMouseEntered()
	{
		GD.Print("Mouse entered");
		GetParentOrNull<PlayerHand>()?.HandleCardHovered(this);
	}

	private void OnArea2DMouseExited() {
		GD.Print("Mouse exited");
		GetParentOrNull<PlayerHand>()?.HandleCardUnhovered(this);
	}

}
