using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace Gripper;

public partial class Masterhand : RigidBody3D
{
	Marker3D TargetPosNode;
	Vector3 SmoothedTargetPos;
	Quaternion SmoothedRotDelta = Quaternion.Identity;
	CharacterBody3D Player;
	[Export]
	public float HandFollowSpeed = 9f;
	[Export]
	public float HandTorqueSpeed = 1000f;
	private Vector3 MarkerVel = Vector3.Zero;
	private Vector3 PrevMarkerPos = Vector3.Inf;
	private Godot.Collections.Array<Node3D> collisions = [];
	private AnimationPlayer animationPlayer;
	private CollisionShape3D PalmCollision;
	private Marker3D PalmPosition;
	private CollisionShape3D IndexBaseCollision;
	private CollisionShape3D IndexShaftCollision;
	private CollisionShape3D IndexTipCollision;
	private Marker3D IndexBasePosition;
	private Marker3D IndexShaftPosition;
	private Marker3D IndexTipPosition;
	private CollisionShape3D MiddleBaseCollision;
	private CollisionShape3D MiddleShaftCollision;
	private CollisionShape3D MiddleTipCollision;
	private Marker3D MiddleBasePosition;
	private Marker3D MiddleShaftPosition;
	private Marker3D MiddleTipPosition;
	private CollisionShape3D RingBaseCollision;
	private CollisionShape3D RingShaftCollision;
	private CollisionShape3D RingTipCollision;
	private Marker3D RingBasePosition;
	private Marker3D RingShaftPosition;
	private Marker3D RingTipPosition;
	private CollisionShape3D PinkyBaseCollision;
	private CollisionShape3D PinkyShaftCollision;
	private CollisionShape3D PinkyTipCollision;
	private Marker3D PinkyBasePosition;
	private Marker3D PinkyShaftPosition;
	private Marker3D PinkyTipPosition;
	private CollisionShape3D ThumbBaseCollision;
	private CollisionShape3D ThumbShaftCollision;
	private Marker3D ThumbBasePosition;
	private Marker3D ThumbShaftPosition;
	public bool IsColliding
	{
		get=> collisions.Count > 0;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TargetPosNode = GetParent<Marker3D>();
		Player = GetParent().GetParent().GetParent<CharacterBody3D>();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		PalmCollision = GetNode<CollisionShape3D>("PalmCollision");
		PalmPosition = GetNode<Marker3D>("Skeleton3D/PalmPosition/Marker3D");

		IndexShaftCollision = GetNode<CollisionShape3D>("IndexShaftCollision");
		IndexShaftPosition = GetNode<Marker3D>("Skeleton3D/IndexShaftPosition/Marker3D");
		IndexBaseCollision = GetNode<CollisionShape3D>("IndexBaseCollision");
		IndexBasePosition = GetNode<Marker3D>("Skeleton3D/IndexBasePosition/Marker3D");
		IndexTipCollision = GetNode<CollisionShape3D>("IndexTipCollision");
		IndexTipPosition = GetNode<Marker3D>("Skeleton3D/IndexTipPosition/Marker3D");


		MiddleShaftCollision = GetNode<CollisionShape3D>("MiddleShaftCollision");
		MiddleShaftPosition = GetNode<Marker3D>("Skeleton3D/MiddleShaftPosition/Marker3D");
		MiddleBaseCollision = GetNode<CollisionShape3D>("MiddleBaseCollision");
		MiddleBasePosition = GetNode<Marker3D>("Skeleton3D/MiddleBasePosition/Marker3D");
		MiddleTipCollision = GetNode<CollisionShape3D>("MiddleTipCollision");
		MiddleTipPosition = GetNode<Marker3D>("Skeleton3D/MiddleTipPosition/Marker3D");


		RingShaftCollision = GetNode<CollisionShape3D>("RingShaftCollision");
		RingShaftPosition = GetNode<Marker3D>("Skeleton3D/RingShaftPosition/Marker3D");
		RingBaseCollision = GetNode<CollisionShape3D>("RingBaseCollision");
		RingBasePosition = GetNode<Marker3D>("Skeleton3D/RingBasePosition/Marker3D");
		RingTipCollision = GetNode<CollisionShape3D>("RingTipCollision");
		RingTipPosition = GetNode<Marker3D>("Skeleton3D/RingTipPosition/Marker3D");


		PinkyShaftCollision = GetNode<CollisionShape3D>("PinkyShaftCollision");
		PinkyShaftPosition = GetNode<Marker3D>("Skeleton3D/PinkyShaftPosition/Marker3D");
		PinkyBaseCollision = GetNode<CollisionShape3D>("PinkyBaseCollision");
		PinkyBasePosition = GetNode<Marker3D>("Skeleton3D/PinkyBasePosition/Marker3D");
		PinkyTipCollision = GetNode<CollisionShape3D>("PinkyTipCollision");
		PinkyTipPosition = GetNode<Marker3D>("Skeleton3D/PinkyTipPosition/Marker3D");


		ThumbShaftCollision = GetNode<CollisionShape3D>("ThumbShaftCollision");
		ThumbShaftPosition = GetNode<Marker3D>("Skeleton3D/ThumbShaftPosition/Marker3D");
		ThumbBaseCollision = GetNode<CollisionShape3D>("ThumbBaseCollision");
		ThumbBasePosition = GetNode<Marker3D>("Skeleton3D/ThumbBasePosition/Marker3D");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		collisions = GetCollidingBodies();
	}

    private void RotationFollow(PhysicsDirectBodyState3D state)
    {
		Quaternion currentRot = GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion targetRot = TargetPosNode.GetGlobalTransform().Basis.GetRotationQuaternion();
		Quaternion rotDelta = (targetRot * currentRot.Inverse()).Normalized();
		rotDelta.Normalized();
		if (rotDelta.W < 0) rotDelta = -rotDelta;
		Vector3 torque = rotDelta.GetAxis() * rotDelta.GetAngle() * HandTorqueSpeed * state.Step;
		ApplyTorque(torque);
    }
	
	private void PositionFollow(PhysicsDirectBodyState3D state)
	{
	 Vector3 targetPos = TargetPosNode.GlobalPosition;

    	// Smooth out the target position changes to avoid abrupt jumps
    	SmoothedTargetPos = SmoothedTargetPos.Lerp(targetPos, 1.0f - Mathf.Exp(-20f * (float)state.Step));
		Vector3 positionError = SmoothedTargetPos - GlobalPosition;
		Vector3 desiredVelocity = positionError / state.Step;
		Vector3 velocityError = desiredVelocity - state.LinearVelocity;
		float dampingFactor = Mathf.Clamp(positionError.Length() / .1f, .1f, 1.0f);
		float forceStrength = Mathf.Clamp(HandFollowSpeed, .0f, velocityError.Length() * dampingFactor);
		GD.Print(velocityError.Length());
		ApplyCentralForce(velocityError * forceStrength);
	}
	
	private void UpdateCollisionShapes()
	{
		PalmCollision.GlobalTransform = PalmPosition.GlobalTransform;

		IndexBaseCollision.GlobalTransform = IndexBasePosition.GlobalTransform;
		IndexShaftCollision.GlobalTransform = IndexShaftPosition.GlobalTransform;
		IndexTipCollision.GlobalTransform = IndexTipPosition.GlobalTransform;

		MiddleBaseCollision.GlobalTransform = MiddleBasePosition.GlobalTransform;
		MiddleShaftCollision.GlobalTransform = MiddleShaftPosition.GlobalTransform;
		MiddleTipCollision.GlobalTransform = MiddleTipPosition.GlobalTransform;

		RingBaseCollision.GlobalTransform = RingBasePosition.GlobalTransform;
		RingShaftCollision.GlobalTransform = RingShaftPosition.GlobalTransform;
		RingTipCollision.GlobalTransform = RingTipPosition.GlobalTransform;

		PinkyBaseCollision.GlobalTransform = PinkyBasePosition.GlobalTransform;
		PinkyShaftCollision.GlobalTransform = PinkyShaftPosition.GlobalTransform;
		PinkyTipCollision.GlobalTransform = PinkyTipPosition.GlobalTransform;

		ThumbBaseCollision.GlobalTransform = ThumbBasePosition.GlobalTransform;
		ThumbShaftCollision.GlobalTransform = ThumbShaftPosition.GlobalTransform;
	}

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		UpdateCollisionShapes();
		TrackMarkerVel(state.Step);
		RotationFollow(state);
		PositionFollow(state);
    }
	public void Grab()
	{
		animationPlayer.Play("ArmatureAction_001");
	}
	
	public void UnGrab()
	{
		animationPlayer.PlayBackwards("ArmatureAction_001");
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
