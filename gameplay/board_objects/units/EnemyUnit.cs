#nullable enable
using Godot;

namespace CrimeChaosAndCatnip;

public partial class EnemyUnit : Unit {

	[Export] private Path3D? _path;

	[Export] private Node3D? _attackMarker;

	private int _nextIndex = 1;

	private bool _reverse = false;

	private bool _attackHit = false;

	private Tween? _tween;

	public void OnAttackHit() {
		if (!_attackHit && _attackMarker!=null) {
			_attackMarker.Visible = true;
			_attackMarker.Scale = Vector3.One*0.01f;
			
			if(_tween!=null && _tween.IsRunning())
				_tween.Kill();
			
			_tween = CreateTween();
			_tween.TweenProperty(_attackMarker, "scale", Vector3.One, 0.3f).SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.Out);
			
			var rotation = CreateTween();
			rotation.TweenProperty(_attackMarker, "rotation", Vector3.Up*Mathf.Tau, 1.5f).From(Vector3.Zero);
			rotation.SetLoops(6);
			
			_tween.Parallel().TweenSubtween(rotation);
			
			_tween.TweenProperty(_attackMarker, "scale", Vector3.One*0.01f, 0.2f).SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.In);

			_tween.TweenCallback(Callable.From(() => _attackMarker.Visible = false));
		}
		_attackHit = true;
	}
	
	public override void OnStep() {
		PlanTurn();
	}

	private void PlanTurn() {
		if (_path == null || _nextIndex>=_path.Curve.PointCount)
			return;
		
		var current = BoardPosition;
		var target = Board.ToBoardPosition(_path.ToGlobal(_path.Curve.GetPointPosition(_nextIndex)));

		if (current == target) { // target reached
			if (_reverse) {
				_nextIndex--;
				if (_nextIndex < 0) {
					_nextIndex = 1;
					_reverse = false;
				}
			} else {
				_nextIndex++;
				if (_nextIndex >= _path.Curve.PointCount) {
					_nextIndex = _path.Curve.PointCount-2;
					_reverse = true;
				}
			}
			target = Board.ToBoardPosition(_path.ToGlobal(_path.Curve.GetPointPosition(_nextIndex)));
		}
		
		MovementLeft = current == target ? 0 : 1;
		
		if (target.X < current.X) MovementDirection = BoardOrientation.West;
		else if (target.X > current.X) MovementDirection = BoardOrientation.East;
		else if (target.Y < current.Y) MovementDirection = BoardOrientation.North;
		else if (target.Y > current.Y) MovementDirection = BoardOrientation.South;
	}

	public override void OnTurnEnd() {
		base.OnTurnEnd();
		PlanTurn();
	}

}
