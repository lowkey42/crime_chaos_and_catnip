namespace CrimeChaosAndCatnip.Level.Entities;
using Godot;
using System.Threading.Tasks;

[GlobalClass]
public partial class Player : Node3D
{
    [Export] public GridMap GridMap;
    [Export] public Vector3I StartGridPosition = new Vector3I(0, 2, 0); 
    [Export] public float StepDuration = 0.8f; 

    private bool isMoving = false;

    public override void _Ready()
    {
        // Spieler auf die Startposition setzen
        SetToGridPosition(StartGridPosition);
    }

    private void SetToGridPosition(Vector3I gridPosition)
    {
        Vector3 worldPosition = GridMap.MapToLocal(gridPosition);
        GlobalTransform = new Transform3D(GlobalTransform.Basis, worldPosition);
    }

    public async void MovePlayer(Vector3 direction, int steps)
    {
        if (isMoving || GridMap == null)
        {
            GD.PrintErr("Bewegung blockiert oder GridMap fehlt");
            return;
        }

        isMoving = true;
        Vector3 cellSize = GridMap.CellSize;
        Vector3I playerGridPos = GridMap.LocalToMap(GlobalTransform.Origin);

        for (int i = 0; i < steps; i++)
        {
            Vector3I moveVector = new Vector3I(
                (int)Mathf.Sign(direction.X),
                (int)Mathf.Sign(direction.Y),
                (int)Mathf.Sign(direction.Z)
            );
	        GD.Print(moveVector);

            playerGridPos += moveVector;
            Vector3 targetPosition = GridMap.MapToLocal(playerGridPos);
            targetPosition = new Vector3(
                (int)(((targetPosition.X / cellSize.X) * cellSize.X) + 0.5),
                (int)(((targetPosition.Y / cellSize.Y) * cellSize.Y) + 0.5),
                (int)(((targetPosition.Z / cellSize.Z) * cellSize.Z) + 0.5)
            );
            await InterpolateMovement(targetPosition);
        }

        isMoving = false;
    }

    private async Task InterpolateMovement(Vector3 targetPosition)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "global_transform:origin", targetPosition, StepDuration)
             .SetEase(Tween.EaseType.InOut);
        
        await ToSignal(tween, "finished");
    }
}