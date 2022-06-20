using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonMapGeneration))]
public class DungeonGenerationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DungeonMapGeneration dunGen = (DungeonMapGeneration)target;

		if (DrawDefaultInspector())
		{

		}

		if (GUILayout.Button("Generate"))
		{
			dunGen.SetupScene();
		}
	}
}