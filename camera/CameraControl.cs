using Godot;
using System;
using CrimeChaosAndCatnip;


public partial class CameraControl : Node3D
{
	private enum CameraState {
		Isometric,
		TopDown,
	}

	[Export] public Node3D MinBoundsOverride;
	[Export] public Node3D MaxBoundsOverride;
	
	[Export] public Camera3D Camera;
	[Export] public Node3D Elevation;
	
	[Export] public float CameraSpeed = 10.0f;
	[Export] public float RotationSpeed = 5.0f;
	[Export] public float MouseRotationSpeed = 5.0f;
	[Export] public float EdgeSensitivity = 100.0f;
	private float _shiftFactor = 3.0f;
	
	[Export] public float ZoomSpeed = 5.0f;
	[Export] public float ZoomMin = 4.0f;
	[Export] public float ZoomMax = 30.0f;
	
	[Export] public float ElevationMin = 10.0f;
	[Export] public float ElevationMax = 85.0f;

	[Export] public bool EnableMouseCameraMovement = true;
	
	private CameraState _currentCameraState = CameraState.Isometric;
	
	private Board _board;

	private bool _isInWindow = true;
	private Vector2 _lastMousePosition;
	private Vector3? _dragPosition;
	
	private float _lastManualElevation;
	private bool _animatingElevation = false;

	private float _zoomVelocity;

	
	public override void _Ready() {
		_board = Board.GetBoard(this);
	}

	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationWMMouseEnter)
			_isInWindow = true;
		if (what == NotificationWMMouseExit)
			_isInWindow = false;
	}

	public override void _Process(double delta) {
		var dt = (float) delta;

		if (!_animatingElevation && Input.IsActionJustReleased("toggle_isometric")) {
			switch (_currentCameraState) {
				case CameraState.TopDown:
					_animatingElevation = true;
					_currentCameraState = CameraState.Isometric;
					break;
				case CameraState.Isometric:
					_lastManualElevation = Elevation.Rotation.X;
					_animatingElevation = true;
					_currentCameraState = CameraState.TopDown;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		if (_animatingElevation) {
			switch (_currentCameraState) {
				case CameraState.TopDown:
					Elevation.Rotation = Elevation.Rotation with {
						X = Mathf.MoveToward(Elevation.Rotation.X, Mathf.DegToRad(-85f), Mathf.DegToRad(90f) * dt)
					};
					if(Mathf.Abs(Elevation.RotationDegrees.X - -85f) < 0.1f)
						_animatingElevation = false;
					break;
				case CameraState.Isometric:
					Elevation.Rotation = Elevation.Rotation with {
						X = Mathf.MoveToward(Elevation.Rotation.X, _lastManualElevation, Mathf.DegToRad(90f) * dt)
					};
					if(Mathf.Abs(Elevation.Rotation.X - _lastManualElevation) < 0.01f)
						_animatingElevation = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return; // ignore input
		}
		
		var movement = Vector3.Zero;
		
		var mousePos = GetViewport().GetMousePosition();
		var mouseDisplacement = _lastMousePosition - mousePos;
		_lastMousePosition = mousePos;
		
		// mouse rotate / elevation
		if (Input.IsActionPressed("camera_rotate")) {
			if(mouseDisplacement.Length() > 0.1f)
				_currentCameraState = CameraState.Isometric;
			
			RotationDegrees = RotationDegrees with {Y = RotationDegrees.Y + mouseDisplacement.X * MouseRotationSpeed * dt};
			Elevation.RotationDegrees = Elevation.RotationDegrees with {X = Mathf.Clamp(Elevation.RotationDegrees.X + mouseDisplacement.Y * MouseRotationSpeed * dt, -ElevationMax, -ElevationMin)};
		}
		
		if (Input.IsActionPressed("rotate_camera_right"))
			RotationDegrees = RotationDegrees with {Y = RotationDegrees.Y + RotationSpeed * dt};
		if (Input.IsActionPressed("rotate_camera_left"))
			RotationDegrees = RotationDegrees with {Y = RotationDegrees.Y - RotationSpeed * dt};

		
		if (Input.IsActionPressed("camera_pan") && !HeldCard.AnyDragged) {
			var ground = CalculateGroundPosition();
			_dragPosition ??= ground;
			if (_dragPosition != null && ground != null)
				GlobalPosition += _dragPosition.Value - ground.Value;
		} else
			_dragPosition = null;
	
		if(EnableMouseCameraMovement && _isInWindow) {
			var screenSize = GetViewport().GetVisibleRect();
			if (mousePos.X <= EdgeSensitivity) {
				movement.X -= 1;
			} else if (mousePos.X >= screenSize.Size.X - EdgeSensitivity) {
				movement.X += 1;
			}
			if (mousePos.Y <= EdgeSensitivity) {
				movement.Z -= 1;
			} else if (mousePos.Y >= screenSize.Size.Y - EdgeSensitivity) {
				movement.Z += 1;
			}
		}
	
		if (Input.IsActionPressed("speedup_movement"))
			_shiftFactor = 1.0f;
		else if (Input.IsActionJustReleased("speedup_movement"))
			_shiftFactor = 3.0f;
	
		//Moves the camera on the axis with WASD
		if (Input.IsActionPressed("move_camera_down"))
			movement.Z += 1;
		if (Input.IsActionPressed("move_camera_up"))
			movement.Z -= 1;
		if (Input.IsActionPressed("move_camera_left"))
			movement.X -= 1;
		if (Input.IsActionPressed("move_camera_right"))
			movement.X += 1;
	
		if (movement.LengthSquared() > 1f)
			movement = movement.Normalized();

		var velocity = movement * CameraSpeed * _shiftFactor;
		GlobalPosition += velocity.Rotated(Vector3.Up, Camera.GlobalRotation.Y) * dt;

			
		//Implements Zoom
		if (Input.IsActionJustReleased("zoom_in"))
			_zoomVelocity = -1;
		if (Input.IsActionJustReleased("zoom_out"))
			_zoomVelocity = 1;

		if (Mathf.Abs(_zoomVelocity) > 0.0001f) {
			var newZoom = Mathf.Clamp(Camera.Position.Z + ZoomSpeed * _zoomVelocity * dt, ZoomMin, ZoomMax);
			var groundPosition = CalculateGroundPosition();

			Camera.Position = Camera.Position with {Z = newZoom};
			if (groundPosition != null) { // offset camera to zoom towards cursor
				GlobalPosition += groundPosition.Value - (CalculateGroundPosition() ?? groundPosition.Value);
			}

			_zoomVelocity *= 0.8f;
		}

		ConfineCamera();
	}

	private Vector3? CalculateGroundPosition() {
		var mousePos = GetViewport().GetMousePosition();
		return Plane.PlaneXZ.IntersectsRay(Camera.ProjectRayOrigin(mousePos), Camera.ProjectRayNormal(mousePos));
	}
	
	private void ConfineCamera() {
		if (_board == null) 
			return;
		
		var planeConfines = _board.BoardWorldDimensions();

		var min = new Vector3(planeConfines.Position.X, 1f, planeConfines.Position.Y);
		if (MinBoundsOverride != null)
			min = MinBoundsOverride.GlobalPosition;

		var max = new Vector3(planeConfines.End.X, 1f, planeConfines.End.Y);
		if (MaxBoundsOverride != null)
			max = MaxBoundsOverride.GlobalPosition;
		
		GlobalPosition = GlobalPosition.Clamp(min, max);
	}
}
