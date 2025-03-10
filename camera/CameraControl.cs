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
	private float CurrentZoomHeight;

	// Toggle Isometric to Topdown
	public bool isometric = false;

	private Vector3 _targetPosition;
	private Vector3 _targetRotation;
	[Export] public float LerpSpeed = 5.0f;
	private Vector3 _originalPosition;
	private Vector3 _originalRotation;

	public override void _Ready()
	{
		_targetPosition = Camera.Position;
		_targetRotation = Camera.Rotation;
		_originalPosition = Camera.Position;
		_originalRotation = Camera.Rotation;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_isometric"))
		{
			if (isometric)
			{
				isometric = !isometric;
				_targetRotation.X += Mathf.DegToRad(RotationDegree);  
				_targetPosition += BaseCameraMove;
			}
			else
			{
				isometric = !isometric;
				_targetRotation.X -= Mathf.DegToRad(RotationDegree); 
				_targetPosition -= BaseCameraMove;
			}

			GD.Print("Isometric toggled!");
		}

		

		if (@event.IsActionPressed("zoom_in"))
		{
			GD.Print("Zoom in");
		}

		if (@event.IsActionPressed("zoom_out"))
		{
			GD.Print("Zoom out");
		}
	}

	public override void _Process(double delta)
	{
		Camera.Position = Camera.Position.Lerp(_targetPosition, (float)(LerpSpeed * delta));
		Camera.Rotation = new Vector3(
			Mathf.LerpAngle(Camera.Rotation.X, _targetRotation.X, (float)(LerpSpeed * delta)),
			Mathf.LerpAngle(Camera.Rotation.Y, _targetRotation.Y, (float)(LerpSpeed * delta)),
			Mathf.LerpAngle(Camera.Rotation.Z, _targetRotation.Z, (float)(LerpSpeed * delta))
		);
		
		Vector3 movement = Vector3.Zero;
		
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
		

		Camera.Position += movement.Normalized() * CameraSpeed * (float)delta;
		
		_targetPosition = Camera.Position;
		
	}
}
