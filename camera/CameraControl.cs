using Godot;
using System;

public partial class CameraControl : Node
{
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
	private float CurrentZoomHeight;


	private enum CameraState {
		Isometric,
		TopDown,
	}

	private class CameraView {

		public Vector3 Position { get; set; } = Vector3.Zero;
		public Vector3 Rotation { get; set; } = Vector3.Zero;

		public void SetView(Vector3 position, Vector3 rotation) {
			Position = position;
			Rotation = rotation;
		}

	}
	private CameraState _currentCameraState = CameraState.Isometric;

	private Vector3 _targetPosition;
	private Vector3 _targetRotation;
	[Export] public float LerpSpeed = 5.0f;

	private CameraView _targetView;
	private CameraView _originalIsometricView;
	private CameraView _originalTopDownView;



	public override void _Ready() {
		
		_originalIsometricView = new CameraView();
		_originalTopDownView = new CameraView();
		_targetView = new CameraView();
		
		_targetPosition = IsometricCamera.Position;
		_targetRotation = IsometricCamera.Rotation;

		_originalIsometricView.SetView(IsometricCamera.Position, IsometricCamera.Rotation);
		_originalTopDownView.SetView(TopDownCamera.Position, TopDownCamera.Rotation);
		
	}



	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_isometric"))
		{
			if (_currentCameraState == CameraState.TopDown)
			{
				IsometricCamera.MakeCurrent();
				
				IsometricCamera.Position = _originalIsometricView.Position;
				IsometricCamera.Rotation = _originalIsometricView.Rotation;
				
				_currentCameraState = CameraState.Isometric;
			}
			else if(_currentCameraState == CameraState.Isometric)
			{
				TopDownCamera.MakeCurrent();
				_currentCameraState = CameraState.TopDown;
				TopDownCamera.Position = _originalTopDownView.Position;
				TopDownCamera.Rotation = _originalTopDownView.Rotation;
			}

			GD.Print("Isometric toggled!");
		}

		
		if (@event is InputEventMouseButton mouseEvent && _currentCameraState == CameraState.Isometric)
		{
			if ((int)mouseEvent.ButtonIndex == 4)
			{
				IsometricCamera.Position = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y - 0.1f, IsometricCamera.Position.Z);
			}
			else if ((int)mouseEvent.ButtonIndex == 5)
			{
				IsometricCamera.Position = new Vector3(IsometricCamera.Position.X, IsometricCamera.Position.Y + 0.1f, IsometricCamera.Position.Z);
			}
		}
	}

	public override void _Process(double delta)
	{
		if(_currentCameraState == CameraState.Isometric)
		{
			Vector3 movement = Vector3.Zero;
			
			
			
			//Configures that the Camera moves on the borders of the screen.

			Vector2 mousePos = GetViewport().GetMousePosition();
			Rect2 screenSize = GetViewport().GetVisibleRect();
			
			
			if (mousePos.X <= EdgeSensitivity)
			{
				movement.X -= CameraSpeed * (float)delta;
			}

			else if (mousePos.X >= screenSize.Size.X - EdgeSensitivity)
			{
				movement.X += CameraSpeed * (float)delta;
			}


			if (mousePos.Y <= EdgeSensitivity)
			{
				movement.Z -= CameraSpeed * (float)delta;
			}
			else if (mousePos.Y >= screenSize.Size.Y - EdgeSensitivity)
			{
				movement.Z += CameraSpeed * (float)delta;
			}
			
			
			//Moves the camera on the correct axis with WASD

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

			
			//Rotate the Camera 
			
			if (Input.IsActionPressed("rotate_camera_right"))
			{
				_targetRotation.Y -= Mathf.DegToRad(0.1f);
			}

			if (Input.IsActionPressed("rotate_camera_left"))
			{
				_targetRotation.Y += Mathf.DegToRad(0.1f);
			}


			Transform3D cameraTransform = IsometricCamera.GlobalTransform;

			Vector3 localMovement = cameraTransform.Basis * (movement.Normalized() * CameraSpeed * (float)delta);


			IsometricCamera.Position += localMovement;

			_targetPosition = IsometricCamera.Position;

			IsometricCamera.Rotation = _targetRotation;
		}

	}
}
