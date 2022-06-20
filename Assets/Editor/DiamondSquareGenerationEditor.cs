using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DiamondSquareGeneration))]
public class DiamondSquareGenerationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DiamondSquareGeneration dsGen = (DiamondSquareGeneration)target;

		if (DrawDefaultInspector())
		{
			
		}
		if (GUILayout.Button("Generate"))
		{
			dsGen.GenerateMapOnTerrain();
		}
	}
}
