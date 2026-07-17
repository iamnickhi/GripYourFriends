using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks.Dataflow;
namespace Gripper;

public partial class Masterhand : RigidBody3D
{
	public Marker3D TargetPosNode;
	Vector3 SmoothedTargetPos;
	Quaternion SmoothedRotDelta = Quaternion.Identity;
	CharacterBody3D player;
	[Export]
	public float HandFollowSpeed = 9f;
	[Export]
	public float HandTorqueSpeed = 1000f;
	private Vector3 MarkerVel = Vector3.Zero;
	private Vector3 PrevMarkerPos = Vector3.Inf;
	private Godot.Collections.Array<Node3D> collisions = [];
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
	private Area3D GripArea;
	public RayCast3D GripCast;
	public bool IsColliding
	{
		get=> collisions.Count > 0;
	}
	public Node3D ObjectGripping;
	public bool IsGripping = false;
	public bool GrippingAir = false;
	private List<PhysicsBody3D> grippableBodies = [];
	private uint[] grippableLayers = { 1, 2, 3};
	private uint grippingType = 0; 
	public Node3D currentlyGripping = null;
	public Vector3 ropeDir;
	public Vector3 GrabOffset = Vector3.Zero;
	public Vector3 HandOffset = Vector3.Zero;
	public Transform3D IndexIKTransform;
	private Marker3D IndexIKTarget;
	private Transform3D[] FingieOGTransforms;
	private Marker3D[] FingieIKTargets;
	public Vector3 IndexFlexOffset = Vector3.Zero;
	private RayCast3D[] IndexFlexRayCasts;
	private RayCast3D[] MiddleFlexRayCasts;
	private RayCast3D[] RingFlexRayCasts;
	private RayCast3D[] PinkyFlexRayCasts;
	private RayCast3D[] ThumbFlexRayCasts;
	//private bool IndexIsGripping = false;
	private RayCast3D[][] FingieFlexCasts;
	private bool[] FingersAreGripping = [false, false, false, false, false];
	private Vector3[] FingieFlexOffsets = new Vector3[5];
	private Marker3D[] FingieFullFlexes;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetParent().GetParent().GetParent<CharacterBody3D>();
		AddCollisionExceptionWith(player);
		TargetPosNode = GetParent<Marker3D>();
		GripArea = GetNode<Area3D>("GripArea");
		GripCast = GetNode<RayCast3D>("RayCast3D");

		PalmCollision = GetNode<CollisionShape3D>("PalmCollision");
		PalmPosition = GetNode<Marker3D>("Skeleton3D/PalmPosition/Marker3D");

		IndexShaftCollision = GetNode<CollisionShape3D>("IndexShaftCollision");
		IndexShaftPosition = GetNode<Marker3D>("Skeleton3D/IndexShaftPosition/Marker3D");
		IndexBaseCollision = GetNode<CollisionShape3D>("IndexBaseCollision");
		IndexBasePosition = GetNode<Marker3D>("Skeleton3D/IndexBasePosition/Marker3D");
		IndexTipCollision = GetNode<CollisionShape3D>("IndexTipCollision");
		IndexTipPosition = GetNode<Marker3D>("Skeleton3D/IndexTipPosition/Marker3D");
		IndexIKTarget = GetNode<Marker3D>("IndexIKTarget");


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
		ThumbFlexRayCasts = [GetNode<RayCast3D>("ThumbFlexRayCast2"), GetNode<RayCast3D>("ThumbFlexRayCast1")];
	

		FingieFlexCasts = [[GetNode<RayCast3D>("IndexFlexRayCast2"), GetNode<RayCast3D>("IndexFlexRayCast1")], 
						   [GetNode<RayCast3D>("MiddleFlexRayCast2"), GetNode<RayCast3D>("MiddleFlexRayCast1")],
						   [GetNode<RayCast3D>("RingFlexRayCast2"), GetNode<RayCast3D>("RingFlexRayCast1")],
						   [GetNode<RayCast3D>("PinkyFlexRayCast2"), GetNode<RayCast3D>("PinkyFlexRayCast1")],
						   [GetNode<RayCast3D>("ThumbFlexRayCast2"), GetNode<RayCast3D>("ThumbFlexRayCast1")]];
		FingieIKTargets = [GetNode<Marker3D>("IndexIKTarget"), GetNode<Marker3D>("MiddleIKTarget"), GetNode<Marker3D>("RingIKTarget"), 
						   GetNode<Marker3D>("PinkyIKTarget"), GetNode<Marker3D>("ThumbIKTarget")];
		FingieOGTransforms = [GetNode<Marker3D>("IndexIKTarget").Transform, GetNode<Marker3D>("MiddleIKTarget").Transform, 
							  GetNode<Marker3D>("RingIKTarget").Transform, GetNode<Marker3D>("PinkyIKTarget").Transform, GetNode<Marker3D>("ThumbIKTarget").Transform];
		FingieFullFlexes = [GetNode<Marker3D>("IndexFullFlex"), GetNode<Marker3D>("MiddleFullFlex"), GetNode<Marker3D>("RingFullFlex"), GetNode<Marker3D>("PinkyFullFlex"), GetNode<Marker3D>("ThumbFullFlex")];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (currentlyGripping != null)
		{
			Vector3 grabPoint = currentlyGripping.ToGlobal(GrabOffset);
			Vector3 target = GlobalTransform.Origin;
			Vector3 direction = target - grabPoint;
			Vector3 force = direction * 50.0f;
			//currentlyGripping.ApplyForce(force, grabPoint - currentlyGripping.GlobalTransform.Origin);

			GripCast.TargetPosition = GripCast.ToLocal(grabPoint);
			GripCast.Enabled = true;
			GripCast.ForceRaycastUpdate();
			
		}
	}

    private void RotationFollow(PhysicsDirectBodyState3D state)
    {
		Quaternion currentRot = GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion targetRot = TargetPosNode.GetGlobalTransform().Basis.GetRotationQuaternion();
		Quaternion rotDelta = (targetRot * currentRot.Inverse()).Normalized();
		if (rotDelta.W < 0) rotDelta = -rotDelta;
		if (rotDelta.GetAngle() < .005f) return;
		Vector3 torque = rotDelta.GetAxis() * rotDelta.GetAngle() * HandTorqueSpeed * state.Step;
    	float dampingFactor = Mathf.Clamp(rotDelta.GetAngle() / Mathf.Pi, 0.1f, 1.0f);
		ApplyTorque(torque * dampingFactor);
    }
	
	private void PositionFollow(PhysicsDirectBodyState3D state)
	{
	 	Vector3 targetPos = TargetPosNode.GlobalPosition;

		Vector3 positionError = targetPos - GlobalPosition;
		Vector3 desiredVelocity = positionError / state.Step;
		Vector3 velocityError = desiredVelocity - state.LinearVelocity;
		float dampingFactor = Mathf.Clamp(positionError.Length(), .1f, 1.0f);
		float forceStrength = Mathf.Clamp(HandFollowSpeed, .0f, velocityError.Length() *1.5f * dampingFactor);
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
		if (currentlyGripping != null)
		{
			UpdateGrippingPoints();
			if (currentlyGripping is LargeBody)
			{
				GlobalPosition = currentlyGripping.GlobalPosition + HandOffset;
			}
		}
		else
		{
			PositionFollow(state);
		}
		collisions = GetCollidingBodies();
		UpdateCollisionShapes();
		RotationFollow(state);
		IKUpdate(state.Step);
    }

	private void UpdateGrippingPoints()
	{
		Vector3 handPos = GlobalTransform.Origin;
		Vector3 objPos = currentlyGripping.GlobalTransform.Origin;
		Vector3 dir = (objPos - handPos).Normalized();

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(handPos, handPos + dir * 100.0f);
		query.CollideWithBodies = true;

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0 && result.ContainsKey("collider"))
		{
			Object hit = (Object)result["collider"];
			if (hit != null && hit.Equals(currentlyGripping))
			{
				Vector3 hitPos = (Vector3)result["position"];
				GrabOffset = currentlyGripping.ToLocal(hitPos);
			}
		}
	}

	public void Grab()
	{
		switch (grippableBodies.Count)
		{
			case 0:
				GrippingAir = true;
				break;
			case 1:
				currentlyGripping = grippableBodies[0];
				FingieIK();
				IsGripping = true;
				if (currentlyGripping is LargeBody)
				{
					Vector3 handPos = GlobalTransform.Origin;
					Vector3 objPos = currentlyGripping.GlobalTransform.Origin;
					HandOffset = handPos - objPos;
				}
				grippingType = grippableBodies[0].CollisionLayer;
				AddCollisionExceptionWith(currentlyGripping);
				break;
			case > 1:
						break;
					default:
						break;
					}
				}
	public void FingieIK()
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 fullyFlexed = Vector3.Zero;
			PhysicsBody3D collidingBody = null;
			Vector3 colliderOffset = Vector3.Zero;
			foreach (RayCast3D rayCast in FingieFlexCasts[i])
			{
				rayCast.ForceRaycastUpdate();
				var collision = rayCast.GetCollider(); 
				if (collision is PhysicsBody3D)
				{
					collidingBody = collision as PhysicsBody3D;
					colliderOffset = rayCast.GetCollisionPoint();
				}
			}

			if (collidingBody is null)
			{
				FingersAreGripping[i] = false;
			}
			else
			{
				FingersAreGripping[i] = true;
				FingieFlexOffsets[i] =  colliderOffset - collidingBody.GlobalPosition;
			}
		}
	}
	public void IKUpdate(float delta)
	{
		if (!IsGripping && !GrippingAir) return;
		for (int i = 0; i < 5; i++)
		{
			if (GrippingAir || !FingersAreGripping[i])
			{
				FingieIKTargets[i].GlobalPosition = FingieFullFlexes[i].GlobalPosition;
				//FingieIKTargets[i].GlobalPosition.Lerp(FingieFullFlexes[i].GlobalPosition, 1);
			}
			else if (FingersAreGripping[i])
			{
				FingieIKTargets[i].GlobalPosition =  FingieFlexOffsets[i] + currentlyGripping.GlobalPosition;
			}
		}
	}
	
	public void UnGrab()
	{
		if (currentlyGripping is not null) RemoveCollisionExceptionWith(currentlyGripping);
		if (currentlyGripping is LargeBody grip)
		{
			//grip.RemoveGripper(this);
		}
		currentlyGripping = null;
		IsGripping = false;
		GrippingAir = false;
		grippingType = 0;
		for (int i = 0; i < 5; i++)
		{
			FingieIKTargets[i].Transform = FingieOGTransforms[i];
		}
	}

	private void TrackMarkerVel(float step)
	{
		if (PrevMarkerPos != Vector3.Inf)
		{
			MarkerVel = (TargetPosNode.GlobalPosition - PrevMarkerPos) / step;
		}
		PrevMarkerPos = TargetPosNode.GlobalPosition;
	}
	
	private void _GripAreaBodyEntered(Node3D body)
	{
		if (body == this || body == player) return;
		if (body is PhysicsBody3D)
		{
			grippableBodies.Add(body as PhysicsBody3D);
		}
	}
	private void _GripAreaBodyExited(Node3D body)
	{
		if (body == this || body == player) return;
		if (body is PhysicsBody3D)
		{
			grippableBodies.Remove(body as PhysicsBody3D);
		}
		
	}
}
