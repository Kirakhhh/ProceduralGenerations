using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerlinNoiseGeneration))]
public class PerlinNoiseGenerationEditor : Editor
{ 
	public override void OnInspectorGUI()
	{
		PerlinNoiseGeneration perlinGen = (PerlinNoiseGeneration)target;

		if (DrawDefaultInspector())
		{

		}

		if (GUILayout.Button("Generate"))
		{
			perlinGen.GenerateMap();
		}
	}
}