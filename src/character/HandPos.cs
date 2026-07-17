using Godot;
using System;
namespace Gripper;

public partial class HandPos : Marker3D
{
	private Vector3 lastPosition;
	public Vector3 Velocity { get; private set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lastPosition = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Velocity = (GlobalPosition - lastPosition) / (float)delta;

		lastPosition = GlobalPosition;
	}
}
