using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Brush), true)]

public class BrushEditor : Editor
{
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

	public override void OnInspectorGUI()
	{
		Brush myBrush = (Brush)target;
		if (myBrush.IsActive())
			DrawDefaultInspector();

		if (ToggleButtonStyleNormal == null)
		{
			ToggleButtonStyleNormal = "Button";
			ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
			ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
		}

		GUIStyle style = myBrush.IsActive() ? ToggleButtonStyleToggled : ToggleButtonStyleNormal;
		if (GUILayout.Button("Use", style))
		{
			myBrush.Toggle();
		}
	}
}
