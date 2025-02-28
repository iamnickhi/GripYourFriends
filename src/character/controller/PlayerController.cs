// COPYRIGHT Colormatic Studios
// MIT license
// Quality Godot First Person Controller v2
//extends CharacterBody3D
using Godot;
using System;
using Debug;
using System.Runtime.Serialization;
namespace player;

public partial class PlayerController : CharacterBody3D
{
	#region character export group
	[ExportCategory("CharacterController")]
	// The speed that the character moves at without crouching or sprinting.
	[Export]
	public double BaseSpeed { get; set; } = 3.0;
	
	// the speed that the character moves at when sprinting.
	[Export]
	public double SprintSpeed { get; set; } = 6.0;
	// the speed that the character moves at when crouching.
	[Export]
	public double CrouchSpeed { get; set; }  = 1.0;
	// how fast the character speeds up and slows down when motion smoothing is on.
	[Export]
	public double Acceleration { get; set; } = 10.0;
	// how high the player jumps.
	[Export]
	public double JumpVelocity { get; set; } = 4.5;
	// how far the player turns when the mouse is moved.
	[Export]
	public double MouseSensitivity { get; set; } = 0.1;
	// invert the x axis input for the camera.
	[Export]
	public bool InvertCameraXAxis { get; set; } = false;
	// invert the y axis input for the camera.
	[Export]
	public bool InvertCameraYAxis { get; set; } = false;
	// whether the player can use movement inputs. does not stop outside forces or jumping. see jumping enabled.
	[Export]
	public bool Immobile { get; set; } = false;
	// the reticle file to import at runtime. by default are in res://addons/fpc/reticles/. set to an empty string to remove.
	[Export(PropertyHint.File)]
	public string DefaultReticle { get; set; } = "";

#endregion

#region nodes export group

	[ExportGroup("nodes")]
	// a reference to the camera for use in the character script. this is the parent node to the camera and is rotated instead of the camera for mouse input.
	[Export]
	public Node3D Head { get; set; }
	// a reference to the camera for use in the character script.
	[Export]
	public Camera3D Camera {get; set; }
	// a reference to the headbob animation for use in the character script.
	[Export]
	public AnimationPlayer HeadbobAnimation { get; set; }
	// a reference to the jump animation for use in the character script.
	[Export]
	public AnimationPlayer JumpAnimation { get; set; }
	// a reference to the crouch animation for use in the character script.
	[Export]
	public AnimationPlayer CrouchAnimation { get; set; }
	// a reference to the the player's collision shape for use in the character script.
	[Export]
	public CollisionShape3D CollisionMesh { get; set; }

#endregion

#region controls export group

	// we are using ui controls because they are built into godot engine so they can be used right away
	[ExportGroup("controls")]
	// use the input map to map a mouse/keyboard input to an action and add a reference to it to this dictionary to be used in the script.
	[Export]
	public Godot.Collections.Dictionary<string, string> Controls { get; set; } = new Godot.Collections.Dictionary<string, string> {
		["left"] = "ui_left",
		["right"] = "ui_right",
		["forward"] = "ui_up",
		["backward"] = "ui_down",
		["jump"] = "ui_accept",
		["crouch"] = "crouch",
		["sprint"] = "sprint",
		["pause"] = "ui_cancel"
		};
	[ExportSubgroup("controller specific")]
	// this only affects how the camera is handled, the rest should be covered by adding controller inputs to the existing actions in the input map.
	[Export]
	public bool ControllerSupport { get; set; } = false;
	// use the input map to map a controller input to an action and add a reference to it to this dictionary to be used in the script.
	[Export]
	public Godot.Collections.Dictionary<string, string> ControllerControls { get; set; } = new Godot.Collections.Dictionary<string, string> {
		["look_left"] = "look_left",
		["look_right"] = "look_right",
		["look_up"] = "look_up",
		["look_down"] = "look_down"
		};
	// the sensitivity of the analog stick that controls camera rotation. lower is less sensitive and higher is more sensitive.
	[Export(PropertyHint.Range, "0.001, 1, 0.001")]
	public double LookSensitivity { get; set; } = 0.035;

#endregion

#region feature settings export group

	[ExportGroup("feature settings")]
	// enable or disable jumping. useful for restrictive storytelling environments.
	[Export]
	public bool JumpingEnabled { get; set; } = true;
	// whether the player can move in the air or not.
	[Export]
	public bool InAirMomentum { get; set; } = true;
	// smooths the feel of walking.
	[Export]
	public bool MotionSmoothing { get; set; } = true;
	// enables or disables sprinting.
	[Export]
	public bool SprintEnabled { get; set; } = true;
	// toggles the sprinting state when button is pressed or requires the player to hold the button down to remain sprinting.int
	public enum SprintSettingEnum
	{
		HoldToSprint,
		ToggleSprint,
	}
	[Export]
	public SprintSettingEnum SprintMode { get; set; } = 0;
	// enables or disables crouching.
	[Export]
	public bool CrouchEnabled { get; set; } = true;
	// toggles the crouch state when button is pressed or requires the player to hold the button down to remain crouched.
	public enum CrouchSettingEnum
	{
		HoldToCrouch,
		ToogleCrouch,
	}
	[Export]
	public CrouchSettingEnum CrouchMode { get; set; } = 0;
	// wether sprinting should effect fov.
	[Export]
	public bool DynamicFov { get; set; } = true;
	// if the player holds down the jump button, should the player keep hopping.
	[Export]
	public bool ContinuousJumping { get; set; } = true;
	// enables the view bobbing animation.
	[Export]
	public bool ViewBobbing { get; set; } = true;
	// enables an immersive animation when the player jumps and hits the ground.
	[Export]
	public bool JumpAnimationCustom { get; set; } = true;
	// this determines wether the player can use the pause button, not wether the game will actually pause.
	[Export]
	public bool PausingEnabled { get; set; } = true;
	// use with caution.
	[Export]
	public bool GravityEnabled { get; set; } = true;
	// if your game changes the gravity value during gameplay, check this property to allow the player to experience the change in gravity.
	[Export]
	public bool DynamicGravity { get; set; } = false;
#endregion
		#region member variable initialization
	private double _speed;
	private double _currentSpeed;
	private string _state;
	private bool _lowCeiling;
	private bool _wasOnFloor;
	private Control Reticle;
	double Gravity = (ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble()); // don't set this as a const, see the gravity section in _physics_process
	private Vector2 MouseInput = new Vector2(0,0);
	public ShapeCast3D CrouchCeilingDetection;
	public Control UserInterface;
	public DebugPanel DebugPanel = new DebugPanel();


	// called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CrouchCeilingDetection = GetNode<ShapeCast3D>("CrouchCeilingDetection");
		UserInterface = GetNode<Control>("UserInterface");
		Head = GetNode<Node3D>("Head");
#endregion

#region main control flow

		//it is safe to comment this line if your game doesn't start with the mouse captured
		Input.MouseMode = Input.MouseModeEnum.Captured;

		//if the controller is rotated in a certain direction for game design purposes, redirect this rotation into the head.
		Head.Rotation = new Vector3(Head.Rotation.X, Rotation.Y, Head.Rotation.Z);
		Rotation = new Vector3(Rotation.X, 0, Rotation.Z);

		if (DefaultReticle.Length > 0)
			{
				ChangeReticle(DefaultReticle);
			}

		InitializeAnimations();
		CheckControls();
		EnterNormalState();
	}

	// called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (PausingEnabled)
		{
		HandlePausing();
		}

		UpdateDebugMenuPerFrame();
	}

    public override void _PhysicsProcess(double delta)
    {
		if (DynamicGravity)
		{
			Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble();
		}
		if (!IsOnFloor() && (!double.IsNaN(Gravity)) && GravityEnabled)
		{
			Velocity = Velocity - new Vector3(0, (float)(Gravity * delta), 0);
		}

		HandleJumping();

		Vector2 InputDir = Vector2.Zero;

		if (!Immobile) // immobility works by interrupting user input, so other forces can still be applied to the player
			InputDir = Input.GetVector(Controls["left"], Controls["right"], Controls["forward"], Controls["backward"]);

		HandleMovement(delta, InputDir);

		HandleHeadRotation();

		// the player is not able to stand up if the ceiling is too low
		_lowCeiling = CrouchCeilingDetection.IsColliding();

		HandleState(InputDir);
		if (DynamicFov) // this may be changed to an animationplayer
		{
			UpdateCameraFov();
		}

		if (ViewBobbing)
		{
			PlayHeadbobAnimation(InputDir);
		}

		if (JumpAnimation.IsNodeReady())
		{
			PlayJumpAnimation();
		}

		UpdateDebugMenuPerTick();

		_wasOnFloor = IsOnFloor(); // this must always be at the end of physics_process

#endregion

    }

#region input handling
	public void HandleJumping()
	{
		if (JumpingEnabled)
		{
			if (ContinuousJumping)
			{
				if (Input.IsActionPressed(Controls["jump"]) && IsOnFloor() && !_lowCeiling)
				{
					if (JumpAnimation.IsNodeReady())
					{
						JumpAnimation.Play("jump", 0.25);
					}
					Velocity += new Vector3(0, (float) JumpVelocity, 0);
				}
			}
			else
			{
				if (Input.IsActionJustPressed(Controls["jump"]) && IsOnFloor() && !_lowCeiling)
				{
					if (JumpAnimation.IsNodeReady())
					{
						JumpAnimation.Play("jump", 0.25);
					}
					Velocity += new Vector3(0, (float) JumpVelocity, 0);
				}
			}
		}
	}
	

	public void HandleMovement(double delta, Vector2 InputDir)
	{
		var direction = new Vector3(InputDir.Rotated(-Head.Rotation.Y).X, 0, InputDir.Rotated(-Head.Rotation.Y).Y);
		MoveAndSlide();

		if (InAirMomentum)
		{
			if (IsOnFloor())
			{
				if (MotionSmoothing)
				{
					Velocity = new Vector3(
					(float)Mathf.Lerp(Velocity.X, direction.X * _speed, Acceleration * (float) delta),
					Velocity.Y,
					(float)Mathf.Lerp(Velocity.Z, direction.Z * _speed, Acceleration * (float) delta));
				}
				else
				{
					Velocity = new Vector3(direction.X * (float)_speed, Velocity.Y, direction.Z * (float)_speed);
				}
			}
		}
		else
		{
			if (MotionSmoothing)
			{
				Velocity = new Vector3(
				(float)Mathf.Lerp(Velocity.X, direction.X * _speed, Acceleration * (float) delta),
				Velocity.Y,
				(float)Mathf.Lerp(Velocity.Z, direction.Z * _speed, Acceleration * (float) delta));
			}
			else
			{
				Velocity = new Vector3(direction.X * (float)_speed, Velocity.Y, direction.Z * (float)_speed);
			}
		}

	}


	public void HandleHeadRotation()
	{
		if (InvertCameraXAxis)
		{
			Head.RotationDegrees = new Vector3(Head.RotationDegrees.X, Head.RotationDegrees.Y - (MouseInput.X * (float) MouseSensitivity * -1), Head.RotationDegrees.Z);
		}
		else
		{
            Head.RotationDegrees = new Vector3(Head.RotationDegrees.X, Head.RotationDegrees.Y - (MouseInput.X * (float) MouseSensitivity), Head.RotationDegrees.Z);
		}

		if (InvertCameraYAxis)
		{
            Head.RotationDegrees = new Vector3(Head.RotationDegrees.X - (MouseInput.Y * (float) MouseSensitivity * -1), Head.RotationDegrees.Y, Head.RotationDegrees.Z);
		}
		else
		{
            Head.RotationDegrees = new Vector3(Head.RotationDegrees.X - (MouseInput.Y * (float) MouseSensitivity), Head.RotationDegrees.Y, Head.RotationDegrees.Z);
		}

		if (ControllerSupport)
		{
            Vector2 controllerViewRotation = Input.GetVector(
                ControllerControls["look-down"], ControllerControls["look-up"],
                ControllerControls["look-right"], ControllerControls["look-left"]
            ) * (float) LookSensitivity;
			if (InvertCameraXAxis)
			{
                Head.Rotation = new Vector3(Head.Rotation.X + (controllerViewRotation.X * -1), Head.Rotation.Y, Head.Rotation.Z);
			}
			else
			{
                Head.Rotation = new Vector3(Head.Rotation.X + controllerViewRotation.X, Head.Rotation.Y, Head.Rotation.Z);
			}

			if (InvertCameraYAxis)
			{
                Head.Rotation = new Vector3(Head.Rotation.X, Head.Rotation.Y + (controllerViewRotation.Y * -1), Head.Rotation.Z);
			}
			else
			{
                Head.Rotation = new Vector3(Head.Rotation.X, Head.Rotation.Y + controllerViewRotation.Y, Head.Rotation.Z);
			}

		}
		MouseInput = Vector2.Zero;
        Head.Rotation = new Vector3(
            Mathf.Clamp(Head.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)),
            Head.Rotation.Y,
            Head.Rotation.Z
        );

	}

    public void CheckControls()
    {
        // Check if each action is mapped in the InputMap
        if (!InputMap.HasAction(Controls["jump"]))
        {
            GD.PushError("No control mapped for jumping. Please add an input map control. Disabling jump.");
            JumpingEnabled = false;
        }
        if (!InputMap.HasAction(Controls["left"]))
        {
            GD.PushError("No control mapped for move left. Please add an input map control. Disabling movement.");
            Immobile = true;
        }
        if (!InputMap.HasAction(Controls["right"]))
        {
            GD.PushError("No control mapped for move right. Please add an input map control. Disabling movement.");
            Immobile = true;
        }
        if (!InputMap.HasAction(Controls["forward"]))
        {
            GD.PushError("No control mapped for move forward. Please add an input map control. Disabling movement.");
            Immobile = true;
        }
        if (!InputMap.HasAction(Controls["backward"]))
        {
            GD.PushError("No control mapped for move backward. Please add an input map control. Disabling movement.");
            Immobile = true;
        }
        if (!InputMap.HasAction(Controls["pause"]))
        {
            GD.PushError("No control mapped for pause. Please add an input map control. Disabling pausing.");
            PausingEnabled = false;
        }
        if (!InputMap.HasAction(Controls["crouch"]))
        {
            GD.PushError("No control mapped for crouch. Please add an input map control. Disabling crouching.");
            CrouchEnabled = false;
        }
        if (!InputMap.HasAction(Controls["sprint"]))
        {
            GD.PushError("No control mapped for sprint. Please add an input map control. Disabling sprinting.");
            SprintEnabled = false;
        }
    }

#endregion

#region state handling

    public void HandleState(Vector2 moving)
    {
        // Sprinting logic
        if (SprintEnabled)
        {
            if ((int) SprintMode == 0)
            {
                if (Input.IsActionPressed(Controls["sprint"]) && _state != "crouching")
                {
                    if (!moving.IsZeroApprox())
                    {
                        if (_state != "sprinting")
                            EnterSprintState();
                    }
                    else
                    {
                        if (_state == "sprinting")
                            EnterNormalState();
                    }
                }
                else if (_state == "sprinting")
                {
                    EnterNormalState();
                }
            }
            else if ((int) SprintMode == 1)
            {
                if (!moving.IsZeroApprox())
                {
                    if (Input.IsActionPressed(Controls["sprint"]) && _state == "normal")
                        EnterSprintState();

                    if (Input.IsActionJustPressed(Controls["sprint"]))
                    {
                        switch (_state)
                        {
                            case "normal":
                                EnterSprintState();
                                break;
                            case "sprinting":
                                EnterNormalState();
                                break;
                        }
                    }
                }
                else if (_state == "sprinting")
                {
                    EnterNormalState();
                }
            }
        }

        // Crouching logic
        if (CrouchEnabled)
        {
            if ((int) CrouchMode == 0)
            {
                if (Input.IsActionPressed(Controls["crouch"]) && _state != "sprinting")
                {
                    if (_state != "crouching")
                        EnterCrouchState();
                }
                else if (_state == "crouching" && !CrouchCeilingDetection.IsColliding())
                {
                    EnterNormalState();
                }
            }
            else if ((int) CrouchMode == 1)
            {
                if (Input.IsActionJustPressed(Controls["crouch"]))
                {
                    switch (_state)
                    {
                        case "normal":
                            EnterCrouchState();
                            break;
                        case "crouching":
                            if (!CrouchCeilingDetection.IsColliding())
                                EnterNormalState();
                            break;
                    }
                }
            }
        }
    }


    public void EnterNormalState()
    {
        string prevState = _state;
        if (prevState == "crouching")
        {
            CrouchAnimation.PlayBackwards("crouch");
        }
        _state = "normal";
        _speed = BaseSpeed;
    }

    public void EnterCrouchState()
    {
        _state = "crouching";
        _speed = CrouchSpeed;
        CrouchAnimation.Play("crouch");
    }

    public void EnterSprintState()
    {
        string prevState = _state;
        if (prevState == "crouching")
        {
            CrouchAnimation.PlayBackwards("crouch");
        }
        _state = "sprinting";
        _speed = SprintSpeed;
    }

#endregion
#region Animation Handling

	public void InitializeAnimations()
	{
		// Reset the camera position
		// If you want to change the default head height, change these animations.
		HeadbobAnimation.Play("RESET");
		JumpAnimation.Play("RESET");
		CrouchAnimation.Play("RESET");
	}


	public void PlayHeadbobAnimation(Vector2 moving)
	{
		if (!moving.IsZeroApprox() && IsOnFloor())
		{
			string useHeadbobAnimation = "";
			switch (_state)
			{
				case "normal":
				case "crouching":
					useHeadbobAnimation = "walk";
					break;
				case "sprinting":
					useHeadbobAnimation = "sprint";
					break;
			}

			bool wasPlaying = false;
			if (HeadbobAnimation.CurrentAnimation == useHeadbobAnimation)
			{
				wasPlaying = true;
			}

			HeadbobAnimation.Play(useHeadbobAnimation, 0.25f);
			HeadbobAnimation.SpeedScale = ((float) _currentSpeed / (float) BaseSpeed) * 1.75f;
			
			if (!wasPlaying)
			{
				HeadbobAnimation.Seek(new Random().Next(0, 2)); // Randomize the initial headbob direction
			}
		}
		else
		{
			if (HeadbobAnimation.CurrentAnimation == "sprint" || HeadbobAnimation.CurrentAnimation == "walk")
			{
				HeadbobAnimation.SpeedScale = 1f;
				HeadbobAnimation.Play("RESET", 1f);
			}
		}
	}


	public void PlayJumpAnimation()
	{
		if (!_wasOnFloor && IsOnFloor()) // The player just landed
		{
			Vector3 facingDirection = Camera.GlobalTransform.Basis.X;
			Vector2 facingDirection2D = new Vector2(facingDirection.X, facingDirection.Z).Normalized();
			Vector2 velocity2D = new Vector2(Velocity.X, Velocity.Z).Normalized();

			// Compares velocity direction against the camera direction (via dot product) to determine which landing animation to play.
			int sideLanded = Mathf.RoundToInt(velocity2D.Dot(facingDirection2D));

			if (sideLanded > 0)
			{
				JumpAnimation.Play("land_right", 0.25f);
			}
			else if (sideLanded < 0)
			{
				JumpAnimation.Play("land_left", 0.25f);
			}
			else
			{
				JumpAnimation.Play("land_center", 0.25f);
			}
		}
	}

#endregion

#region Debug Menu
	public void UpdateDebugMenuPerFrame()
	{
		DebugPanel.AddProperty("FPS", Performance.GetMonitor(Performance.Monitor.TimeFps), 0);
		string status = _state;
		if (!IsOnFloor())
		{
			status += " in the air";
		}
		DebugPanel.AddProperty("State", status, 4);
	}

	public void UpdateDebugMenuPerTick()
	{
		// Big thanks to github.com/LorenzoAncora for the concept of the improved debug values
		Vector3 _currentSpeedD = GetRealVelocity();
		DebugPanel.AddProperty("Speed", _currentSpeedD.Snapped(0.001f), 1);
		DebugPanel.AddProperty("Target speed", _speed, 2);

		Vector3 cv = GetRealVelocity();
		Vector3 sv = cv.Snapped(0.001f);
		float[] vd = {
			sv.X,
			sv.Y,
			sv.Z	
		};

		string readableVelocity = $"X: {vd[0]} Y: {vd[1]} Z: {vd[2]}";
		DebugPanel.AddProperty("Velocity", readableVelocity, 3);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			MouseInput.X += ((InputEventMouseMotion)@event).Relative.X;
			MouseInput.Y += ((InputEventMouseMotion)@event).Relative.Y;
		}
		// Toggle debug menu
		else if (@event is InputEventKey)
		{
			if (((InputEventKey)@event).IsReleased())
			{
				// Where we're going, we don't need InputMap
				if ((int)((InputEventKey)@event).Keycode == 4194338) // F7
				{
					DebugPanel.Visible = !DebugPanel.Visible;
				}
			}
		}
	}

#endregion

#region Misc Functions
	public void ChangeReticle(string reticle)
	{
		// Yup, this function is kinda strange
		if (Reticle != null)
		{
			Reticle.QueueFree();
		}
		PackedScene scene = (PackedScene)ResourceLoader.Load(reticle);
		if (scene != null)
		{
			Reticle = (Control)scene.Instantiate();
			UserInterface.AddChild(Reticle);
		}
	}

	public void UpdateCameraFov()
	{
		if (_state == "sprinting")
		{
			Camera.Fov = Mathf.Lerp(Camera.Fov, 85.0f, 0.3f);
		}
		else
		{
			Camera.Fov = Mathf.Lerp(Camera.Fov, 75.0f, 0.3f);
		}
	}

	public void HandlePausing()
	{
		if (Input.IsActionJustPressed(Controls["pause"]))
		{
			// You may want another node to handle pausing, because this player may get paused too.
			switch (Input.MouseMode)
			{
				case Input.MouseModeEnum.Captured:
					Input.MouseMode = Input.MouseModeEnum.Visible;
					// get_tree().Paused = false;
					break;
				case Input.MouseModeEnum.Visible:
					Input.MouseMode = Input.MouseModeEnum.Captured;
					// get_tree().Paused = false;
					break;
			}
		}
	}
#endregion
}