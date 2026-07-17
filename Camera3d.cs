using Godot;
using System;
using System.Net.Http;

public partial class Camera3d : Camera3D
{
	private Node3D Head;
	// the sensitivity of the analog stick that controls camera rotation. lower is less sensitive and higher is more sensitive.
	[Export(PropertyHint.Range, "0.001, 1, 0.001")]
	public double LookSensitivity { get; set; } = 0.035;	[Export]
	public double MouseSensitivity { get; set; } = 0.1;
	// invert the x axis input for the camera.
	[Export]
	public bool InvertCameraXAxis { get; set; } = false;
	// invert the y axis input for the camera.
	[Export]
	public bool InvertCameraYAxis { get; set; } = false;
	private Vector2 MouseInput = new Vector2(0,0);
	// this only affects how the camera is handled, the rest should be covered by adding controller inputs to the existing actions in the input map.
	[Export]
	public bool ControllerSupport { get; set; } = false;
	// whether the player can use movement inputs. does not stop outside forces or jumping. see jumping enabled.
	// use the input map to map a controller input to an action and add a reference to it to this dictionary to be used in the script.
	[Export]
	public Godot.Collections.Dictionary<string, string> ControllerControls { get; set; } = new Godot.Collections.Dictionary<string, string> {
		["look_left"] = "look_left",
		["look_right"] = "look_right",
		["look_up"] = "look_up",
		["look_down"] = "look_down"
		};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Head = GetParent<Node3D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    public override void _PhysicsProcess(double delta)
    { 
		//HandleCameraMovement();
    }
    public void HandleCameraMovement()
	{
		if (InvertCameraXAxis)
		{
			RotationDegrees = new Vector3(RotationDegrees.X, RotationDegrees.Y - (MouseInput.X * (float) MouseSensitivity * -1), RotationDegrees.Z);
		}
		else
		{
            RotationDegrees = new Vector3(RotationDegrees.X, RotationDegrees.Y - (MouseInput.X * (float) MouseSensitivity), RotationDegrees.Z);
		}

		if (InvertCameraYAxis)
		{
            RotationDegrees = new Vector3(RotationDegrees.X - (MouseInput.Y * (float) MouseSensitivity * -1), RotationDegrees.Y, RotationDegrees.Z);
		}
		else
		{
            RotationDegrees = new Vector3(RotationDegrees.X - (MouseInput.Y * (float) MouseSensitivity), RotationDegrees.Y, RotationDegrees.Z);
		}

		if (ControllerSupport)
		{
            Vector2 controllerViewRotation = Input.GetVector(
                ControllerControls["look-down"], ControllerControls["look-up"],
                ControllerControls["look-right"], ControllerControls["look-left"]
            ) * (float) LookSensitivity;
			if (InvertCameraXAxis)
			{
                Rotation = new Vector3(Rotation.X + (controllerViewRotation.X * -1), Rotation.Y, Rotation.Z);
			}
			else
			{
                Rotation = new Vector3(Rotation.X + controllerViewRotation.X, Rotation.Y, Rotation.Z);
			}

			if (InvertCameraYAxis)
			{
                Rotation = new Vector3(Rotation.X, Rotation.Y + (controllerViewRotation.Y * -1), Rotation.Z);
			}
			else
			{
                Rotation = new Vector3(Rotation.X, Rotation.Y + controllerViewRotation.Y, Rotation.Z);
			}

		}
		MouseInput = Vector2.Zero;
        Rotation = new Vector3(
            Mathf.Clamp(Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)),
            Rotation.Y,
            Rotation.Z
        );

	}
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			MouseInput.X += ((InputEventMouseMotion)@event).Relative.X;
			MouseInput.Y += ((InputEventMouseMotion)@event).Relative.Y;
		}
	}


}

