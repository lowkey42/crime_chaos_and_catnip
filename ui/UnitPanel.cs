using System;
using System.Collections.Generic;
using Godot;

namespace CrimeChaosAndCatnip;

public partial class UnitPanel : ScrollContainer {

	[Export] private Control _container;

	[Export] private CameraControl _camera;

	private readonly Dictionary<Unit, Control> _buttons = new();
	
	public override void _Ready() {
		var board = Board.GetBoard(this);
		if (board == null || _camera == null)
			return;

		foreach(var unit in board.GetUnits())
			OnUnitAdded(unit);
		
		board.UnitAdded += OnUnitAdded;
		board.UnitRemoved += OnUnitRemoved;
	}

	private void OnUnitRemoved(Unit unit) {
		if (unit is PlayerUnit && _buttons.TryGetValue(unit, out var button)) {
			button.QueueFree();
			_buttons.Remove(unit);
		}
	}

	private void OnUnitAdded(Unit unit) {
		if (unit is not PlayerUnit {ButtonScene: not null} playerUnit)
			return;
		
		var button = playerUnit.ButtonScene.Instantiate<UnitButton>();
		button.Activated += () => _camera.FocusPosition(unit.GlobalPosition);
		_container.AddChild(button);
		_buttons.Add(unit, button);
	}

}
