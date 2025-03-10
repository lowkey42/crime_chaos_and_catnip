using Godot;
using System;

public partial class CameraControl : Node
{
	[Export] public Vector3 BaseCameraMove;
	[Export] public float RotationDegree;

	[Export] public Camera3D Camera;
	[Export] public float CameraSpeed = 10.0f;
	
	[Export] public float RotationSpeed = 5.0f;
	[Export] public float ZoomSpeed = 5.0f;
	[Export] public float ZoomMin = 5.0f;
	[Export] public float ZoomMax = 5.0f;
	[Export] public float EdgeSensitivity = 100.0f;
	private float CurrentZoomHeight;

	// Toggle Isometric to Topdown
	private bool _isometric = false;

	private Vector3 _targetPosition;
	private Vector3 _targetRotation;
	[Export] public float LerpSpeed = 5.0f;

	public override void _Ready()
	{
		_targetPosition = Camera.Position;
		_targetRotation = Camera.Rotation;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_isometric"))
		{
			if (_isometric)
			{
				_isometric = !_isometric;
				_targetRotation.X += Mathf.DegToRad(RotationDegree);  
				_targetPosition += BaseCameraMove;
			}
			else
			{
				_isometric = !_isometric;
				_targetRotation.X -= Mathf.DegToRad(RotationDegree); 
				_targetPosition -= BaseCameraMove;
			}

			GD.Print("Isometric toggled!");
		}
		
		if (@event is InputEventMouseButton mouseEvent)
		{
	
			if ((int)mouseEvent.ButtonIndex == 4)
			{
				Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y - 0.1f, Camera.Position.Z);
			}

			else if ((int)mouseEvent.ButtonIndex == 5)
			{
				Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y + 0.1f, Camera.Position.Z);
			}
		}
	}

	public override void _Process(double delta)
	{
		Vector3 movement = Vector3.Zero;
		
		
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
		

		if (Input.IsActionPressed("move_camera_down")) {
			movement.Z += 1;
		}
		if (Input.IsActionPressed("move_camera_up")) {
			movement.Z -= 1;
		}
		if (Input.IsActionPressed("move_camera_left")) {
			movement.X -= 1;
		}
		if (Input.IsActionPressed("move_camera_right")) {
			movement.X += 1;
		}
		if (Input.IsActionPressed("zoom_in")) {
			movement.Y += 1;
		}

		if (Input.IsActionPressed("zoom_out")) {
			movement.Y -= 1;
		}

		Camera.Position += movement.Normalized() * CameraSpeed * (float)delta;


		_targetPosition = Camera.Position;


		Camera.Position = Camera.Position.Lerp(_targetPosition, (float)(LerpSpeed * delta));
		Camera.Rotation = new Vector3(
			Mathf.LerpAngle(Camera.Rotation.X, _targetRotation.X, (float)(LerpSpeed * delta)),
			Mathf.LerpAngle(Camera.Rotation.Y, _targetRotation.Y, (float)(LerpSpeed * delta)),
			Mathf.LerpAngle(Camera.Rotation.Z, _targetRotation.Z, (float)(LerpSpeed * delta))
		);
	}
}
