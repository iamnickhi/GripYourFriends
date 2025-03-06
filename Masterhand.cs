using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Masterhand : RigidBody3D
{
	Marker3D TargetPosNode;
	[Export]
	public float HandRotateSpeed = 0.2f; 
	[Export]
	public float HandFollowSpeed = 7f;
	private Vector3 MarkerVel = Vector3.Zero;
	private Vector3 PrevMarkerPos = Vector3.Inf;
	private Godot.Collections.Array<Node3D> collisions;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TargetPosNode = GetParent<Marker3D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		collisions = GetCollidingBodies();
	}

    private void RotationFollow(PhysicsDirectBodyState3D state)
    {
		Vector3 forwardLocalAxis = new(0, 0, -1);
        Vector3 forwardDir = (GlobalTransform.Basis * forwardLocalAxis).Normalized();

		Vector3 targetDir = (TargetPosNode.GlobalBasis * forwardLocalAxis).Normalized();
		float localSpeed = Mathf.Clamp(HandRotateSpeed, 0.0f, 0.75f * Mathf.Acos(forwardDir.Dot(targetDir)));
        if (forwardDir.Dot(targetDir) > 1e-4)
        {
            AngularVelocity = forwardDir.Cross(targetDir) * localSpeed / state.Step;
        }
		GlobalRotation = new Vector3(GlobalRotation.X, GlobalRotation.Y, TargetPosNode.GlobalRotation.Z);
    }
	
	private void PositionFollow(PhysicsDirectBodyState3D state)
	{
		Vector3 targetPos = TargetPosNode.GlobalPosition;
		float localSpeed = Mathf.Clamp(HandFollowSpeed, 0.0f, 2*(targetPos - GlobalPosition + MarkerVel).Length());
		ApplyCentralForce((targetPos - GlobalPosition) * localSpeed / state.Step);
	}

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		TrackMarkerVel(state.Step);
		RotationFollow(state);
		PositionFollow(state);
    }
	
	private void TrackMarkerVel(float step)
	{
		if (PrevMarkerPos != Vector3.Inf)
		{
			MarkerVel = (TargetPosNode.GlobalPosition - PrevMarkerPos) / step;
		}
		PrevMarkerPos = TargetPosNode.GlobalPosition;
	}
	
	private float SpeedByDistance(float distance)
	{
		return 0f;
	}
}
