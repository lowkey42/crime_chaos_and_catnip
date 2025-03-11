using Godot;
using System;


public partial class CameraControl : Node
{
	private enum CameraState {
		Isometric,
		TopDown,
	}

	private class CameraView {

		public Vector3 Position { get; private set; } = Vector3.Zero;
		public Vector3 Rotation { get; private set; } = Vector3.Zero;

		public void SetView(Vector3 position, Vector3 rotation) {
			Position = position;
			Rotation = rotation;
		}

	}
	[Export] public Vector3 BaseCameraMove;
	[Export] public float RotationDegree;

	[Export] public Camera3D IsometricCamera;
	[Export] public Camera3D TopDownCamera;
	[Export] public float CameraSpeed = 10.0f;

	[Export] public float RotationSpeed = 5.0f;
	[Export] public float ZoomSpeed = 5.0f;
	[Export] public float ZoomMin = 5.0f;
	[Export] public float ZoomMax = 5.0f;
	[Export] public float EdgeSensitivity = 100.0f;
	private float _shiftFactor = 1.0f;
	
	private CameraState _currentCameraState = CameraState.Isometric;

	private Vector3 _targetPosition;
	private Vector3 _targetRotation;
	[Export] public float LerpSpeed = 5.0f;

	private CameraView _originalIsometricView;
	private CameraView _originalTopDownView;
	
	private bool _isRightMouseButtonPressed;

	
	public override void _Ready() {
		
		_originalIsometricView = new CameraView();
		_originalTopDownView = new CameraView();
		
		_targetRotation = IsometricCamera.Rotation;

		_originalIsometricView.SetView(IsometricCamera.Position, IsometricCamera.Rotation);
		_originalTopDownView.SetView(TopDownCamera.Position, TopDownCamera.Rotation);
		
	}



	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_isometric")) {
			switch (_currentCameraState) {
				case CameraState.TopDown:
					IsometricCamera.MakeCurrent();
				
					IsometricCamera.Position = _originalIsometricView.Position;
					IsometricCamera.Rotation = _originalIsometricView.Rotation;
				
					_currentCameraState = CameraState.Isometric;
					break;
				case CameraState.Isometric:
					TopDownCamera.MakeCurrent();
					_currentCameraState = CameraState.TopDown;
					TopDownCamera.Position = _originalTopDownView.Position;
					TopDownCamera.Rotation = _originalTopDownView.Rotation;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			GD.Print("Isometric toggled!");
		}


		
		if (@event is InputEventMouseButton mouseEvent && _currentCameraState == CameraState.Isometric) {
			switch ((int)mouseEvent.ButtonIndex) {
				case 4:
					IsometricCamera.Position = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y - 0.1f, IsometricCamera.Position.Z);
					break;
				case 5:
					IsometricCamera.Position = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y + 0.1f, IsometricCamera.Position.Z);
					break;
				case 2:
					break;
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

			if (Input.IsActionPressed("rotate_camera_right"))
			{
				_targetRotation.Y -= Mathf.DegToRad(0.1f);
			}

			if (Input.IsActionPressed("rotate_camera_left"))
			{
				_targetRotation.Y += Mathf.DegToRad(0.1f);
			}

		}
		
		if (Input.IsActionPressed("speedup_movement")) {
			_shiftFactor = 3.0f;
		}
		else if (Input.IsActionJustReleased("speedup_movement")){
			_shiftFactor = 1.0f;
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
				movement.Y += 1;
			}

			if (Input.IsActionPressed("zoom_out"))
			{
				movement.Y -= 1;
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
