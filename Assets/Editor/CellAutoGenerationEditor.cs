using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CellAutoGeneration))]
public class CellAutoGenerationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		CellAutoGeneration cellGen = (CellAutoGeneration)target;

		if (DrawDefaultInspector())
		{

		}
		if (GUILayout.Button("Generate"))
		{
			cellGen.GenerateMapOnMesh();
		}
	}
}