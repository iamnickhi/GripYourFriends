using Godot;
using System;
using System.Collections.Generic;
namespace Gripper;

public partial class LargeBody : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // List of all hands currently grabbing this body
    private List<Masterhand> Grippers = new List<Masterhand>();

    // Add a grabber
    public void AddGripper(Masterhand gripper)
    {
		Grippers.Add(gripper);
    }

    // Remove a grabber
    public void RemoveGripper(Masterhand gripper)
    {
		if (Grippers.Contains(gripper))
		{
			Grippers.Remove(gripper);
		}
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        /*foreach (var g in Grippers)
        {
            // World-space grab point on this rigidbody
            Vector3 grabPoint = ToGlobal(g.GrabOffset);

            // Desired target position (hand position)
            Vector3 target = g.GlobalTransform.Origin;

            // Apply damping to prevent oscillation
            Vector3 localVel = state.GetVelocityAtLocalPosition(grabPoint);

            // Apply combined force
            state.ApplyForce(g.ropeDir * 50.0f * state.Step + localVel, grabPoint - GlobalTransform.Origin);
			GD.Print("being puleld");
        }
		*/
    }
}
