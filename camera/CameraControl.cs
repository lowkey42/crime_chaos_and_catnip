using Godot;
using System;


public partial class CameraControl : Node
{
	private enum CameraState {
		Isometric,
		TopDown,
	}

	[Export] public Vector3 BaseCameraMove;
	[Export] public float RotationDegree;

	[Export] public Camera3D IsometricCamera;
	[Export] public Camera3D TopDownCamera;
	[Export] public float CameraSpeed = 10.0f;

	[Export] public float RotationSpeed = 5.0f;
	[Export] public float EdgeSensitivity = 100.0f;
	private float _shiftFactor = 3.0f;

	[Export] public bool EnableMouseCameraMovement = true;

	[Export] public RayCast3D CameraRaycast;
	
	private CameraState _currentCameraState = CameraState.Isometric;

	private Vector3 _targetPosition;
	private Vector3 _targetRotation;
	[Export] public float LerpSpeed = 5.0f;

	
	private bool _isRightMouseButtonPressed;

	
	public override void _Ready() {
		
		
		
		_targetRotation = IsometricCamera.Rotation;

		
		
	}



	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_isometric")) {
			switch (_currentCameraState) {
				case CameraState.TopDown:
					IsometricCamera.MakeCurrent();
				
					_currentCameraState = CameraState.Isometric;
					break;
				case CameraState.Isometric:
					TopDownCamera.MakeCurrent();
					_currentCameraState = CameraState.TopDown;
					
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		
		if (@event is InputEventMouseButton mouseEvent) {
			if(_currentCameraState == CameraState.Isometric)
			{
				var zoomAmount = 1f;
				var targetPosition = IsometricCamera.Position;
				
				switch ((int)mouseEvent.ButtonIndex) {
					case 4:
						targetPosition = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y - zoomAmount, IsometricCamera.Position.Z);
						break;
					case 5:
						targetPosition = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y + zoomAmount, IsometricCamera.Position.Z);
						break;
					case 2:
						break;
				}
				var tween = GetTree().CreateTween();
				tween.TweenProperty(IsometricCamera, "position", targetPosition, 0.1f);
			} else {
				var zoomAmount = 1f;
				var minZoom = 5f;
				var maxZoom = 20f; 
				var targetPosition = TopDownCamera.Position;
				
				switch ((int)mouseEvent.ButtonIndex) {
					case 4:
						targetPosition.Y = Mathf.Max(TopDownCamera.Position.Y - zoomAmount, minZoom);
						break;
					case 5:
						targetPosition.Y = Mathf.Min(TopDownCamera.Position.Y + zoomAmount, maxZoom);
						break;
					case 2:
						break;
				}
				var tween = GetTree().CreateTween();
				tween.TweenProperty(TopDownCamera, "position", targetPosition, 0.1f);
				
			}
		}
		
		switch (@event) {
			case InputEventMouseButton { ButtonIndex: MouseButton.Right } mouseEvent2:
				_isRightMouseButtonPressed = mouseEvent2.Pressed;
				break;
			case InputEventMouseMotion mouseMotionEvent when _isRightMouseButtonPressed: {
				var rotationAmount = -mouseMotionEvent.Relative.X * 0.005f;
				_targetRotation.Y += rotationAmount;
				break;
			}
		}
	}

	public override void _Process(double delta)
	{
		var movement = Vector3.Zero;
		
		if (_currentCameraState == CameraState.Isometric) {
			
			//Configures that the Camera moves on the borders of the screen.

			var mousePos = GetViewport().GetMousePosition();
			var screenSize = GetViewport().GetVisibleRect();

			if(EnableMouseCameraMovement)
			{
				if (mousePos.X <= EdgeSensitivity) {
					movement.X -= CameraSpeed * (float) delta;
				} else if (mousePos.X >= screenSize.Size.X - EdgeSensitivity) {
					movement.X += CameraSpeed * (float) delta;
				}


				if (mousePos.Y <= EdgeSensitivity) {
					movement.Z -= CameraSpeed * (float) delta;
				} else if (mousePos.Y >= screenSize.Size.Y - EdgeSensitivity) {
					movement.Z += CameraSpeed * (float) delta;
				}
			}
			if (Input.IsActionPressed("rotate_camera_right"))
			{
				_targetRotation.Y -= (float) (Mathf.DegToRad(RotationSpeed) * delta);
			}

			if (Input.IsActionPressed("rotate_camera_left"))
			{
				_targetRotation.Y += (float) (Mathf.DegToRad(RotationSpeed) * delta);
			}

		}
		
		if (Input.IsActionPressed("speedup_movement")) {
			_shiftFactor = 1.0f;
		}
		else if (Input.IsActionJustReleased("speedup_movement")){
			_shiftFactor = 3.0f;
		}

		//Moves the camera on the axis with WASD

			if (Input.IsActionPressed("move_camera_down"))
			{
				movement.Z += 1;
			}
			if (Input.IsActionPressed("move_camera_up"))
			{
				movement.Z -= 1;
			}
			if (Input.IsActionPressed("move_camera_left"))
			{
				movement.X -= 1;
			}
			if (Input.IsActionPressed("move_camera_right"))
			{
				movement.X += 1;
			}
			
			//Implements Zoom
			if (Input.IsActionPressed("zoom_in"))
			{
				movement.Y += 1000;
			}

			if (Input.IsActionPressed("zoom_out"))
			{
				movement.Y -= 1000;
			}

			var cameraTransform = IsometricCamera.GlobalTransform;

			var movementDirection = movement.Normalized();
			movementDirection.Y = 0;

			var forward = cameraTransform.Basis.Z;
			var right = cameraTransform.Basis.X;


			forward.Y = 0;
			right.Y = 0;

			forward = forward.Normalized();
			right = right.Normalized();

			var localMovement = (right * movementDirection.X + forward * movementDirection.Z) * CameraSpeed *  _shiftFactor *(float)delta;

			CameraRaycast.GlobalPosition = IsometricCamera.Position;
			CameraRaycast.TargetPosition = movement.Normalized() * 1.5f;
			CameraRaycast.Rotation = IsometricCamera.Rotation;
			CameraRaycast.ForceRaycastUpdate();

			if (!CameraRaycast.IsColliding()) {
				if (_currentCameraState == CameraState.Isometric) {
					IsometricCamera.Position += localMovement;
					_targetPosition = IsometricCamera.Position;
					IsometricCamera.Rotation = _targetRotation;
				} else {
					localMovement = new Vector3(movement.X, 0, movement.Z) * CameraSpeed * _shiftFactor * (float)delta;

					TopDownCamera.Position += localMovement;
					_targetPosition = TopDownCamera.Position;
				}
				
			}

	}
}
