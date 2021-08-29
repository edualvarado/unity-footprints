/****************************************************
 * File: BrushEditorPhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Physically-driven Footprints Generation for Real-Time Interactions between a Character and Deformable Terrains
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BrushPhysicalFootprintSphere), true)]

public class BrushEditorPhysicalFootprintSphere : Editor
{
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

	public override void OnInspectorGUI()
	{
		BrushPhysicalFootprintSphere myBrush = (BrushPhysicalFootprintSphere)target;

		// To start brush by default
		//if (!myBrush.IsActive() && Application.isPlaying)
		//	myBrush.Toggle();

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
