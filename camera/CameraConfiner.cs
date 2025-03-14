using System;
using Godot;

namespace CrimeChaosAndCatnip.camera;

public partial class CameraConfiner : Node3D
{
		[Export]
		public Vector3 MinLimit = new Vector3(-40, 0, -100);  
		[Export]
		public Vector3 MaxLimit = new Vector3(70, 50, 10);
		[Export]
		public Camera3D Camera;
		[Export]
		public Camera3D Camera2;

		public override void _Process(double delta)
		{
			if (Camera != null)
			{
		
				Vector3 cameraPosition = Camera.Position;
				Vector3 camera2Position = Camera2.Position;

				
				cameraPosition.X = Mathf.Clamp(cameraPosition.X, MinLimit.X, MaxLimit.X);
				camera2Position.X = Mathf.Clamp(camera2Position.X, MinLimit.X, MaxLimit.X);

				
				cameraPosition.Y = Mathf.Clamp(cameraPosition.Y, MinLimit.Y, MaxLimit.Y);
				camera2Position.Y = Mathf.Clamp(camera2Position.Y, MinLimit.Y, MaxLimit.Y);

				
				cameraPosition.Z = Mathf.Clamp(cameraPosition.Z, MinLimit.Z, MaxLimit.Z);
				camera2Position.Z = Mathf.Clamp(camera2Position.Z, MinLimit.Z, MaxLimit.Z);
				
				Camera.Position = cameraPosition;
				Camera2.Position = camera2Position;
			}
		}
}
