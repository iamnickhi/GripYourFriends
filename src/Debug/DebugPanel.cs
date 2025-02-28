using System;
using System.Linq;
using Godot;
namespace Debug;

public partial class DebugPanel : PanelContainer
{
	VBoxContainer mContainer = null;
    public override void _Process(double delta)
    {
        if (Visible)
        {
            // No code here in the original GDScript
            // but you can add any functionality if needed.
        }
    }
    public override void _Ready()
    {
		mContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
    }

    public void AddProperty(string title, object value, int order)
    {

        Node target = null;
        
        // Find child by title in the VBoxContainer
		if (mContainer != null)
		{
        	target = mContainer
            .GetChildren()
			.Cast<Node>()
            .FirstOrDefault(child => child.Name == title);
		}

        
        if (target == null)
        {
            target = new Label(); // Debug lines are of type Label
			if (mContainer != null)
			{
				mContainer.AddChild(target);
				target.Name = title;
				(target as Label).Text = $"{title}: {value}";
			}
        }
        else if (Visible)
        {
            (target as Label).Text = $"{title}: {value}";
			if (mContainer != null) mContainer.MoveChild(target, order);
        }
    }
}